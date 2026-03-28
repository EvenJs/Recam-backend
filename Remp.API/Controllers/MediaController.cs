using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Service.DTOs.MediaAsset;
using Remp.Service.Interfaces;
using System.Security.Claims;

namespace Remp.API.Controllers;

[ApiController]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaAssetService _mediaAssetService;

    public MediaController(IMediaAssetService mediaAssetService)
    {
        _mediaAssetService = mediaAssetService;
    }

    /// <summary>
    /// Uploads media files to a listing case. Admin only.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="request">Media upload request.</param>
    /// <returns>Uploaded media asset details.</returns>
    /// <response code="201">Media uploaded successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not an Admin.</response>
    [HttpPost("listings/{id}/media")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<MediaAssetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadMedia(
        [FromRoute] int id,
        [FromForm] UploadMediaRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        var result = await _mediaAssetService.UploadMediaAsync(id, request, userId);
        return StatusCode(201, ApiResponse<IEnumerable<MediaAssetResponse>>.Created(result));
    }

    /// <summary>
    /// Returns all media for a listing case grouped by type.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <returns>List of media assets.</returns>
    /// <response code="200">Media list returned.</response>
    /// <response code="404">Listing case not found.</response>
    [HttpGet("listings/{id}/media")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MediaAssetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListingMedia([FromRoute] int id)
    {
        var result = await _mediaAssetService.GetMediaByListingIdAsync(id);
        return Ok(ApiResponse<IEnumerable<MediaAssetResponse>>.Ok(result));
    }

    /// <summary>
    /// Deletes a media file. Admin only.
    /// </summary>
    /// <param name="id">Media asset ID.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Media deleted successfully.</response>
    /// <response code="404">Media not found.</response>
    [HttpDelete("media/{id}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        await _mediaAssetService.DeleteMediaAsync(id, userId);
        return Ok(ApiResponse<object>.Ok("Media deleted successfully."));
    }

    /// <summary>
    /// Sets the hero image for a listing case.
    /// </summary>
    /// <param name="id">Listing case ID.</param>
    /// <param name="mediaId">Media asset ID to set as hero image.</param>
    /// <returns>Success message.</returns>
    /// <response code="200">Hero image set successfully.</response>
    /// <response code="404">Listing case or media not found.</response>
    [HttpPut("listings/{id}/cover-image")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetHeroImage(
        [FromRoute] int id,
        [FromBody] int mediaId)
    {
        await _mediaAssetService.SetHeroImageAsync(id, mediaId);
        return Ok(ApiResponse<object>.Ok("Hero image set successfully."));
    }


    [HttpGet("media/{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DownloadMedia([FromRoute] int id)
    {
        // TODO: implement DownloadMediaAsync in IMediaAssetService
        throw new NotImplementedException("Download not yet implemented.");
    }

    [HttpGet("listings/{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DownloadAllMedia([FromRoute] int id)
    {
        // TODO: implement DownloadAllMediaAsZipAsync in IMediaAssetService
        throw new NotImplementedException("ZIP download not yet implemented.");
    }
}
