using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DNP3.Interface;
using TimeSeriesFramework;

namespace Dnp3Adapters
{
    /// <summary>
    /// This is the data adapter that marshalls data from the dnp3 world to the TSF
    /// </summary>
    class TsfDataObserver : IDataObserver
    {
        public delegate void OnNewMeasurements(ICollection<IMeasurement> measurements);
        public event OnNewMeasurements NewMeasurements;

        private MeasurementLookup m_lookup;
        private List<IMeasurement> m_Measurements = new List<IMeasurement>();

        public TsfDataObserver(MeasurementLookup lookup)
        {
            this.m_lookup = lookup;
        }

        public void End()
        {
            if (m_Measurements.Count > 0 && this.NewMeasurements != null)
            {
                this.NewMeasurements(m_Measurements);
            }
        }

        public void Start()
        {
            m_Measurements.Clear();
        }

        public void Update(SetpointStatus update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);

        }

        public void Update(ControlStatus update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Counter update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Analog update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Binary update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }


    }
}
