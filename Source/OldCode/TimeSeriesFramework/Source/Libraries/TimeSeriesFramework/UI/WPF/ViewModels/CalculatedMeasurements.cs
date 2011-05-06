using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    internal class CalculatedMeasurements : PagedViewModelBase<CalculatedMeasurement, int>
    {
        #region [ Members ]

        private Dictionary<Guid, string> m_nodeLookupList;
        private Dictionary<string, string> m_downsamplingMethod;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Node"/> defined in the database.
        /// </summary>
        public Dictionary<Guid, string> NodeLookupList
        {
            get
            {
                return m_nodeLookupList;
            }
        }

        public Dictionary<string, string> DownsamplingMethodLookupList
        {
            get
            {
                return m_downsamplingMethod;
            }
        }
        #endregion

        #region [ Constructor ]

        public CalculatedMeasurements(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_nodeLookupList = Node.GetLookupList(null);
            m_downsamplingMethod = CommonFunctions.GetDownsamplingMethodLookupList();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.Name;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CalculatedMeasurement"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.NodeID = m_nodeLookupList.First().Key;
            CurrentItem.DownsamplingMethod = m_downsamplingMethod.First().Key;
        }

        #endregion
    }
}
