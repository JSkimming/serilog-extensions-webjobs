// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class LoggerBindingShould
    {
        private static readonly ParameterInfo ParameterInfo = GetParameterInfo();

        private readonly LoggerBinding _sut;
        private readonly ValueBindingContext _valueBindingContext;
        private readonly BindingContext _bindingContext;
        private Func<ILogger> _currentLoggerFactory = () => null;

        // ReSharper disable once UnusedParameter.Local
        private static ParameterInfo GetParameterInfo(ILogger unused = null)
        {
            MethodInfo method =
                typeof(LoggerBindingShould).GetMethod(
                    "GetParameterInfo",
                    BindingFlags.NonPublic | BindingFlags.Static);

            return method.GetParameters().Single();
        }

        public LoggerBindingShould()
        {
            _sut = new LoggerBinding(ParameterInfo, () => _currentLoggerFactory());

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _valueBindingContext = fixture.Create<ValueBindingContext>();
            _bindingContext = fixture.Create<BindingContext>();
        }

        [Fact]
        public void ImplementTheIBindingInterface()
        {
            _sut.Should().BeAssignableTo<IBinding>();
        }

        [Fact]
        public void NotIndicateWhetherTheBindingWasSourcedFromAParameterAttribute()
        {
            _sut.FromAttribute.Should().BeFalse();
        }

        [Fact]
        public void ProvideAParameterDescriptor()
        {
            // Act
            ParameterDescriptor descriptor = _sut.ToParameterDescriptor();

            // Assert
            descriptor.Should().NotBeNull();
            descriptor.Name.Should().Be(ParameterInfo.Name);
            descriptor.DisplayHints.Should().NotBeNull();
            descriptor.DisplayHints.Description.Should().Be("Function Logger");
        }

        [Fact]
        public async Task BindToAValueBindingContextWhenThereIsNoCurrentLogger()
        {
            // Act
            IValueProvider actual = await _sut.BindAsync(null, _valueBindingContext).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull().And.BeAssignableTo<LoggerValueProvider>();
        }

        [Fact]
        public async Task BindToABindingContextWhenThereIsNoCurrentLogger()
        {
            // Act
            IValueProvider actual = await _sut.BindAsync(_bindingContext).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull().And.BeAssignableTo<LoggerValueProvider>();
        }

        [Fact]
        public async Task BindToAValueBindingContextWhenThereIsACurrentLogger()
        {
            // Arrange - Inject an existing logger.
            _currentLoggerFactory = new LoggerConfiguration().CreateLogger;

            // Act
            IValueProvider actual = await _sut.BindAsync(null, _valueBindingContext).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull().And.BeAssignableTo<LoggerValueProvider>();
        }

        [Fact]
        public async Task BindToABindingContextWhenThereIsACurrentLogger()
        {
            // Arrange - Inject an existing logger.
            _currentLoggerFactory = new LoggerConfiguration().CreateLogger;

            // Act
            IValueProvider actual = await _sut.BindAsync(_bindingContext).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull().And.BeAssignableTo<LoggerValueProvider>();
        }
    }
}
