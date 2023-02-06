// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupportDepartmentsController : ControllerBase
{
    private readonly IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> _supportDepartmentService;
    private readonly ISubEntityService<CustomerInquiry, CustomerInquiryInput> _customerInquiryService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserTeamsService _userChannelService;
    private readonly ILogger<SupportDepartmentsController> _logger;

    public SupportDepartmentsController(
        IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> supportDepartmentService,
        ISubEntityService<CustomerInquiry, CustomerInquiryInput> customerInquiryService,
        IAuthorizationService authorizationService,
        IUserTeamsService userChannelService,
        ILogger<SupportDepartmentsController> logger)
    {
        _supportDepartmentService = supportDepartmentService;
        _customerInquiryService = customerInquiryService;
        _authorizationService = authorizationService;
        _userChannelService = userChannelService;
        _logger = logger;
    }

    /// <summary>
    /// Get all the customer support departments that user has access to.
    /// Access is determined based on the channels, and customer support departments associated with those channel.
    /// </summary>
    /// <returns>
    /// An Ok Result on success along with a json response of customer support departments
    /// An empty list when no customer support department are found
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<SupportDepartment[]>> GetSupportDepartmentsAsync()
    {
        var allSupportDepartments = await _supportDepartmentService.GetAll().ConfigureAwait(false);
        var userTeams = await _userChannelService.GetUsersTeamsAsync().ConfigureAwait(false);

        var userTeamIds = userTeams.Select(c => c.Id).ToHashSet();
        var usersSupportDepartments = allSupportDepartments.Where(sd => userTeamIds.Contains(sd.GroupId)).ToList();

        return new OkObjectResult(usersSupportDepartments);
    }

    /// <summary>
    /// Gets a specific customer support department matching with an entity id.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns>An Ok Result on success, along with a json response of the customer support department</returns>
    /// <exception cref="ApiException">
    /// If no matching customer support department for the specific entity id is found
    /// A Http Not found 404 exception with Document Not found error type is thrown
    /// </exception>
    [HttpGet("{entityId}")]
    public async Task<ActionResult<SupportDepartment>> GetSupportDepartmentAsync([FromRoute] string entityId)
    {
        ApiArgumentException.ThrowIfNullOrEmpty(entityId);

        var supportDepartment = await _supportDepartmentService.GetSingle(entityId).ConfigureAwait(false);
        await _authorizationService.AuthorizeAsync(
            User,
            supportDepartment.GroupId,
            AuthZPolicy.IsMemberOfTeamPolicy);

        return new OkObjectResult(supportDepartment);
    }

    /// <summary>
    /// Create a new Customer Support Department
    /// </summary>
    /// <param name="supportDepartmentInput">Input details required to create a customer support department</param>
    /// <returns>An OK Result on success along with a json response of the customer support department created</returns>
    /// <exception cref="ApiArgumentNullException">If the parameters Title, TeamChannelId or GroupId from the input are null</exception>
    [HttpPost]
    public async Task<ActionResult<SupportDepartment>> CreateSupportDepartmentAsync([FromBody] SupportDepartmentInput supportDepartmentInput)
    {
        ApiArgumentNullException.ThrowIfNull(supportDepartmentInput?.Title);
        ApiArgumentNullException.ThrowIfNull(supportDepartmentInput?.TeamChannelId);
        ApiArgumentNullException.ThrowIfNull(supportDepartmentInput?.GroupId);

        await _authorizationService.AuthorizeAsync(User, supportDepartmentInput.GroupId, AuthZPolicy.IsMemberOfTeamPolicy);

        SupportDepartment supportDepartment = await _supportDepartmentService.Create(supportDepartmentInput).ConfigureAwait(false);

        return new OkObjectResult(supportDepartment);
    }

    /// <summary>
    /// Gets a customer inquiry
    /// </summary>
    /// <param name="entityId">EntityId of the customer support department</param>
    /// <param name="subEntityId">SubEntityId of the singular customer inquiry</param>
    /// <returns>An OK Result on success, with the <see cref="ISubEntity"/></returns>
    /// <exception cref="ApiArgumentException">If the parameters are null, or default</exception>
    [HttpGet("{entityId}/inquiry/{subEntityId}")]
    public async Task<ActionResult<ISubEntity>> GetCustomerInquiryAsync([FromRoute] string entityId, [FromRoute] string subEntityId)
    {
        ApiArgumentException.ThrowIfNullOrEmpty(entityId);
        ApiArgumentException.ThrowIfNullOrEmpty(subEntityId);

        var supportDepartment = await _supportDepartmentService.GetSingle(entityId).ConfigureAwait(false);
        await _authorizationService.AuthorizeAsync(
            User,
            supportDepartment.GroupId,
            AuthZPolicy.IsMemberOfTeamPolicy);

        var customerInquiry = await _customerInquiryService.GetSubEntity(entityId, subEntityId).ConfigureAwait(false);

        return new OkObjectResult(customerInquiry);
    }
}
