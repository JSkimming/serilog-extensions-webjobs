// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog.Extensions.WebJobs;
    using AutoTest.ArgNullEx;
    using AutoTest.ArgNullEx.Xunit;
    using Xunit;

    public class RequiresArgNullEx
    {
        [Theory, RequiresArgNullExAutoMoq(typeof(SerilogJobHostConfigurationExtensions))]
        [Exclude(Type = typeof(LoggerBinding), Method = "BindAsync", Parameter = "value")]
        public Task WebJobs(MethodData method)
        {
            return method.Execute();
        }
    }
}
