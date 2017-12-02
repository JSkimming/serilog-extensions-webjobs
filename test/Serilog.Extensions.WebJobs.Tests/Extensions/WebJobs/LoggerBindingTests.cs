// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Serilog.Core;
    using Serilog.Events;
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

            return method?.GetParameters().Single();
        }

        public LoggerBindingShould()
        {
            _sut = new LoggerBinding(ParameterInfo, () => _currentLoggerFactory());

            IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

            // A JToken is needed deep into the object graph for a ValueBindingContext
            fixture.Inject<JToken>(new JObject());

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

        [Fact]
        public async Task UseTheGlobalLoggerIfNoneSpecified()
        {
            // Arrange - Use a different constructor to test whether the global logger is used.
            var sut = new LoggerBinding(ParameterInfo);

            // Set-up the global logger to be a ILogEventSink.
            var mockSink = new Mock<ILogEventSink>();
            Log.Logger = mockSink.As<ILogger>().Object;

            // Act
            var provider = (LoggerValueProvider)await sut.BindAsync(_bindingContext).ConfigureAwait(false);

            // Assert
            provider.Log.Error("A message");
            mockSink.Verify(s => s.Emit(It.IsAny<LogEvent>()), Times.Once);
        }

        [Fact]
        public async Task UseTheInjectedLoggerFactoryIfSpecified()
        {
            // Arrange - Set-up the injected logger to be a ILogEventSink.
            var mockSink = new Mock<ILogEventSink>();
            _currentLoggerFactory = () => mockSink.As<ILogger>().Object;

            // Act
            var provider = (LoggerValueProvider) await _sut.BindAsync(_bindingContext).ConfigureAwait(false);

            // Assert
            provider.Log.Error("A message");
            mockSink.Verify(s => s.Emit(It.IsAny<LogEvent>()), Times.Once);
        }
    }
}
