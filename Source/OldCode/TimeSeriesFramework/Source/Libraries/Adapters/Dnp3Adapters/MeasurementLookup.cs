using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DNP3.Interface;
using TimeSeriesFramework;

namespace Dnp3Adapters
{
    /// <summary>
    /// Helper class that converts measurements and provides a lookup capbility
    /// </summary>
    class MeasurementLookup
    {
        public MeasurementLookup(MeasurementMap map)
        {
            map.binaryMap.ForEach(m => binaryMap.Add(m.dnpIndex, m));
            map.analogMap.ForEach(m => analogMap.Add(m.dnpIndex, m));
            map.counterMap.ForEach(m => counterMap.Add(m.dnpIndex, m));
            map.controlStatusMap.ForEach(m => controlStatusMap.Add(m.dnpIndex, m));
            map.setpointStatusMap.ForEach(m => setpointStatusMap.Add(m.dnpIndex, m));
        }

        public IMeasurement LookupMaybeNull(Binary meas, UInt32 index)
        {
            return LookupMaybeNull<Binary>(meas, index, binaryMap, ConvertBinary);
        }

        public IMeasurement LookupMaybeNull(Analog meas, UInt32 index)
        {
            return LookupMaybeNull<Analog>(meas, index, analogMap, ConvertAnalog);
        }

        public IMeasurement LookupMaybeNull(Counter meas, UInt32 index)
        {
            return LookupMaybeNull<Counter>(meas, index, counterMap, ConvertCounter);
        }

        public IMeasurement LookupMaybeNull(ControlStatus meas, UInt32 index)
        {
            return LookupMaybeNull<ControlStatus>(meas, index, controlStatusMap, ConvertControlStatus);
        }

        public IMeasurement LookupMaybeNull(SetpointStatus meas, UInt32 index)
        {
            return LookupMaybeNull<SetpointStatus>(meas, index, setpointStatusMap, ConvertSetpointStatus);
        }

        private Measurement ConvertBinary(Binary meas, uint id, String source)
        {
            var m = new Measurement();
            m.Key = new MeasurementKey(Guid.Empty, id, source);
            m.Value = 42.8;// meas.value ? 1.0 : 0.0;
            m.Timestamp = DateTime.UtcNow;
            return m;
        }

        private Measurement ConvertAnalog(Analog meas, uint id, String source)
        {
            var m = new Measurement();
            m.Key = new MeasurementKey(Guid.Empty, id, source);
            m.Value = 23.7;// meas.value;
            m.Timestamp = DateTime.UtcNow;
            return m;
        }

        private Measurement ConvertCounter(Counter meas, uint id, String source)
        {
            var m = new Measurement();
            m.Key = new MeasurementKey(Guid.Empty, id, source);
            m.Value = meas.value; //auto cast to double
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertControlStatus(ControlStatus meas, uint id, String source)
        {
            var m = new Measurement();
            m.Key = new MeasurementKey(Guid.Empty, id, source);
            m.Value = meas.value ? 1.0 : 0.0;
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertSetpointStatus(SetpointStatus meas, uint id, String source)
        {
            var m = new Measurement();
            m.Key = new MeasurementKey(Guid.Empty, id, source);
            m.Value = meas.value;
            m.Timestamp = meas.time;
            return m;
        }

        private static IMeasurement LookupMaybeNull<T>(T meas, UInt32 index, Dictionary<UInt32, Mapping> map, Func<T, uint, String, Measurement> converter)
        {
            Mapping id;
            if (map.TryGetValue(index, out id)) return converter(meas, id.tsfId, id.tsfSource);
            else return null;
        }

        private Dictionary<UInt32, Mapping> binaryMap = new Dictionary<uint, Mapping>();
        private Dictionary<UInt32, Mapping> analogMap = new Dictionary<uint, Mapping>();
        private Dictionary<UInt32, Mapping> counterMap = new Dictionary<uint, Mapping>();
        private Dictionary<UInt32, Mapping> controlStatusMap = new Dictionary<uint, Mapping>();
        private Dictionary<UInt32, Mapping> setpointStatusMap = new Dictionary<uint, Mapping>();
    }
}
