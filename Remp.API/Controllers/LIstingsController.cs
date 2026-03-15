using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Service.DTOs.ListingCase;
using Remp.Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Remp.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IListingCaseService _listingCaseService;

    public ListingsController(IListingCaseService listingCaseService)
    {
        _listingCaseService = listingCaseService;
    }

        /// <summary>
    /// Creates a new listing case. Admin only.
    /// </summary>
    /// <param name="request">Listing case details.</param>
    /// <returns>Created listing case.</returns>
    /// <response code="201">Listing case created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an Admin.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ListingCaseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateListingCase([FromBody] CreateListingCaseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        var result = await _listingCaseService.CreateListingCaseAsync(request, userId);
        return StatusCode(201, ApiResponse<ListingCaseResponse>.Created(result));
    }

    /// <summary>
    /// Returns paginated listing cases. Admin sees own listings, Agent sees assigned listings.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10).</param>
    /// <returns>Paginated listing case list.</returns>
    /// <response code="200">Listing list returned.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ListingCaseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListing(
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        
        var isAdmin = User.IsInRole(Roles.Admin);

        var adminId = isAdmin ? userId : null;
        var agentId = isAdmin ? null : userId;


        var result = await _listingCaseService.GetListingsAsync(userId, agentId, status, page, pageSize);
        return Ok(ApiResponse<IEnumerable<ListingCaseResponse>>.Ok(result.Items));
    }

    /// <summary>
    /// Returns a single listing case by ID.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>Listing case details.</returns>
    /// <response code="200">Listing case returned.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ListingCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListingById([FromRoute] int id)
    {
        var result = await _listingCaseService.GetListingByIdAsync(id);
        return Ok(ApiResponse<ListingCaseResponse>.Ok(result));
    }

    /// <summary>
    /// Updates a listing case. Admin only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="request">Updated listing details.</param>
    /// <returns>Updated listing case.</returns>
    /// <response code="200">Listing case updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ListingCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListing(
        [FromRoute] int id,
        [FromBody] UpdateListingCaseRequest request)
    {
        var result = await _listingCaseService.UpdateListingCaseAsync(id, request);
        return Ok(ApiResponse<ListingCaseResponse>.Ok(result));
    }

    /// <summary>
    /// Soft deletes a listing case. Admin only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>No content.</returns>
    /// <response code="200">Listing case deleted.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteListing([FromRoute] int id)
    {
        await _listingCaseService.DeleteListingCaseAsync(id);
        return Ok(ApiResponse<object>.Ok("Listing case deleted successfully."));
    }

    /// <summary>
    /// Updates listing case status. Admin only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="status">New status value.</param>
    /// <returns>Updated listing case.</returns>
    /// <response code="200">Status updated.</response>
    /// <response code="400">Invalid status transition.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ListingCaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListingStatus(
        [FromRoute] int id,
        [FromBody] int status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token");
        
        await _listingCaseService.UpdateListingStatusAsync(id, status, userId);
        return Ok(ApiResponse<object>.Ok("Listing status updated successfully."));
    }

    /// <summary>
    /// Assigns an agent to a listing case. Admin only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="agentId">Agent ID to assign.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Agent assigned successfully.</response>
    /// <response code="404">Listing case or agent not found.</response>
    [HttpPost("{id}/assign-agent")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAgent(
        [FromRoute] int id,
        [FromBody] string agentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        await _listingCaseService.AssignAgentToListingAsync(id, agentId, userId);
        return Ok(ApiResponse<object>.Ok("Agent assign successfully."));
    }
}
