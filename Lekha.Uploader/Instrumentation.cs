﻿using System.Diagnostics.Metrics;
using System.Diagnostics;
using System;

namespace Lekha.Uploader
{
    /// <summary>
    /// It is recommended to use a custom type to hold references for
    /// ActivitySource and Instruments. This avoids possible type collisions
    /// with other components in the DI container.
    /// </summary>
    public class Instrumentation : IDisposable
    {
        internal const string ActivitySourceName = "Uploader";
        internal const string MeterName = "Uploader";
        private readonly Meter meter;

        public Instrumentation()
        {
            string version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
            this.ActivitySource = new ActivitySource(ActivitySourceName, version);
            this.meter = new Meter(MeterName, version);
            this.FreezingDaysCounter = this.meter.CreateCounter<long>("weather.days.freezing", "The number of days where the temperature is below freezing");
        }

        public ActivitySource ActivitySource { get; }

        public Counter<long> FreezingDaysCounter { get; }

        public void Dispose()
        {
            this.ActivitySource.Dispose();
            this.meter.Dispose();
        }
    }

}
