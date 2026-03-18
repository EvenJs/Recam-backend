using Moq;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Remp.Service.Services;
using Remp.Service.DTOs.Auth;
using Remp.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Remp.Models.Constants;

namespace Remp.Tests;


public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("test-secrete-key-at-least-32-chars!!");
        _configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("RempAPI");
        _configMock.Setup(c => c["JwtSettings:Audience"]).Returns("RempClient");
        _configMock.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("60");

        _authService = new AuthService(_userManagerMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var user = new PhotographyCompany
        {
            Id = "user-123",
            Email = "admin@remp.com",
            UserName = "admin@remp.com"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("admin@remp.com")).ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Admin@123!")).ReturnsAsync(true);

        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> {"PhotographyCompany"});

        var request = new LoginRequest
        {
            Email = "admin@remp.com",
            Password = "Admin@123!"
        };

        var result = await _authService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal("admin@remp.com", result.Email);
        Assert.Equal(Roles.Admin, result.Role);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var request = new LoginRequest
        {
            Email = "notfound@remp.com",
            Password = "password"
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new PhotographyCompany
        {
            Id = "user-123",
            Email = "admin@remp.com"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("admin@remp.com")).ReturnsAsync(user);

        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrongPassword")).ReturnsAsync(false);

        var request = new LoginRequest
        {
            Email = "admin@remp.com",
            Password = "wrongPassword"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
    }
}
