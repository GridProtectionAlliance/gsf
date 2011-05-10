using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    internal class Measurements : PagedViewModelBase<TimeSeriesFramework.UI.DataModels.Measurement, Guid>
    {

        #region [ Members ]

        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_deviceLookupList;
        private Dictionary<int, string> m_signalTypeLookupList;
        private Dictionary<int, string> m_phasorLookupList;

        #endregion

        #region [ Constructors ]

        public Measurements(int itemsPerPage, bool autosave = true)
            : base(itemsPerPage, autosave)
        {
            m_historianLookupList = Historian.GetLookupList(null);
            m_deviceLookupList = Device.GetLookupList(null);
            m_signalTypeLookupList = SignalType.GetLookupList(null);
            m_phasorLookupList = Phasor.GetLookupList(null);
        }

        #endregion

        #region [ Properties ]

        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.SignalID == null;
            }
        }

        public Dictionary<int, string> HistorianLookupList
        {
            get
            {
                return m_historianLookupList;
            }
        }

        public Dictionary<int, string> DeviceLookupList
        {
            get
            {
                return m_deviceLookupList;
            }
        }

        public Dictionary<int, string> SignalTypeLookupList
        {
            get
            {
                return m_signalTypeLookupList;
            }
        }

        public Dictionary<int, string> PhasorLookupList
        {
            get
            {
                return m_phasorLookupList;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override Guid GetCurrentItemKey()
        {
            return CurrentItem.SignalID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.SignalName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Measurement"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.HistorianID = m_historianLookupList.First().Key;
            CurrentItem.SignalTypeID = m_signalTypeLookupList.First().Key;
            CurrentItem.DeviceID = m_deviceLookupList.First().Key;
            CurrentItem.PhasorSourceIndex = m_phasorLookupList.First().Key;
        }        

        #endregion
        
    }
}
