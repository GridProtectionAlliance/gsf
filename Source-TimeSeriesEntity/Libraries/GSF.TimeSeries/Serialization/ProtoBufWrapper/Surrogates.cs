using System;
using ProtoBuf;

namespace GSF.TimeSeries.Serialization.ProtoBufWrapper
{
    [ProtoContract]
    class MeasurementSurrogate<T>
    {
        /// <summary>
        /// DO NOT USE.  ProtoBuf-net requires a parameterless constructor and 
        /// can access it via reflection.  
        /// </summary>
        private MeasurementSurrogate()
        {

        }

        public static implicit operator MeasurementSurrogate<T>(Measurement<T> measurement)
        {
            return
                measurement != null
                ? new MeasurementSurrogate<T> { Id = measurement.ID, Timestamp = measurement.Timestamp, StateFlags = measurement.StateFlags, Value = measurement.Value }
                : null;
        }

        public static implicit operator Measurement<T>(MeasurementSurrogate<T> measurementSurrogate)
        {
            return new Measurement<T>(measurementSurrogate.Id, 
                                      measurementSurrogate.Timestamp,
                                      measurementSurrogate.StateFlags, 
                                      measurementSurrogate.Value);
        }

        [ProtoMember(1)]
        private Guid Id { get; set; }
    
        [ProtoMember(2)]
        private Ticks Timestamp { get; set; }

        [ProtoMember(3)]
        private MeasurementStateFlags StateFlags { get; set; }

        [ProtoMember(4)]
        private T Value { get; set; }
    }

    [ProtoContract]
    class TicksSurrogate
    {
        [ProtoMember(1)]
        private long Ticks { get; set; }

        public static implicit operator TicksSurrogate(Ticks ticks)
        {
            long toAssign = ticks;
            return
                ticks != null
                ? new TicksSurrogate { Ticks = (Int64) ticks }
                : null;
        }

        public static implicit operator Ticks(TicksSurrogate ticksSurrogate)
        {
            return new Ticks(ticksSurrogate.Ticks);
        }
    }
}
