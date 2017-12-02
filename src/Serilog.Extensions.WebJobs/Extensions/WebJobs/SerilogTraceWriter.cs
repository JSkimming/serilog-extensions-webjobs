// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Azure.WebJobs.Host;
    using Serilog.Debugging;
    using Serilog.Events;

    /// <summary>
    /// An Azure WebJobs <see cref="TraceWriter"/> to receive trace events and log them to <c>serilog</c>.
    /// </summary>
    public class SerilogTraceWriter : TraceWriter
    {
        private readonly Func<ILogger> _currentLoggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogTraceWriter"/> class.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel" /> used to filter trace events.</param>
        public SerilogTraceWriter(TraceLevel level)
            : this(level, UseGlobalLogger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogTraceWriter"/> class.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel" /> used to filter trace events.</param>
        /// <param name="currentLoggerFactory">
        /// The factory function get the current logger. This is provided to facilitate testing.
        /// </param>
        public SerilogTraceWriter(TraceLevel level, Func<ILogger> currentLoggerFactory)
            : base(level)
        {
            if (currentLoggerFactory == null)
                throw new ArgumentNullException(nameof(currentLoggerFactory));

            _currentLoggerFactory = currentLoggerFactory;
        }

        /// <summary>
        /// Writes a <paramref name="traceEvent"/> to <c>serilog</c> if the <see cref="TraceEvent.Source"/> is not from
        /// <see cref="TraceWriterSink"/>.
        /// </summary>
        /// <param name="traceEvent">The <see cref="TraceEvent" /> to trace.</param>
        public override void Trace(TraceEvent traceEvent)
        {
            if (traceEvent == null)
                throw new ArgumentNullException(nameof(traceEvent));

            // Don't write the log to Serilog if the trace event originated from Serilog.
            if (traceEvent.Source == TraceWriterSink.SourceName || traceEvent.Level == TraceLevel.Off)
                return;

            ILogger logger = _currentLoggerFactory();

            if (!string.IsNullOrWhiteSpace(traceEvent.Source))
                logger = logger.ForContext("WebJobsEventSource", traceEvent.Source);

            LogEventLevel level = GetLogEventLevel(traceEvent.Level);
            logger.Write(level, traceEvent.Exception, traceEvent.Message);
        }

        private static ILogger UseGlobalLogger()
        {
            return Log.Logger;
        }

        private static LogEventLevel GetLogEventLevel(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Verbose:
                    return LogEventLevel.Verbose;
                case TraceLevel.Info:
                    return LogEventLevel.Information;
                case TraceLevel.Warning:
                    return LogEventLevel.Warning;
                case TraceLevel.Error:
                    return LogEventLevel.Error;
                default:
                    SelfLog.WriteLine("Unexpected trace level, using LogEventLevel of Information.");
                    return LogEventLevel.Information;
            }
        }
    }
}
