using DTO.Admin;

namespace BLL.Interfaces;

public interface IAdminService
{
    Task<List<AdminIssueListItemDto>> GetOpenIssuesAsync();
    Task DeactivateUserAsync(long userId, long targetUserId, AdminModerateUserDto request);
    Task DeactivateProductAsync(long userId, long productId, AdminModerateProductDto request);
}