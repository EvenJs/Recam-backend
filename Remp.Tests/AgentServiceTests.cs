using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Repository.Common;
using Remp.Repository.Interfaces;
using Remp.Service.DTOs.Agent;
using Remp.Service.DTOs.Auth;
using Remp.Service.Interfaces;
using Remp.Service.Services;

namespace Remp.Tests;

public class AgentServiceTests
{
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly Mock<IMapper> _mapperMock;
  private readonly Mock<IAgentRepository> _agentRepoMock;
  private readonly Mock<IAgentPhotographyCompanyRepository> _agentCompanyRepoMock;
  private readonly AgentService _service;

  public AgentServiceTests()
  {
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _userManagerMock = new Mock<UserManager<ApplicationUser>>(
      Mock.Of<IUserStore<ApplicationUser>>(),
      null, null, null, null, null, null, null, null);
    _emailServiceMock = new Mock<IEmailService>();
    _mapperMock = new Mock<IMapper>();
    _agentRepoMock = new Mock<IAgentRepository>();
    _agentCompanyRepoMock = new Mock<IAgentPhotographyCompanyRepository>();

    _unitOfWorkMock.Setup(u => u.Agents).Returns(_agentRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.AgentPhotographyCompanies).Returns(_agentCompanyRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

    _service = new AgentService(
      _unitOfWorkMock.Object,
      _userManagerMock.Object,
      _emailServiceMock.Object,
      _mapperMock.Object);
  }

  [Fact]
  public async Task CreateAgentAsync_DuplicateEmail_ThrowConflictException()
  {
    var existingUser = new Agent { Email = "agent@remp.com" };

    _userManagerMock.Setup(m =>m.FindByEmailAsync("agent@remp.com")).ReturnsAsync(existingUser);

    var request = new RegisterAgentRequest
    {
      Email = "agent@remp.com",
      AgentFirstName = "John",
      AgentLastName = "Doe",
      CompanyName = "Test Co"
    };

    await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAgentAsync(request, "company-123"));
  }

  [Fact]
  public async Task CreateAgentAsync_ValidRequest_ReturnsAgentResponse()
  {
    _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

    _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<Agent>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<Agent>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

    _agentCompanyRepoMock.Setup(r => r.AddAsync(It.IsAny<AgentPhotographyCompany>())).Returns(Task.CompletedTask);

    _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

    var agentResponse = new AgentResponse { Email = "newagent@remp.com" };
    _mapperMock.Setup(m => m.Map<AgentResponse>(It.IsAny<Agent>())).Returns(agentResponse);

    var request = new RegisterAgentRequest
    {
      Email = "newagent@remp.com",
      AgentFirstName = "Jane",
      AgentLastName = "Doe",
      CompanyName = "Test Co"
    };

    var result = await _service.CreateAgentAsync(request, "company-123");

    Assert.NotNull(result);
    Assert.Equal("newagent@remp.com", result.Email);
  }

  [Fact]
  public async Task LinkAgentToCompanyAsync_AlreadyLinked_ThrowsConflictException()
  {
    _agentCompanyRepoMock.Setup(r => r.ExistsAsync("agent-123", "company-123")).ReturnsAsync(true);

    await Assert.ThrowsAsync<ConflictException>(() => _service.LinkAgentToCompanyAsync("agent-123","company-123"));
  }

  [Fact]
  public async Task GetAgentByEmailAsync_NotFound_ThrowsNotFoundException()
  {
    _agentRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Agent?)null);

    await Assert.ThrowsAsync<Remp.Common.Exceptions.NotFoundException>(() => _service.GetAgentByEmailAsync("notfound@remp.com"));
  }
}
