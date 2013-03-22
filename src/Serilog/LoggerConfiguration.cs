﻿using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.DumpFile;
using Serilog.Sinks.SystemConsole;
using Serilog.Sinks.Trace;

namespace Serilog
{
    public class LoggerConfiguration
    {
        readonly List<ILogEventSink> _logEventSinks = new List<ILogEventSink>();
        readonly List<ILogEventEnricher> _enrichers = new List<ILogEventEnricher>(); 
        readonly IMessageTemplateCache _parsedMessageTemplateCache = new MessageTemplateCache();

        LogEventLevel _minimumLevel = LogEventLevel.Information;

        public IMessageTemplateCache ParsedMessageTemplateCache { get { return _parsedMessageTemplateCache; } }

        public LoggerConfiguration WithConsoleSink(LogEventLevel restrictedToMinimumLevel = LogEventLevel.Minimum)
        {
            const string defaultOutputTemplate = "{TimeStamp} [{Level}] {Message:l}{NewLine:l}{Exception:l}";
            var formatter = new MessageTemplateTextFormatter(defaultOutputTemplate, ParsedMessageTemplateCache);
            return WithSink(new ConsoleSink(formatter), restrictedToMinimumLevel);
        }

        public LoggerConfiguration WithSink(ILogEventSink logEventSink, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Minimum)
        {
            var sink = logEventSink;
            if (restrictedToMinimumLevel > LogEventLevel.Minimum)
                sink = new RestrictedSink(sink, restrictedToMinimumLevel);

            _logEventSinks.Add(sink);
            return this;
        }

        public LoggerConfiguration MinimumLevel(LogEventLevel minimumLevel)
        {
            _minimumLevel = minimumLevel;
            return this;
        }

        public LoggerConfiguration EnrichedBy(params ILogEventEnricher[] enrichers)
        {
            if (enrichers == null) throw new ArgumentNullException("enrichers");
            _enrichers.AddRange(enrichers);
            return this;
        }

        public ILogger CreateLogger()
        {
            return new Logger(_parsedMessageTemplateCache, _minimumLevel, new SafeAggregateSink(_logEventSinks), _enrichers);
        }

        public LoggerConfiguration WithDumpFile(string path, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Minimum)
        {
            return WithSink(new DumpFileSink(path), restrictedToMinimumLevel);
        }

        public LoggerConfiguration WithDiagnosticTraceSink(LogEventLevel restrictedToMinimumLevel = LogEventLevel.Minimum)
        {
            const string defaultOutputTemplate = "[{Level}] {Message:l}{NewLine:l}{Exception:l}";
            var formatter = new MessageTemplateTextFormatter(defaultOutputTemplate, ParsedMessageTemplateCache);
            return WithSink(new DiagnosticTraceSink(formatter), restrictedToMinimumLevel);
        }

        public LoggerConfiguration WithFixedProperty(string propertyName, object value)
        {
            return EnrichedBy(new FixedPropertyEnricher(new[] { LogEventProperty.For(propertyName, value) }));
        }
    }
}
