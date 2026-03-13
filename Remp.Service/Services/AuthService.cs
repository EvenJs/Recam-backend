using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;
using Remp.Service.DTOs.Auth;
using Remp.Service.Interfaces;


namespace Remp.Service.Services;

public class AuthService : IAuthService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IConfiguration _configuration;
  private readonly IUserActivityLogRepository _activityLogRepo;

  public AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
  {
    _userManager = userManager;
    _configuration = configuration;
  }

  public async Task<LoginResponse> LoginAsync(LoginRequest request)
  {
    var user = await _userManager.FindByEmailAsync(request.Email)
      ?? throw new NotFoundException("User not found.");

    var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
    if(!isValidPassword)
      throw new UnauthorizedAccessException("Invalid credentials.");

    var roles = await _userManager.GetRolesAsync(user);
    var role = roles.FirstOrDefault() ?? string.Empty;

    var token = GenerateJwtToken(user, role);

    return new LoginResponse
    {
      Token = token,
      UserId = user.Id,
      Email = user.Email ?? string.Empty,
      Role = role,
      ExpiresAt = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpiryHours"] ?? "24"))
    };
  }

  public async Task<string> GetCurrentUserAsync(string userId)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new NotFoundException("User not found.");

    return user.Email ?? string.Empty;
  }

  public async Task UpdatePasswordAsync(string userId, string oldPassword, string newPassword)
  {
    var user = await _userManager.FindByIdAsync(userId)
      ?? throw new NotFoundException("User not found.");

    var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    if (!result.Succeeded)
      throw new BadRequestException(result.Errors.First().Description);
  }

  private string GenerateJwtToken(ApplicationUser user, string role)
  {
    var jwtKey = _configuration["Jwt:Key"]
      ?? throw new InvalidOperationException("JWT key is not configured");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id),
      new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
      new Claim(ClaimTypes.Role, role),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
      issuer: _configuration["Jwt:Issuer"],
      audience: _configuration["Jwt: Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpiryHours"] ?? "24")),
      signingCredentials: credentials);
  
    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
