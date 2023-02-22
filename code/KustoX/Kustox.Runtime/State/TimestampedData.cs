using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class TimestampedData<T> : TimestampedData
    {
        public TimestampedData(T data, DateTime timestamp)
        {
            Data = data;
            Timestamp = timestamp;
        }

        public T Data { get; }

        public DateTime Timestamp { get; }
    }

    public class TimestampedData
    {
        public static TimestampedData<T> Create<T>(T data, DateTime timestamp)
        {
            return new TimestampedData<T>(data, timestamp);
        }

        public static TimestampedData<T> Create<T>(T data, long timestamp)
        {
            return new TimestampedData<T>(
                data,
                DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime);
        }
    }
}