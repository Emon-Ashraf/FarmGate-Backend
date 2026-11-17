using System.Text.Json;
using System.Text.RegularExpressions;
using BLL.Exceptions;
using BLL.Interfaces;
using SkiaSharp;

namespace FarmGate_Backend.Media;

// Only generated JPEGs are exposed. Sidecars and partial files remain outside the public route.
public sealed class FileImageStorage : IImageStorageService
{
    public const int MaxBytes = 5 * 1024 * 1024;
    private readonly SemaphoreSlim decoding = new(2, 2);
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string? root;
    private readonly string? baseUrl;
    public bool Enabled => root != null && baseUrl != null;
    public FileImageStorage(IConfiguration configuration, IHostEnvironment? environment = null)
    {
        root = configuration["Images:Root"];
        baseUrl = configuration["Images:PublicBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(root) && string.IsNullOrWhiteSpace(baseUrl)) { root = null; baseUrl = null; return; }
        if (root == null || !Path.IsPathFullyQualified(root) ||
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("https" or "http") || uri.Query != "" || uri.Fragment != "" ||
            !uri.AbsolutePath.EndsWith("/api/images", StringComparison.Ordinal))
            throw new InvalidOperationException("Configure an absolute Images:Root and an HTTP(S) Images:PublicBaseUrl ending in /api/images.");
        if (environment?.IsProduction() == true && uri.Scheme != "https")
            throw new InvalidOperationException("Production image URLs must use HTTPS.");
        Directory.CreateDirectory(root);
    }

    public record StoredImage(string Url);
    public record Ownership(long OwnerId, string Purpose, DateTime CreatedAt, bool Referenced = false);

    public async Task<StoredImage> SaveAsync(Stream input, string mediaType, long ownerId, string purpose, CancellationToken ct)
    {
        if (!Enabled) throw new InvalidOperationException("Image storage is not configured.");
        if (mediaType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new BadRequestException("Choose a JPEG, PNG, or WebP image.");
        using var buffer = new MemoryStream();
        var bytes = new byte[81920]; int count;
        while ((count = await input.ReadAsync(bytes, ct)) > 0)
        {
            if (buffer.Length + count > MaxBytes) throw new BadRequestException("Image exceeds 5 MB.");
            await buffer.WriteAsync(bytes.AsMemory(0, count), ct);
        }
        buffer.Position = 0;
        await decoding.WaitAsync(ct);
        try
        {
            using var codec = SKCodec.Create(buffer) ?? throw new BadRequestException("Unsupported or invalid image content.");
            var actual = codec.EncodedFormat switch {
                SKEncodedImageFormat.Jpeg => "image/jpeg", SKEncodedImageFormat.Png => "image/png",
                SKEncodedImageFormat.Webp => "image/webp", _ => null
            };
            if (actual != mediaType) throw new BadRequestException("Image content does not match its media type.");
            if (codec.Info.Width <= 0 || codec.Info.Height <= 0 || (long)codec.Info.Width * codec.Info.Height > 24_000_000)
                throw new BadRequestException("Image dimensions exceed the 24 megapixel limit.");
            using var decoded = new SKBitmap(new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
            if (codec.GetPixels(decoded.Info, decoded.GetPixels()) != SKCodecResult.Success)
                throw new BadRequestException("Invalid or truncated image content.");
            var origin = (int)codec.EncodedOrigin;
            var w = decoded.Width; var h = decoded.Height;
            var orientedWidth = origin >= 5 ? h : w;
            var orientedHeight = origin >= 5 ? w : h;
            var max = purpose == "product" ? 1600 : 800;
            var ratio = Math.Min(1d, (double)max / Math.Max(orientedWidth, orientedHeight));
            var targetWidth = Math.Max(1, (int)Math.Round(orientedWidth * ratio));
            var targetHeight = Math.Max(1, (int)Math.Round(orientedHeight * ratio));
            using var normalized = new SKBitmap(targetWidth, targetHeight);
            using (var canvas = new SKCanvas(normalized)) {
                canvas.Clear(SKColors.White);
                canvas.Scale((float)targetWidth / orientedWidth, (float)targetHeight / orientedHeight);
                var orientation = OrientationMatrix(origin, w, h);
                canvas.Concat(in orientation);
                using var paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
                canvas.DrawBitmap(decoded, 0, 0, paint);
            }
            using var encoded = normalized.Encode(SKEncodedImageFormat.Jpeg, 85)
                ?? throw new IOException("Image encoding failed.");
            var id = Guid.NewGuid().ToString("N");
            var file = Path.Combine(root!, id + ".jpg");
            var partial = file + ".partial";
            try
            {
                await using (var target = File.Create(partial)) { encoded.SaveTo(target); await target.FlushAsync(ct); }
                await File.WriteAllTextAsync(Path.Combine(root!, id + ".json"), JsonSerializer.Serialize(new Ownership(ownerId, purpose, DateTime.UtcNow)), ct);
                File.Move(partial, file);
                return new StoredImage(baseUrl + "/" + id + ".jpg");
            }
            catch
            {
                File.Delete(partial); File.Delete(file); File.Delete(Path.Combine(root!, id + ".json"));
                throw;
            }
        }
        finally { decoding.Release(); }
    }

    public static SKMatrix OrientationMatrix(int origin, int width, int height) => origin switch {
        2 => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),
        3 => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1),
        4 => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),
        5 => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
        6 => new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1),
        7 => new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1),
        8 => new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1),
        _ => SKMatrix.Identity
    };

    public string? GetPath(string name) => Enabled && Regex.IsMatch(name, "^[a-f0-9]{32}\\.jpg$")
        ? Path.Combine(root!, name) : null;

    public async Task ValidateReferenceAsync(string? url, long ownerId, string purpose)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") || url.Length > 1000)
            throw new BadRequestException("Image URL must be an absolute HTTP(S) URL of at most 1000 characters.");
        // Keep legacy externally hosted URLs compatible; managed URLs require ownership.
        if (!Enabled) return;
        var prefix = new Uri(baseUrl + "/");
        if (!string.Equals(uri.Scheme, prefix.Scheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.IdnHost, prefix.IdnHost, StringComparison.OrdinalIgnoreCase) ||
            uri.Port != prefix.Port || !uri.AbsolutePath.StartsWith(prefix.AbsolutePath, StringComparison.Ordinal)) return;
        if (uri.Query != "" || uri.Fragment != "") throw new BadRequestException("Managed image URLs cannot contain query strings or fragments.");
        await gate.WaitAsync();
        try {
            var name = Uri.UnescapeDataString(uri.AbsolutePath[prefix.AbsolutePath.Length..]);
            var path = GetPath(name);
            if (path == null || !File.Exists(path)) throw new BadRequestException("Uploaded image is missing. Upload again.");
            var metadata = await File.ReadAllTextAsync(Path.ChangeExtension(path, ".json"));
            var owner = JsonSerializer.Deserialize<Ownership>(metadata);
            if (owner?.OwnerId != ownerId || owner.Purpose != purpose)
                throw new ForbiddenException("This image does not belong to this user and image purpose.");
            // Claim BEFORE saving a reference. Keep claimed images even if a later DB save fails;
            // cleanup must never race a product/profile write and delete a valid image.
            if (!owner.Referenced)
            {
                var sidecar = Path.ChangeExtension(path, ".json");
                await File.WriteAllTextAsync(sidecar + ".partial", JsonSerializer.Serialize(owner with { Referenced = true }));
                File.Move(sidecar + ".partial", sidecar, true);
            }
        } finally { gate.Release(); }
    }

    public async Task CleanupPendingAsync(CancellationToken ct)
    {
        if (!Enabled) return;
        await gate.WaitAsync(ct);
        try {
            foreach (var sidecar in Directory.EnumerateFiles(root!, "*.json"))
            {
                ct.ThrowIfCancellationRequested();
                var owner = JsonSerializer.Deserialize<Ownership>(await File.ReadAllTextAsync(sidecar, ct));
                if (owner is { Referenced: false } && owner.CreatedAt < DateTime.UtcNow.AddDays(-7))
                {
                    File.Delete(Path.ChangeExtension(sidecar, ".jpg"));
                    File.Delete(sidecar);
                }
            }
        } finally { gate.Release(); }
    }
}
