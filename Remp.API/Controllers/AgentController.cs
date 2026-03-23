using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Service.DTOs.Agent;
using Remp.Service.DTOs.Auth;
using Remp.Service.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Remp.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// Creates a new agent and sends credentials via email. Admin only.
    /// </summary>
    /// <param name="request">Agent registration details.</param>
    /// <returns>Created agent details.</returns>
    /// <response code="201">Agent created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an Admin.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<AgentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAgent([FromBody] RegisterAgentRequest request)
    {
        var companyId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        var result = await _agentService.CreateAgentAsync(request, companyId);
        return StatusCode(201, ApiResponse<AgentResponse>.Created(result));
    }

    /// <summary>
    /// Returns all agents under the current Admin. Admin only.
    /// </summary>
    /// <returns>List of agents.</returns>
    /// <response code="200">Agent list returned.</response>
    /// <response code="403">Caller is not an Admin.</response>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AgentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAgentsByCompany()
    {
        var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token");

        var result = await _agentService.GetAgentsByCompanyAsync(companyId);
        return Ok(ApiResponse<IEnumerable<AgentResponse>>.Ok(result));
    }

    /// <summary>
    /// Searches for an agent by exact email match. Admin only.
    /// </summary>
    /// <param name="email">Agent email address.</param>
    /// <returns>Matching agent details.</returns>
    /// <response code="200">Agent found.</response>
    /// <response code="404">Agent not found.</response>
    [HttpGet("search")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<AgentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchAgentByEmail([FromQuery] string email)
    {
        var result = await _agentService.GetAgentByEmailAsync(email);
        return Ok(ApiResponse<IEnumerable<AgentResponse>>.Ok(result));
    }

    /// <summary>
    /// Links an agent to a photography company. Admin only.
    /// </summary>
    /// <param name="id">Agent ID.</param>
    /// <returns>No content.</returns>
    /// <response code="200">Agent linked successfully.</response>
    /// <response code="400">Association already exists.</response>
    /// <response code="403">Caller is not an Admin.</response>
    [HttpPost("{id}/photography-company")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LinkAgentToCompany([FromRoute] string id)
    {
        var companyId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("Invalid token");

        await _agentService.LinkAgentToCompanyAsync(id, companyId);
        return Ok(ApiResponse<object>.Ok("Agent linked to company successfully."));
    }
}
