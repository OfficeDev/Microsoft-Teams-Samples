// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Exceptions;

/// <summary>
/// This API is used for the External Service that connects to the Teams app.
/// In this proof-of-concept, this external service is a simple admin portal.
/// In an actual implementation, this integration might be with a job posting board,
/// or omni-channel app to provide information to Teams.
/// </summary>
/// <remarks>
/// This API is NOT Authenticated.
/// Because it is a proof-of-concept we are not going to check AuthN, but you should ensure only verified systems can call your service.
/// </remarks>
[ApiController]
[Route("api/external")]
public class ExternalController : ControllerBase
{
    private readonly IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> _supportDepartmentService;
    private readonly ISubEntityService<CustomerInquiry, CustomerInquiryInput> _customerInquiryService;
    private readonly ILogger<ExternalController> _logger;

    public ExternalController(
        IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> supportDepartmentService,
        ISubEntityService<CustomerInquiry, CustomerInquiryInput> customerInquiryService,
        ILogger<ExternalController> logger)
    {
        _supportDepartmentService = supportDepartmentService;
        _customerInquiryService = customerInquiryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all the customer support departments that user has access too.
    /// Access is determined based on the channels, and customer support departments associated with those channel.
    /// </summary>
    /// <returns>
    /// An Ok Result on success along with a json response of customer support departments
    /// An empty list when no customer support department are found
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<SupportDepartment[]>> GetAllSupportDepartmentsAsync()
    {
        var supportDepartments = await _supportDepartmentService.GetAll().ConfigureAwait(false);
        return new OkObjectResult(supportDepartments);
    }

    /// <summary>
    /// Creates a new customer inquiry
    /// </summary>
    /// <param name="entityId">Entity id of the customer support department the inquiry is for</param>
    /// <param name="customerInquiryInput">The customer's inquiry</param>
    /// <returns>An OK Result on success, with the created customer inquiry object</returns>
    /// <exception cref="ApiArgumentNullException">If the parameters are null, or default</exception>
    [HttpPost("{entityId}/inquiry")]
    public async Task<ActionResult<ISubEntity>> CreateCustomerInquiryAsync([FromRoute] string entityId, [FromBody] CustomerInquiryInput customerInquiryInput)
    {
        ApiArgumentException.ThrowIfNullOrEmpty(entityId);
        ApiArgumentNullException.ThrowIfNull(customerInquiryInput);
        ApiArgumentException.ThrowIfNullOrEmpty(customerInquiryInput?.Question);

        var createdCustomerInquiry = await _customerInquiryService.CreateSubEntity(entityId, customerInquiryInput).ConfigureAwait(false);

        return new OkObjectResult(createdCustomerInquiry);
    }
}
