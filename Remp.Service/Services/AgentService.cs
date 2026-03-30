using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Remp.Common.Exceptions;
using Remp.Common.Helpers;
using Remp.Models.Constants;
using Remp.Models.Entities;
using Remp.Repository.Common;
using Remp.Service.DTOs.Agent;
using Remp.Service.DTOs.Auth;
using Remp.Service.Interfaces;
namespace Remp.Service.Services;

public class AgentService : IAgentService
{
  private readonly IUnitOfWork _uintOfWork;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IEmailService _emailService;
  private readonly IMapper _mapper;

  public AgentService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IMapper mapper)
  {
    _uintOfWork = unitOfWork;
    _userManager = userManager;
    _emailService = emailService;
    _mapper = mapper;
  }

  public async Task<AgentResponse> CreateAgentAsync(RegisterAgentRequest request, string photographyCompanyId)
  {
    var existingUser = await _userManager.FindByEmailAsync(request.Email);
    if (existingUser != null)
      throw new ConflictException("An account with this email already exists.");

    var password = GenerateRandomPassword();

    var agent = new Agent
    {
      UserName = request.Email,
      Email = request.Email,
      AgentFirstName = request.AgentFirstName,
      AgentLastName = request.AgentLastName,
      CompanyName = request.CompanyName,
      CreatedAt = DateTime.UtcNow
    };

    var result = await _userManager.CreateAsync(agent, password);
    if (!result.Succeeded)
      throw new BadRequestException(result.Errors.First().Description);

    await _userManager.AddToRoleAsync(agent, Roles.Agent);

    var agentPhotographyCompany = new AgentPhotographyCompany
    {
      AgentId = agent.Id,
      PhotographyCompanyId = photographyCompanyId
    };

    await _uintOfWork.AgentPhotographyCompanies.AddAsync(agentPhotographyCompany);
    await _uintOfWork.SaveChangesAsync();

    await _emailService.SendEmailAsync(
      request.Email,
      "Your Remp Account Credentials",
      $"Welcome to Remp! Your login email is {request.Email} and your temporary password is: {password}"
    );

    return _mapper.Map<AgentResponse>(agent);
  }

  public async Task<IEnumerable<AgentResponse>> GetAgentsByCompanyAsync(string photographyCompanyId)
  {
    var agents = await _uintOfWork.Agents.GetByPhotographyCompanyIdAsync(photographyCompanyId);
    return _mapper.Map<IEnumerable<AgentResponse>>(agents);
  }

  public async Task<IEnumerable<AgentResponse>> GetAgentByEmailAsync(string email)
  {
    var agent = await _uintOfWork.Agents.GetByEmailAsync(email)
      ?? throw new NotFoundException($"Agent with email {email} not found.");

    return _mapper.Map<IEnumerable<AgentResponse>>(agent);
  }

  public async Task LinkAgentToCompanyAsync(string agentId, string photographyCompanyId)
  {
    var exists = await _uintOfWork.AgentPhotographyCompanies.ExistsAsync(agentId, photographyCompanyId);
    if (exists)
      throw new ConflictException("Agent is already linked to this company.");

    var link = new AgentPhotographyCompany
    {
      AgentId = agentId,
      PhotographyCompanyId = photographyCompanyId
    };

    await _uintOfWork.AgentPhotographyCompanies.AddAsync(link);
    await _uintOfWork.SaveChangesAsync();
  }

  public Task<(IEnumerable<AgentResponse> Items, int TotalCount)> GetAllUsersAsync(int page, int pageSize)
  {
    var users = _userManager.Users
      .Where(u => !u.IsDeleted)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToList();

    var totalCount = _userManager.Users.Count(u => !u.IsDeleted);

    var mapped = _mapper.Map<IEnumerable<AgentResponse>>(users);
    return Task.FromResult((mapped, totalCount));
  }

  private static string GenerateRandomPassword()
  {
    const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    const string lowercase = "abcdefghijkmnpqrstuvwxyz";
    const string digits = "23456789";
    const string special = "!@#$%";
    const string allChars = uppercase + lowercase + digits + special;

    var random = new Random();
    string password;

    do
    {
      var chars = new List<char>
        {
            uppercase[random.Next(uppercase.Length)],
            lowercase[random.Next(lowercase.Length)],
            digits[random.Next(digits.Length)],
            special[random.Next(special.Length)]
        };

      for (int i = 0; i < 8; i++)
        chars.Add(allChars[random.Next(allChars.Length)]);

      // Fisher-Yates shuffle
      for (int i = chars.Count - 1; i > 0; i--)
      {
        int j = random.Next(i + 1);
        (chars[i], chars[j]) = (chars[j], chars[i]);
      }

      password = new string(chars.ToArray());

    } while (!password.Any(c => special.Contains(c))); // retry if no special char after shuffle

    return password;
  }

}
