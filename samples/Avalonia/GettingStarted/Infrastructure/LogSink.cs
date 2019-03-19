using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;

namespace GettingStarted.Infrastructure
{
    class LogSink : ILogSink
    {
        readonly ILoggerFactory _loggerFactory;

        public LogSink(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        LogLevel GetLogLevelFrom(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    return LogLevel.Trace;
                case LogEventLevel.Debug:
                    return LogLevel.Debug;
                case LogEventLevel.Information:
                    return LogLevel.Information;
                case LogEventLevel.Warning:
                    return LogLevel.Warning;
                case LogEventLevel.Error:
                    return LogLevel.Error;
                case LogEventLevel.Fatal:
                    return LogLevel.Critical;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate, params object[] propertyValues)
        {
            // we need no caching because the default logger factory implementation already returns cached logger instances
            _loggerFactory.CreateLogger(area + ":" + source).Log(GetLogLevelFrom(level), messageTemplate, propertyValues);
        }
    }
}
