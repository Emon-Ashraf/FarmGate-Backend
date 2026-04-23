using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Repositories;

namespace FarmGate_Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IPickupLocationRepository, PickupLocationRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IIssueReportRepository, IssueReportRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IPickupLocationService, PickupLocationService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IIssueReportService, IssueReportService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}