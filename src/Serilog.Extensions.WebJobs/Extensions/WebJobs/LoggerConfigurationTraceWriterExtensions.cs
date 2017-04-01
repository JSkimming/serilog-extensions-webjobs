// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Serilog.Configuration;
    using Serilog.Events;

    /// <summary>
    /// Extension methods on <see cref="LoggerSinkConfiguration"/> to set-up <c>serilog</c> logging.
    /// </summary>
    public static class LoggerConfigurationTraceWriterExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to a <see cref="Microsoft.Azure.WebJobs.Host.TraceWriter"/>.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="trace">The trace writer instance to write to <see cref="LogEvent"/>s.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to
        /// the sink.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration TraceWriter(
            this LoggerSinkConfiguration loggerConfiguration,
            Microsoft.Azure.WebJobs.Host.TraceWriter trace,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null)
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            return loggerConfiguration.Sink(new TraceWriterSink(trace, formatProvider), restrictedToMinimumLevel);
        }
    }
}
