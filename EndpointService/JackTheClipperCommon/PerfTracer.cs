using System;
using System.Diagnostics;

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
        /// Initializes a new instance of the <see cref="PerfTracer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public PerfTracer(string name)
        {
            this.name = name;
            this.watch = Stopwatch.StartNew();
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            watch.Stop();

            Console.WriteLine(DateTime.UtcNow.ToLongTimeString() + " " + name + " took " + this.watch.ElapsedMilliseconds);
            GC.SuppressFinalize(this);
        }
    }
}