// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Azure.WebJobs.Host;
    using Serilog.Core;
    using Serilog.Debugging;
    using Serilog.Events;

    /// <summary>
    /// A <c>serilog</c> <see cref="ILogEventSink"/> to write events to an Azure WebJobs <see cref="TraceWriter"/>.
    /// </summary>
    public class TraceWriterSink : ILogEventSink
    {
        /// <summary>
        /// The name of this sink when used as the <see cref="TraceEvent.Source"/> for a <see cref="TraceEvent"/>.
        /// </summary>
        public const string SourceName = nameof(TraceWriterSink);

        private readonly object _lock = new object();

        private readonly TraceWriter _trace;

        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceWriterSink"/> class.
        /// </summary>
        /// <param name="trace">The <see cref="TraceWriter"/> to write <c>serilog</c> events.</param>
        /// <param name="formatProvider">
        /// A <see cref="IFormatProvider"/> use to format a <see cref="LogEvent"/>.
        /// </param>
        public TraceWriterSink(TraceWriter trace, IFormatProvider formatProvider = null)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _formatProvider = formatProvider;
        }

        /// <summary>
        /// Emit the provided log event to an Azure WebJobs <see cref="TraceWriter"/>.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));

            lock (_lock)
            {
                TraceLevel traceLevel = GetTraceLevel(logEvent.Level);
                string message = logEvent.RenderMessage(_formatProvider);
                Exception exception = logEvent.Exception;

                var traceEvent = new TraceEvent(traceLevel, message, SourceName, exception);
                _trace.Trace(traceEvent);
            }
        }

        private static TraceLevel GetTraceLevel(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    return TraceLevel.Verbose;
                case LogEventLevel.Information:
                    return TraceLevel.Info;
                case LogEventLevel.Warning:
                    return TraceLevel.Warning;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    return TraceLevel.Error;
                default:
                    SelfLog.WriteLine("Unexpected logging level, writing to TraceWriter as Info.");
                    return TraceLevel.Info;
            }
        }
    }
}
