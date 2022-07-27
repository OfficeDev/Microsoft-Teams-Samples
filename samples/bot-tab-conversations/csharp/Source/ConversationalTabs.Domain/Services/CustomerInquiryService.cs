// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class CustomerInquiryService : ISubEntityService<CustomerInquiry, CustomerInquiryInput>
{
    private readonly ISubEntityRepository<CustomerInquiry> _customerInquiryRepository;
    private readonly IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> _supportDepartmentsService;
    private readonly IBotService _botService;
    private readonly IAdaptiveCardFactory _adaptiveCardFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomerInquiry> _logger;

    public CustomerInquiryService(
        ISubEntityRepository<CustomerInquiry> customerInquiryRepository,
        IRepositoryObjectService<SupportDepartment, SupportDepartmentInput> customerSupportDepartmentService,
        IBotService botService,
        IAdaptiveCardFactory adaptiveCardFactory,
        IConfiguration configuration,
        ILogger<CustomerInquiry> logger)
    {
        _customerInquiryRepository = customerInquiryRepository;
        _supportDepartmentsService = customerSupportDepartmentService;
        _botService = botService;
        _adaptiveCardFactory = adaptiveCardFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Handles the creation of a SubEntity, in our proof-of-concept a sub-entity is an inquiry.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="subEntityInput"></param>
    /// <returns>The created sub-entity, it will also lead to a </returns>
    public async Task<CustomerInquiry> CreateSubEntity(string entityId, CustomerInquiryInput subEntityInput)
    {
        var subEntityId = Guid.NewGuid().ToString();
        CustomerInquiry customerInquiry = new(subEntityId, DateTime.UtcNow, subEntityInput.CustomerName, subEntityInput.Question, "", true);
        customerInquiry = await _customerInquiryRepository.CreateSubEntity(entityId, customerInquiry).ConfigureAwait(false);

        try
        {
            var supportDepartment = await _supportDepartmentsService.GetSingle(entityId);
            var conversationResponse = await _botService.CreateConversation(
                _adaptiveCardFactory.CreateCustomerInquiryCard(supportDepartment, customerInquiry),
                supportDepartment.ProactiveBotData.ServiceUrl,
                supportDepartment.TeamChannelId,
                supportDepartment.TenantId,
                $"28:{_configuration.GetValue<string>("Bot:MicrosoftAppId")}");

            customerInquiry = customerInquiry with { ConversationId = conversationResponse.ActivityId };
            await _customerInquiryRepository.UpdateSubEntity(entityId, customerInquiry).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Something went wrong while adding '{subEntityInput.CustomerName}''s inquiry to customer support department '{entityId}'");
            await _customerInquiryRepository.DeleteSubEntity(entityId, customerInquiry);

            throw new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown, $"Error while adding '{subEntityInput.CustomerName}''s inquiry to customer support department '{entityId}'");
        }

        return customerInquiry;
    }

    public async Task<CustomerInquiry> GetSubEntity(string entityId, string subEntityId)
    {
        var allItems = await GetSubEntities(entityId);
        var subEntityItem = allItems.FirstOrDefault(i => i.SubEntityId == subEntityId);

        if (subEntityItem == default(CustomerInquiry))
        {
            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.ItemNotFound, $"Requested customer inquiry with id '{subEntityId}' was not found.");
        }

        return subEntityItem;
    }

    public async Task<ICollection<CustomerInquiry>> GetSubEntities(string entityId)
    {
        return await _customerInquiryRepository.GetSubEntities(entityId).ConfigureAwait(false);
    }
}
