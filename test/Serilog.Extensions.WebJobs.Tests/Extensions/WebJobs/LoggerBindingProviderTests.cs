// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Xunit;

    public class LoggerBindingProviderShould
    {
        private static readonly ParameterInfo LoggerParameterInfo = GetParameterInfo().First();

        private static readonly ParameterInfo StringParameterInfo = GetParameterInfo().Last();

        private readonly LoggerBindingProvider _sut;

// ReSharper disable UnusedParameter.Local
        private static ParameterInfo[] GetParameterInfo(ILogger unused1 = null, string unused2 = null)
        {
            MethodInfo method =
                typeof(LoggerBindingProviderShould).GetMethod(
                    "GetParameterInfo",
                    BindingFlags.NonPublic | BindingFlags.Static);

            return method.GetParameters();
        }
// ReSharper restore UnusedParameter.Local

        public LoggerBindingProviderShould()
        {
            _sut = new LoggerBindingProvider();
        }

        [Fact]
        public void ImplementTheIBindingProviderInterface()
        {
            _sut.Should().BeAssignableTo<IBindingProvider>();
        }

        [Fact]
        public async Task CreateALoggerBindingForILoggerParameters()
        {
            // Arrange
            var context = new BindingProviderContext(
                LoggerParameterInfo,
                new Dictionary<string, Type>(),
                CancellationToken.None);

            // Act
            IBinding actual = await _sut.TryCreateAsync(context).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull().And.BeAssignableTo<LoggerBinding>();
        }

        [Fact]
        public async Task ReturnNullForNonILoggerParameters()
        {
            // Arrange
            var context = new BindingProviderContext(
                StringParameterInfo,
                new Dictionary<string, Type>(),
                CancellationToken.None);

            // Act
            IBinding actual = await _sut.TryCreateAsync(context).ConfigureAwait(false);

            // Assert
            actual.Should().BeNull();
        }
    }
}
