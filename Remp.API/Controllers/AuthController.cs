using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Service.DTOs.Auth;
using Remp.Service.Interfaces;

namespace Remp.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
   private readonly IAuthService _authService;

   public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and user info.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }
    
    /// <summary>
    /// Returns the currently authenticated user's info.
    /// </summary>
    /// <returns>Current user details.</returns>
    /// <response code="200">User info returned.</response>
    /// <response code="401">Token missing or invalid.</response>  
    [HttpGet("/users/me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var result = await _authService.GetCurrentUserAsync(User);
        return Ok(ApiResponse<object>.Ok(result));
    }
    

    /// <summary>
    /// Updates the current user's password.
    /// </summary>
    /// <param name="request">Old and new password.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Password updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Token missing or invalid.</response>
    [HttpPut("/users/me/password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        await _authService.UpdatePasswordAsync(User, request);
        return Ok(ApiResponse<object>.Ok(200, "Password updated successfully"));
    }
}
