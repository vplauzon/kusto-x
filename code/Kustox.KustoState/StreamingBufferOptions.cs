namespace Kustox.KustoState
{
    public class StreamingBufferOptions
    {
        #region Constructor
        public StreamingBufferOptions()
        {
        }

        public StreamingBufferOptions(StreamingBufferOptions original)
        {
        }
        #endregion

        /// <summary>
        /// Interval to wait for items to queue before pushing them for streaming.
        /// </summary>
        public TimeSpan BatchInterval { get; set; } = TimeSpan.FromSeconds(0.1);

        /// <summary>Interval to wait before retrying a throttled streaming ingestion.</summary>
        public TimeSpan ThrottleRetryInterval { get; set; } = TimeSpan.FromSeconds(0.05);

        /// <summary>Interval to wait before retrying to get elected.</summary>
        public TimeSpan ElectionRetryInterval { get; set; } = TimeSpan.FromSeconds(0.1);
    }
}