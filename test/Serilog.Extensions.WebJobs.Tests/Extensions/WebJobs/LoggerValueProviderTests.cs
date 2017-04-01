// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Xunit;

    public class LoggerValueProviderShould
    {
        private readonly LoggerValueProvider _sut;
        private readonly ILogger _logger;

        public LoggerValueProviderShould()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            _logger = fixture.Freeze<ILogger>();

            _sut = fixture.Create<LoggerValueProvider>();
        }

        [Fact]
        public void ImplementTheIValueProviderInterface()
        {
            _sut.Should().BeAssignableTo<IValueProvider>();
        }

        [Fact]
        public void ReturnILoggerAsTheValueType()
        {
            // Act
            Type actual = _sut.Type;

            // Assert
            actual.Should().BeSameAs(typeof(ILogger));
        }

        [Fact]
        public async Task ReturnTheProvidedLoggerAsTheValue()
        {
            // Act
            object actual = await _sut.GetValueAsync().ConfigureAwait(false);

            // Assert
            actual.Should().BeSameAs(_logger);
        }

        [Fact]
        public async Task ReturnTheSameLoggerWhenCalledMultipleTimes()
        {
            // Arrange
            object expected = await _sut.GetValueAsync().ConfigureAwait(false);

            // Act
            object actual = await _sut.GetValueAsync().ConfigureAwait(false);

            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public void ShouldReturnLoggerToString()
        {
            // Arrange
            string expected = _logger.ToString();

            // Act
            string actual = _sut.ToInvokeString();

            // Assert
            actual.Should().Be(expected);
        }
    }
}
