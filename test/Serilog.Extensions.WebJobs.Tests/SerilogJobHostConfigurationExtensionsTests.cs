// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Serilog.Extensions.WebJobs;
    using Xunit;

    public class SerilogJobHostConfigurationExtensionsShould
    {
        private readonly JobHostConfiguration _configuration;

        public SerilogJobHostConfigurationExtensionsShould()
        {
            _configuration = new JobHostConfiguration();
        }

        [Fact]
        public void RegisterTheLoggerBindingProvider()
        {
            // Act
            _configuration.UseSerilog();

            // Assert
            _configuration
                .GetService<IExtensionRegistry>()
                .GetExtensions<IBindingProvider>()
                .Should()
                .ContainSingle(s => s.GetType() == typeof(LoggerBindingProvider));
        }

        [Fact]
        public void AddedTheSerilogTraceWriter()
        {
            // Act
            _configuration.UseSerilog();

            // Assert
            _configuration.Tracing.Tracers
                .Should()
                .ContainSingle(s => s.GetType() == typeof(SerilogTraceWriter));
        }
    }
}
