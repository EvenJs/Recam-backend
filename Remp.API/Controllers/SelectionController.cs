using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Service.DTOs.CaseContact;
using Remp.Service.Interfaces;
using System.Security.Claims;

namespace Remp.API.Controllers;

[ApiController]
[Authorize]
public class SelectionController : ControllerBase
{
    private readonly IMediaAssetService _mediaAssetService;
    private readonly ICaseContactService _caseContactService;
    private readonly IListingCaseService _listingCaseService;

    public SelectionController(
        IMediaAssetService mediaAssetService,
        ICaseContactService caseContactService,
        IListingCaseService listingCaseService)
    {
        _mediaAssetService = mediaAssetService;
        _caseContactService = caseContactService;
        _listingCaseService = listingCaseService;
    }

    /// <summary>
    /// Agent selects display media for a listing case. Max 10 images. Agent only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="mediaIds">List of media asset IDs to select.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Media selection updated.</response>
    /// <response code="400">Validation failed or exceeds max 10 images.</response>
    /// <response code="403">Caller is not an Agent.</response>
    [HttpPut("listings/{id}/selected-media")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSelectedMedia(
        [FromRoute] int id,
        [FromBody] List<int> mediaIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        await _mediaAssetService.UpdateSelectedMediaAsync(id, mediaIds, userId);
        return Ok(ApiResponse<object>.Ok("Media selection updated successfully."));
    }

    /// <summary>
    /// Returns the finalised display media selection for a listing case.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>List of selected media assets.</returns>
    /// <response code="200">Selected media returned.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpGet("listings/{id}/final-selection")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinalSelection([FromRoute] int id)
    {
        var result = await _mediaAssetService.GetSelectedMediaAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Adds a contact to a listing case. Agent only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="request">Contact details.</param>
    /// <returns>Created contact.</returns>
    /// <response code="201">Contact added successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an Agent.</response>
    [HttpPost("listings/{id}/contacts")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(ApiResponse<CaseContactResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddContact(
        [FromRoute] int id,
        [FromBody] AddCaseContactRequest request)
    {
        var result = await _caseContactService.AddContactAsync(id, request);
        return StatusCode(201, ApiResponse<CaseContactResponse>.Created(result));
    }

    /// <summary>
    /// Returns all contacts for a listing case.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>List of contacts.</returns>
    /// <response code="200">Contact list returned.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpGet("listings/{id}/contacts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CaseContactResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContacts([FromRoute] int id)
    {
        var result = await _caseContactService.GetContactsByListingIdAsync(id);
        return Ok(ApiResponse<IEnumerable<CaseContactResponse>>.Ok(result));
    }

    /// <summary>
    /// Returns full preview data for a listing case showcase page.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>Listing preview data.</returns>
    /// <response code="200">Preview data returned.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpGet("listings/{id}/preview")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPreview([FromRoute] int id)
    {
        var result = await _listingCaseService.GetListingByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Generates a shareable link for a listing case.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>Shareable URL.</returns>
    /// <response code="200">Shareable link generated.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpPost("listings/{id}/publish")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishListing([FromRoute] int id)
    {
        // TODO: implement PublishListingAsync in IListingCaseService
        throw new NotImplementedException("Publish not yet implemented.");
    }

}
