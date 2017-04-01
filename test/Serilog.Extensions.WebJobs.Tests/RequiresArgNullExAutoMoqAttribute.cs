// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoTest.ArgNullEx;
    using AutoTest.ArgNullEx.Xunit;
    using Ploeh.AutoFixture;

    [AttributeUsage(AttributeTargets.Method)]
    internal class RequiresArgNullExAutoMoqAttribute : RequiresArgumentNullExceptionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresArgNullExAutoMoqAttribute"/> class.
        /// </summary>
        /// <param name="assemblyUnderTest">A type in the assembly under test.</param>
        public RequiresArgNullExAutoMoqAttribute(Type assemblyUnderTest)
            : base(CreateFixture(GetAssembly(assemblyUnderTest)))
        {
        }

        private static Assembly GetAssembly(Type assemblyUnderTest)
        {
            if (assemblyUnderTest == null)
                throw new ArgumentNullException(nameof(assemblyUnderTest));

            return assemblyUnderTest.Assembly;
        }

        private static readonly Lazy<ILogger> TestLogger = new Lazy<ILogger>(
            () =>
            {
                ILogger logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Trace()
                    .CreateLogger();

                Log.Logger = logger;

                return logger;
            });

        private static readonly ParameterInfo ParameterInfo = GetParameterInfo();

        // ReSharper disable once UnusedParameter.Local
        private static ParameterInfo GetParameterInfo(object unused = null)
        {
            var method =
                typeof(RequiresArgNullExAutoMoqAttribute).GetMethod(
                    "GetParameterInfo",
                    BindingFlags.NonPublic | BindingFlags.Static);

            return method.GetParameters().Single();
        }

        private static IArgumentNullExceptionFixture CreateFixture(Assembly assemblyUnderTest)
        {
            var fixture = new Fixture().Customize(new WebJobsCustomization());

            fixture.Inject(TestLogger.Value);
            fixture.Inject(ParameterInfo);
            fixture.Inject(new LoggerConfiguration().WriteTo);

            var argNullFixture = new ArgumentNullExceptionFixture(assemblyUnderTest, fixture);

            return argNullFixture;
        }
    }

}
