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
            map.binaryMap.ForEach(m => binaryMap.Add(m.dnpIndex, m.tsfId));
            map.analogMap.ForEach(m => analogMap.Add(m.dnpIndex, m.tsfId));
            map.counterMap.ForEach(m => counterMap.Add(m.dnpIndex, m.tsfId));
            map.controlStatusMap.ForEach(m => controlStatusMap.Add(m.dnpIndex, m.tsfId));
            map.setpointStatusMap.ForEach(m => setpointStatusMap.Add(m.dnpIndex, m.tsfId));
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
            return LookupMaybeNull<Counter>(meas, index, analogMap, ConvertCounter);
        }

        public IMeasurement LookupMaybeNull(ControlStatus meas, UInt32 index)
        {
            return LookupMaybeNull<ControlStatus>(meas, index, analogMap, ConvertControlStatus);
        }

        public IMeasurement LookupMaybeNull(SetpointStatus meas, UInt32 index)
        {
            return LookupMaybeNull<SetpointStatus>(meas, index, analogMap, ConvertSetpointStatus);
        }

        private Measurement ConvertBinary(Binary meas, String id)
        {
            var m = new Measurement();
            m.TagName = id;
            m.Value = meas.value ? 1.0 : 0.0;
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertAnalog(Analog meas, String id)
        {
            var m = new Measurement();
            m.TagName = id;
            m.Value = meas.value;
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertCounter(Counter meas, String id)
        {
            var m = new Measurement();
            m.TagName = id;            
            m.Value = meas.value; //auto cast to double
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertControlStatus(ControlStatus meas, String id)
        {
            var m = new Measurement();
            m.TagName = id;
            m.Value = meas.value ? 1.0 : 0.0;
            m.Timestamp = meas.time;
            return m;
        }

        private Measurement ConvertSetpointStatus(SetpointStatus meas, String id)
        {
            var m = new Measurement();
            m.TagName = id;
            m.Value = meas.value;
            m.Timestamp = meas.time;
            return m;
        }

        private static IMeasurement LookupMaybeNull<T>(T meas, UInt32 index, Dictionary<UInt32, String> map, Func<T, String, Measurement> converter)
        {
            string id;
            if (map.TryGetValue(index, out id)) return converter(meas, id);
            else return null;
        }

        private Dictionary<UInt32, String> binaryMap = new Dictionary<uint, string>();
        private Dictionary<UInt32, String> analogMap = new Dictionary<uint, string>();
        private Dictionary<UInt32, String> counterMap = new Dictionary<uint, string>();
        private Dictionary<UInt32, String> controlStatusMap = new Dictionary<uint, string>();
        private Dictionary<UInt32, String> setpointStatusMap = new Dictionary<uint, string>();
    }
}
