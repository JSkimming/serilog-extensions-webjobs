﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using AutoFixture.Xunit2;

    internal class WebJobsCustomization : CompositeCustomization
    {
        public WebJobsCustomization()
            : base(new AutoMoqCustomization(), new FixtureCustomization())
        {
        }
    }

    internal class FixtureCustomization : ICustomization
    {

        void ICustomization.Customize(IFixture fixture)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().Customize(new WebJobsCustomization()))
        {
        }
    }
}
