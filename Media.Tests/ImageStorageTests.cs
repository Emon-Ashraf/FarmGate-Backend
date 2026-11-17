using BLL.Exceptions;
using FarmGate_Backend.Media;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using Xunit;

public sealed class ImageStorageTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    readonly FileImageStorage storage;
    public ImageStorageTests() => storage = new FileImageStorage(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
        ["Images:Root"] = root, ["Images:PublicBaseUrl"] = "https://images.example/api/images"
    }).Build());
    static async Task<MemoryStream> ImageBytes()
    {
        var data = new MemoryStream(); using var image = new SKBitmap(20, 10);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 85); encoded.SaveTo(data); data.Position = 0; await Task.CompletedTask; return data;
    }
    [Fact] public async Task UploadNormalizesAndEnforcesOwnershipAndPurpose()
    {
        using var data = await ImageBytes();
        var result = await storage.SaveAsync(data, "image/jpeg", 7, "product", default);
        var name = result.Url.Split('/').Last();
        Assert.Matches("^[a-f0-9]{32}\\.jpg$", name);
        using var decoded = SKBitmap.Decode(storage.GetPath(name)!);
        Assert.Equal(20, decoded.Width); // must not upscale
        await storage.ValidateReferenceAsync(result.Url, 7, "product");
        await Assert.ThrowsAsync<ForbiddenException>(() => storage.ValidateReferenceAsync(result.Url, 8, "product"));
        await Assert.ThrowsAsync<ForbiddenException>(() => storage.ValidateReferenceAsync(result.Url, 7, "avatar"));
        Assert.Null(storage.GetPath("../appsettings.json"));
        Assert.Null(storage.GetPath(name + ".json"));
    }
    [Theory] [InlineData("image/png")] [InlineData("image/svg+xml")]
    public async Task SpoofedMimeRejected(string mime)
    {
        using var data = await ImageBytes();
        await Assert.ThrowsAsync<BadRequestException>(() => storage.SaveAsync(data, mime, 7, "avatar", default));
        Assert.Empty(Directory.GetFiles(root));
    }
    [Fact] public async Task CorruptAndOversizeContentRejected()
    {
        using var bad = new MemoryStream(new byte[] {1,2,3});
        await Assert.ThrowsAsync<BadRequestException>(() => storage.SaveAsync(bad, "image/jpeg", 7, "avatar", default));
        using var large = new MemoryStream(new byte[FileImageStorage.MaxBytes + 1]);
        await Assert.ThrowsAsync<BadRequestException>(() => storage.SaveAsync(large, "image/jpeg", 7, "avatar", default));
    }
    [Fact] public async Task PendingCleanupKeepsClaimedImagesAndRemovesAbandonedUploads()
    {
        using var first = await ImageBytes(); using var second = await ImageBytes();
        var keep = await storage.SaveAsync(first, "image/jpeg", 7, "avatar", default);
        var remove = await storage.SaveAsync(second, "image/jpeg", 7, "avatar", default);
        foreach(var sidecar in Directory.GetFiles(root, "*.json"))
            await File.WriteAllTextAsync(sidecar, System.Text.Json.JsonSerializer.Serialize(new FileImageStorage.Ownership(7,"avatar",DateTime.UtcNow.AddDays(-8))));
        await storage.ValidateReferenceAsync(keep.Url, 7, "avatar");
        await storage.CleanupPendingAsync(default);
        Assert.True(File.Exists(storage.GetPath(keep.Url.Split('/').Last())));
        Assert.False(File.Exists(storage.GetPath(remove.Url.Split('/').Last())));
    }
    [Fact] public async Task LegacyUrlsWorkButContentUrisAreRejected()
    {
        await storage.ValidateReferenceAsync("https://legacy.example/photo.jpg", 7, "avatar");
        await Assert.ThrowsAsync<BadRequestException>(() => storage.ValidateReferenceAsync("content://photo/1", 7, "avatar"));
    }
    public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, true); }
}
