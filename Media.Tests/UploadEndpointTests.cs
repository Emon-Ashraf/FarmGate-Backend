using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SkiaSharp;
using System.Net;
using System.Security.Claims;
using DAL.Data;
using DAL.Enums;
using DAL.Models;
using FarmGate_Backend.Controllers;
using FarmGate_Backend.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class UploadEndpointTests
{
    [Fact] public async Task AnonymousUploadIsChallengedByRealAuthenticationMiddleware()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string,string?> {
                ["Jwt:Key"] = "test-only-key-at-least-thirty-two-characters", ["Images:Root"] = null, ["Images:PublicBaseUrl"] = null
            })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var form = new MultipartFormDataContent(); form.Add(new ByteArrayContent(new byte[] {1}), "file", "x.jpg");
        var response = await client.PostAsync("/api/images/avatar", form);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact] public async Task AuthenticatedMultipartUploadCanBeReadAndPersistedThroughExistingProfileApi()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        const string key = "test-only-key-at-least-thirty-two-characters";
        try {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string,string?> {
                    ["Jwt:Key"] = key, ["Jwt:Issuer"] = "test", ["Jwt:Audience"] = "test",
                    ["Images:Root"] = root, ["Images:PublicBaseUrl"] = "https://localhost/api/images"
                }));
                builder.ConfigureServices(services => {
                    services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>("Bearer", options => {
                        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
                        options.TokenValidationParameters.ValidIssuer = "test";
                        options.TokenValidationParameters.ValidAudience = "test";
                    });
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(root));
                });
            });
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            using (var scope = factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = new User { Id = 1, Role = RoleType.Customer, FullName = "Test", PhoneNumber = "123", PasswordHash = "test" };
                db.Customers.Add(new Customer { UserId = 1, User = user }); await db.SaveChangesAsync();
            }
            var token = new JwtSecurityToken("test", "test", new[] { new Claim(ClaimTypes.NameIdentifier,"1"), new Claim(ClaimTypes.Role,"Customer") },
                expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
            client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
            using var image = new SKBitmap(20, 10); using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 85);
            using var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(encoded.ToArray()); content.Headers.ContentType = new("image/jpeg");
            form.Add(content, "file", "../../untrusted.jpg");
            var response = await client.PostAsync("/api/images/avatar", form);
            Assert.True(response.IsSuccessStatusCode, $"Upload status {response.StatusCode}: {await response.Content.ReadAsStringAsync()} {response.Headers.WwwAuthenticate}");
            var uploaded = await response.Content.ReadFromJsonAsync<FileImageStorage.StoredImage>();
            Assert.NotNull(uploaded);
            var read = await client.GetAsync(uploaded.Url); read.EnsureSuccessStatusCode();
            Assert.Equal("image/jpeg", read.Content.Headers.ContentType!.MediaType);
            var saved = await client.PutAsJsonAsync("/api/profile/customer", new { profileImageUrl = uploaded.Url });
            saved.EnsureSuccessStatusCode();
            using var result = System.Text.Json.JsonDocument.Parse(await saved.Content.ReadAsStringAsync());
            Assert.Equal(uploaded.Url, result.RootElement.GetProperty("profileImageUrl").GetString());
            // Customer may upload an avatar, but cannot upload a product image.
            using var denied = new MultipartFormDataContent();
            denied.Add(new ByteArrayContent(encoded.ToArray()), "file", "image.jpg");
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/images/product", denied)).StatusCode);
        } finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Theory] [InlineData(RoleType.Customer, "product")] [InlineData(RoleType.Farmer, "other")]
    public async Task ProductRoleAndUnknownPurposeAreRejected(RoleType role, string purpose)
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Users.Add(new User { Id = 1, Role = role, FullName = "Test", PhoneNumber = "123", PasswordHash = "test", IsActive = true });
        await db.SaveChangesAsync();
        var storage = new FileImageStorage(new ConfigurationBuilder().Build());
        var controller = new ImagesController(storage, db, NullLogger<ImagesController>.Instance) {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "test"));
        using var bytes = new MemoryStream(new byte[] {1});
        var result = await controller.Upload(purpose, new FormFile(bytes, 0, 1, "file", "../../x.jpg"), default);
        if (purpose == "product") Assert.IsType<ForbidResult>(result);
        else Assert.Equal(400, Assert.IsType<ObjectResult>(result).StatusCode);
    }
}
