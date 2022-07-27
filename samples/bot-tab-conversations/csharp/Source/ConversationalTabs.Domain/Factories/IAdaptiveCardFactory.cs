// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Factories;

using Microsoft.Bot.Schema;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public interface IAdaptiveCardFactory
{
    /// <summary>
    /// Create a card for a Customer Inquiry.
    /// </summary>
    /// <param name="supportDepartment"></param>
    /// <param name="inquiry"></param>
    /// <returns>A card that contains the customer inquiry details</returns>
    Attachment CreateCustomerInquiryCard(SupportDepartment supportDepartment, CustomerInquiry inquiry);
}
