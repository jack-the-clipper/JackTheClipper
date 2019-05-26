using System;
using System.Diagnostics;
using JackTheClipperCommon.Configuration;

namespace JackTheClipperCommon
{
    /// <summary>
    /// Performance Tracer with a name for the Debug Output
    /// </summary>
    public class PerfTracer : IDisposable
    {
        /// <summary>
        /// The name
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The stop watch
        /// </summary>
        private readonly Stopwatch watch;

        /// <summary>
        /// The minimum amount of milliseconds which must be exceeded before logging occurs (default:0)
        /// </summary>
        private readonly int minimumMillis;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfTracer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minimumMillis">The minimum amount of milliseconds which must be exceeded before logging occurs (default:0)</param>
        public PerfTracer(string name, int minimumMillis = 0)
        {
            if (AppConfiguration.PerformanceTraceActive)
            {
                this.name = name;
                this.watch = Stopwatch.StartNew();
                this.minimumMillis = minimumMillis;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (AppConfiguration.PerformanceTraceActive)
            {
                if (watch != null)
                {
                    watch.Stop();

                    if (watch.ElapsedMilliseconds > this.minimumMillis)
                    {
                        Console.WriteLine(DateTime.UtcNow.ToLongTimeString() + " " + name + " took " +
                                          this.watch.ElapsedMilliseconds);
                    }
                }

                GC.SuppressFinalize(this);
            }
        }
    }
}