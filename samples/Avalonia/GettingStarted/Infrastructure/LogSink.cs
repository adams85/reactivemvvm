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
        private readonly LogEventLevel _minLevel;

        public LogSink(ILoggerFactory loggerFactory, LogEventLevel minLevel = LogEventLevel.Information)
        {
            _loggerFactory = loggerFactory;
            _minLevel = minLevel;
        }

        public bool IsEnabled(LogEventLevel level, string area) => level >= _minLevel;

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

        public void Log(LogEventLevel level, string area, object source, string messageTemplate)
        {
            Log(level, area, source, messageTemplate, Array.Empty<object>());
        }

        public void Log<T0>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0)
        {
            Log(level, area, source, messageTemplate, new object[] { propertyValue0 });
        }

        public void Log<T0, T1>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            Log(level, area, source, messageTemplate, new object[] { propertyValue0, propertyValue1 });
        }

        public void Log<T0, T1, T2>(LogEventLevel level, string area, object source, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            Log(level, area, source, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });
        }
    }
}
