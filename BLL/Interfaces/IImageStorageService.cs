namespace BLL.Interfaces;

public interface IImageStorageService
{
    Task ValidateReferenceAsync(string? url, long ownerId, string purpose);
}
