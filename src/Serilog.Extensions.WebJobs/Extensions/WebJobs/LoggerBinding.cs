// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Serilog.Core;

    /// <summary>
    /// Provides a <see cref="LoggerValueProvider"/> for a parameter binding.
    /// </summary>
    public class LoggerBinding : IBinding
    {
        private readonly ParameterInfo _parameter;

        private readonly Func<ILogger> _currentLoggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerBinding"/> class.
        /// </summary>
        /// <param name="parameter">The parameter to bind a <see cref="LoggerValueProvider"/>.</param>
        public LoggerBinding(ParameterInfo parameter)
            : this(parameter, UseGlobalLogger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerBinding"/> class.
        /// </summary>
        /// <param name="parameter">The parameter to bind a <see cref="LoggerValueProvider"/>.</param>
        /// <param name="currentLoggerFactory">
        /// The factory function get the current logger. This is provided to facilitate testing.
        /// </param>
        public LoggerBinding(ParameterInfo parameter, Func<ILogger> currentLoggerFactory)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            _currentLoggerFactory =
                currentLoggerFactory ?? throw new ArgumentNullException(nameof(currentLoggerFactory));
        }

        /// <summary>
        /// Gets a value indicating whether the binding was sourced from a parameter attribute.
        /// </summary>
        public bool FromAttribute => false;

        /// <summary>
        /// Returns the <see cref="LoggerValueProvider"/> for the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to bind to.</param>
        /// <param name="context">The binding context.</param>
        /// <returns>A task that returns the <see cref="LoggerValueProvider" /> for the binding.</returns>
        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return BindAsync(context.FunctionInstanceId, context.Trace);
        }

        /// <summary>
        /// Returns the <see cref="LoggerValueProvider"/> for the specified <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="BindingContext"/>.</param>
        /// <returns>A task that returns the <see cref="LoggerValueProvider" /> for the binding.</returns>
        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return BindAsync(context.FunctionInstanceId, context.Trace);
        }

        /// <summary>
        /// Get the description of the binding.
        /// </summary>
        /// <returns>The description of the binding.</returns>
        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "Function Logger",
                }
            };
        }

        private static ILogger UseGlobalLogger()
        {
            return Log.Logger;
        }

        private Task<IValueProvider> BindAsync(Guid functionInstanceId, TraceWriter trace)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            LoggerConfiguration configuration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("FunctionInstanceId", functionInstanceId)
                .WriteTo.TraceWriter(trace);

            // Add the original logger as a sink if it has been configured.
            // The default silent logger does not implement ILogEventSink.
            if (_currentLoggerFactory() is ILogEventSink original)
                configuration = configuration.WriteTo.Sink(original);

            ILogger log = configuration.CreateLogger();

            return Task.FromResult<IValueProvider>(new LoggerValueProvider(log));
        }
    }
}
