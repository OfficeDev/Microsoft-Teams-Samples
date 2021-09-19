using MeetingApp.Data.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Data.Repositories.Feedback
{
    public interface IFeedbackRepository
    {
        Task<TableResult> StoreFeedbackAsync(FeedbackEntity entity);
    }
}
