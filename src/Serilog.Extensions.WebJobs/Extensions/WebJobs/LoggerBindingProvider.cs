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
    /// Creates the <see cref="LoggerBinding"/> for <see cref="ILogger"/> parameters of web job functions.
    /// </summary>
    public class LoggerBindingProvider : IBindingProvider
    {
        private static readonly Task<IBinding> NullResult = Task.FromResult<IBinding>(null);

        /// <summary>
        /// Creates the <see cref="LoggerBinding"/> for <see cref="ILogger"/> parameters of web job functions.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <returns>A task that returns the binding on completion.</returns>
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Parameter.ParameterType == typeof(ILogger)
                ? Task.FromResult<IBinding>(new LoggerBinding(context.Parameter))
                : NullResult;
        }
    }
}
