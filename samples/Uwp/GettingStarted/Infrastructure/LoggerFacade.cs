using System;
using Microsoft.Extensions.Logging;
using Prism.Logging;

namespace GettingStarted.Infrastructure
{
    class LoggerFacade : ILoggerFacade
    {
        readonly ILogger _logger;

        public LoggerFacade(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<App>();
        }

        LogLevel GetLogLevelFrom(Category category)
        {
            switch (category)
            {
                case Category.Debug:
                    return LogLevel.Debug;
                case Category.Info:
                    return LogLevel.Information;
                case Category.Warn:
                    return LogLevel.Warning;
                case Category.Exception:
                    return LogLevel.Error;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Log(string message, Category category, Priority priority)
        {
            _logger.Log(GetLogLevelFrom(category), message);
        }
    }
}
