using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Service.Interfaces;


namespace Remp.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    // GET /users will be implemented in a refactor PR
    // when GetAllUsersAsync is added to IAgentService
}