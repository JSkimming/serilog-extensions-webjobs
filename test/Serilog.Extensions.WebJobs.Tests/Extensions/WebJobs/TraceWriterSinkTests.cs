// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host;
    using Moq;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Parsing;
    using Xunit;

    public class TraceWriterSinkShould
    {
        private readonly TraceWriterSink _sut;
        private readonly Mock<TraceWriter> _traceWriterMock;

        public TraceWriterSinkShould()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            // Freeze the injected mocks.
            _traceWriterMock = fixture.Freeze<Mock<TraceWriter>>();

            // Create the system under test.
            _sut = fixture.Freeze<TraceWriterSink>();
        }

        private static LogEvent CreateLogEvent(LogEventLevel level, string message)
        {
            var logEvent =
                new LogEvent(
                    timestamp: DateTimeOffset.UtcNow,
                    level: level,
                    exception: null,
                    messageTemplate: new MessageTemplate(new[] { new TextToken(message) }),
                    properties: Enumerable.Empty<LogEventProperty>());

            return logEvent;
        }

        [Fact]
        public void ImplementTheILogEventSinkInterface()
        {
            _sut.Should().BeAssignableTo<ILogEventSink>();
        }

        [Theory]
        [InlineData("A test Verbose message.", LogEventLevel.Verbose, TraceLevel.Verbose)]
        [InlineData("A test Debug message.", LogEventLevel.Debug, TraceLevel.Verbose)]
        [InlineData("A test Information message.", LogEventLevel.Information, TraceLevel.Info)]
        [InlineData("A test Warning message.", LogEventLevel.Warning, TraceLevel.Warning)]
        [InlineData("A test Error message.", LogEventLevel.Error, TraceLevel.Error)]
        [InlineData("A test Fatal message.", LogEventLevel.Fatal, TraceLevel.Error)]
        [InlineData("A test invalid log level message.", (LogEventLevel)(-1), TraceLevel.Info)]
        public void EmitValidTraceEvents(string message, LogEventLevel logLevel, TraceLevel expected)
        {
            // Arrange
            var logEvent = CreateLogEvent(logLevel, message);
            Expression<Func<TraceEvent, bool>> expr = te => te.Message == message && te.Level == expected;

            // Act
            _sut.Emit(logEvent);

            // Assert
            _traceWriterMock.Verify(t => t.Trace(It.IsAny<TraceEvent>()), Times.Once);
            _traceWriterMock.Verify(t => t.Trace(It.Is(expr)));
        }
    }
}
