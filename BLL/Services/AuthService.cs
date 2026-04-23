using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BLL.Exceptions;
using BLL.Interfaces;
using BCrypt.Net;
using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using DTO.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BadRequestException("Full name is required.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new BadRequestException("Phone number is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        var exists = await _userRepository.ExistsByPhoneNumberAsync(request.PhoneNumber);
        if (exists)
            throw new BadRequestException("Phone number is already registered.");

        if (request.Role == RoleType.Farmer && string.IsNullOrWhiteSpace(request.DisplayName))
            throw new BadRequestException("Display name is required for farmer registration.");

        var user = new User
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        switch (request.Role)
        {
            case RoleType.Farmer:
                var farmer = new Farmer
                {
                    UserId = user.Id,
                    DisplayName = request.DisplayName!,
                    PrimaryCityId = request.PrimaryCityId,
                    IsProfileCompleted = false
                };
                await _context.Farmers.AddAsync(farmer);
                break;

            case RoleType.Customer:
                var customer = new Customer
                {
                    UserId = user.Id,
                    PrimaryCityId = request.PrimaryCityId
                };
                await _context.Customers.AddAsync(customer);
                break;

            case RoleType.Admin:
                var admin = new Admin
                {
                    UserId = user.Id
                };
                await _context.Admins.AddAsync(admin);
                break;

            default:
                throw new BadRequestException("Invalid role.");
        }

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = GenerateJwtToken(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
            throw new BadRequestException("Phone number or email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password is required.");

        var user = await _userRepository.GetByPhoneOrEmailAsync(request.Login);
        if (user is null)
            throw new BadRequestException("Invalid login or password.");

        if (!user.IsActive)
            throw new ForbiddenException("User account is deactivated.");

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
            throw new BadRequestException("Invalid login or password.");

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role,
            AccessToken = GenerateJwtToken(user)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new BadRequestException("JWT key is not configured.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}