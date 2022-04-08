// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Extensions.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AccessibilityInsights.Extensions.TelemetryTests
{
    [TestClass]
    public class TelemetryClientWrapperUnitTests
    {
        [TestMethod]
        [Timeout(1000)]
        public void Ctor_ClientIsNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TelemetryClientWrapper(null));
        }

        [TestMethod]
        [Timeout(1000)]
        public void Ctor_ClientIsNotNull_DoesNotThrow()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            new TelemetryClientWrapper(new TelemetryClient());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        [Timeout(1000)]
        public void TrackEvent_DoesNotThrow()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ITelemetryClientWrapper wrapper = new TelemetryClientWrapper(new TelemetryClient());
#pragma warning restore CS0618 // Type or member is obsolete

            wrapper.TrackEvent(new EventTelemetry());
        }

        [TestMethod]
        [Timeout(1000)]
        public void TrackException_DoesNotThrow()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ITelemetryClientWrapper wrapper = new TelemetryClientWrapper(new TelemetryClient());
#pragma warning restore CS0618 // Type or member is obsolete

            wrapper.TrackException(new InvalidOperationException(), new Dictionary<string, string>());
        }
    }
}
