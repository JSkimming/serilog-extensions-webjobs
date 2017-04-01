// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Azure.WebJobs.Host;
    using Moq;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoMoq;
    using Serilog.Events;
    using Xunit;

    public class SerilogTraceWriterShould
    {
        private readonly SerilogTraceWriter _sut;
        private readonly Mock<ILogger> _loggerMock;

        public SerilogTraceWriterShould()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            _loggerMock = fixture.Create<Mock<ILogger>>();
            _loggerMock.Setup(l => l.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .Returns(_loggerMock.Object);

            // Create the system under test.
            _sut = new SerilogTraceWriter(TraceLevel.Info, () => _loggerMock.Object);
        }

        private static TraceEvent CreateTraceEvent(TraceLevel level, string message, string source = null)
        {
            var traceEvent = new TraceEvent(level, message, source);
            return traceEvent;
        }

        [Fact]
        public void ExtendTraceWriter()
        {
            _sut.Should().BeAssignableTo<TraceWriter>();
        }

        [Theory]
        [InlineData("A test Verbose message.", TraceLevel.Verbose, LogEventLevel.Verbose)]
        [InlineData("A test Info message.", TraceLevel.Info, LogEventLevel.Information)]
        [InlineData("A test Warning message.", TraceLevel.Warning, LogEventLevel.Warning)]
        [InlineData("A test Error message.", TraceLevel.Error, LogEventLevel.Error)]
        [InlineData("A test invalid log level message.", (TraceLevel)(-1), LogEventLevel.Information)]
        public void WriteValidMessages(string message, TraceLevel logLevel, LogEventLevel expected)
        {
            // Arrange
            TraceEvent traceEvent = CreateTraceEvent(logLevel, message);

            // Act
            _sut.Trace(traceEvent);

            // Assert
            _loggerMock.Verify(l => l.Write(expected, default(Exception), message), Times.Once);
        }

        [Fact]
        public void ShouldNotWriteMessageIfTheSourceIsTraceWriterSink()
        {
            // Arrange
            TraceEvent traceEvent = CreateTraceEvent(TraceLevel.Error, string.Empty, TraceWriterSink.SourceName);

            // Act
            _sut.Trace(traceEvent);

            // Assert
            _loggerMock.Verify(
                l => l.Write(It.IsAny<LogEventLevel>(), It.IsAny<Exception>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ShouldPropagateTheWebJobsEventSource()
        {
            // Arrange
            string source = "EventSource_" + Guid.NewGuid().ToString("N").ToUpperInvariant();
            TraceEvent traceEvent = CreateTraceEvent(TraceLevel.Error, string.Empty, source);

            // Act
            _sut.Trace(traceEvent);

            // Assert
            _loggerMock.Verify(l => l.ForContext("WebJobsEventSource", source, false), Times.Once());
        }
    }
}
