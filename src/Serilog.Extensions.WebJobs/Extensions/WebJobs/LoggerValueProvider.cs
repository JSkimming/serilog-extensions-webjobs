// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog.Extensions.WebJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host.Bindings;

    /// <summary>
    /// Provides <see cref="ILogger"/> values.
    /// </summary>
    public class LoggerValueProvider : IValueProvider
    {
        private Task<object> _asyncValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerValueProvider"/> class.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> value.</param>
        public LoggerValueProvider(ILogger log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/> value.
        /// </summary>
        public ILogger Log { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="ILogger"/> value.
        /// </summary>
        public Type Type => typeof(ILogger);

        /// <summary>
        /// Gets the <see cref="ILogger"/> value.
        /// </summary>
        /// <returns>A task that returns the value.</returns>
        public Task<object> GetValueAsync() => _asyncValue ?? (_asyncValue = Task.FromResult<object>(Log));

        /// <summary>
        /// Returns a string representation of the <see cref="ILogger"/> value.
        /// </summary>
        /// <returns>The string representation of the <see cref="ILogger"/> value.</returns>
        public string ToInvokeString() => Log.ToString();
    }
}
