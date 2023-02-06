namespace Microsoft.Teams.Selfhelp.Authentication.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Repositories.LogEventRepository;

    /// <summary>
    /// Controller for log custom event details.
    /// </summary>
    [Route("api/logevent")]
    public class LogCustomEventContoller : BaseController
    {
        /// <summary>
        /// Instance of log event repository details.
        /// </summary>
        private readonly ILogEventRepository logEventRepository;

        /// <summary>
        /// Instance of logger details.
        /// </summary>
        private readonly ILogger<LogCustomEventContoller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogCustomEventContoller"/> class.
        /// </summary>
        /// <param name="logEventRepository">Entity represent log event repository details.</param>
        /// <param name="loggerFactory">Entity represent logger factory details.</param>
        /// <param name="telemetryClient">Entity represent application insights telemetry client.</param>
        public LogCustomEventContoller(
            ILogEventRepository logEventRepository,
            ILoggerFactory loggerFactory,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.logEventRepository = logEventRepository ?? throw new ArgumentNullException(nameof(logEventRepository));
            this.logger = loggerFactory?.CreateLogger<LogCustomEventContoller>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Create or update learning path.
        /// </summary>
        /// <param name="entity">Details of the custom event to be logged.</param>
        /// <returns>Returns true if task is created else return false.</returns>
        [HttpPost]
        public async Task<IActionResult> AddCustomEventLog([FromBody] EventLogEntity entity)
        {
            this.RecordEvent("AddCustomEventLog- The HTTP POST call to create task has been initiated.", RequestType.Initiated);

            if (entity == null)
            {
                this.logger.LogError("AddCustomEventLog Entitydetail is null.");
                this.RecordEvent("AddCustomEventLog - The HTTP POST call to create task has has failed.", RequestType.Failed);
                return this.BadRequest();
            }

            try
            {
                entity.EventId = Guid.NewGuid().ToString();
                entity.CreatedOn = DateTime.UtcNow;
                await this.logEventRepository.AddEventLog(entity);

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.RecordEvent("AddCustomEventLog- The HTTP POST call has failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while AddCustomEventLog.");
                throw;
            }
        }
    }
}