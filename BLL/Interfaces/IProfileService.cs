using DTO.Profile;

namespace BLL.Interfaces;

public interface IProfileService
{
    Task<object> GetMyProfileAsync(long userId);

    Task<FarmerProfileDto> UpdateFarmerProfileAsync(
        long userId,
        UpdateFarmerProfileDto request
    );

    Task<CustomerProfileDto> UpdateCustomerProfileAsync(
        long userId,
        UpdateCustomerProfileDto request
    );
}