// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Serilog.Extensions.WebJobs;

    /// <summary>
    /// Extension methods for Serilog extension integration. This registers support for
    /// <see cref="LoggerBindingProvider" /> which allows an <see cref="ILogger"/> parameter binding and
    /// <see cref="SerilogTraceWriter"/>
    /// </summary>
    public static class SerilogJobHostConfigurationExtensions
    {
        /// <summary>
        /// Registers the Serilog extensions.
        /// </summary>
        /// <param name="config">The <see cref="JobHostConfiguration"/></param>
        /// <param name="level">The <see cref="TraceLevel" /> used to filter <see cref="TraceEvent"/> messages.</param>
        /// <returns>The specified <paramref name="config"/> to be used in a fluent configuration.</returns>
        public static JobHostConfiguration UseSerilog(
            this JobHostConfiguration config,
            TraceLevel level = TraceLevel.Info)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.GetService<IExtensionRegistry>().RegisterExtension<IBindingProvider>(new LoggerBindingProvider());
            config.Tracing.Tracers.Add(new SerilogTraceWriter(level));

            return config;
        }
    }
}
