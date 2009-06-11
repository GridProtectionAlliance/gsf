//*******************************************************************************************************
//  MetadataRecordGeneralFlags.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/20/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************


namespace TVA.Historian.Files
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the type of data being archived for a <see cref="MetadataRecord"/>.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Data value is analog in nature.
        /// </summary>
        Analog,
        /// <summary>
        /// Data value is digital in nature.
        /// </summary>
        Digital,
        /// <summary>
        /// Data value is calculated based on an equation upon retrieval.
        /// </summary>
        Composed,
        /// <summary>
        /// Data value is a constant and no real-time time series data is received.
        /// </summary>
        Constant
    }

    #endregion

    /// <summary>
    /// Defines general boolean settings for a <see cref="MetadataRecord"/>.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    public class MetadataRecordGeneralFlags
    {
        #region [ Members ]

        // Constants
        private const Bits DataTypeMask = Bits.Bit00 | Bits.Bit01 | Bits.Bit02;
        private const Bits EnabledMask = Bits.Bit03;
        private const Bits UsedMask = Bits.Bit04;
        private const Bits AlarmEnabledMask = Bits.Bit05;
        private const Bits NotifyByEmailMask = Bits.Bit06;
        private const Bits NotifyByPagerMask = Bits.Bit07;
        private const Bits NotifyByPhoneMask = Bits.Bit08;
        private const Bits LogToFileMask = Bits.Bit09;
        private const Bits SpareMask = Bits.Bit10;
        private const Bits ChangedMask = Bits.Bit11;
        private const Bits StepCheckMask = Bits.Bit12;

        // Fields
        private int m_value;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataType"/> of archived data for the <see cref="MetadataRecord"/>.
        /// </summary>
        public DataType DataType
        {
            get
            {
                return (DataType)m_value.GetMaskedValue(DataTypeMask);
            }
            set
            {
                m_value = m_value.SetMaskedValue(DataTypeMask, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MetadataRecord"/> is currently enabled for archival of new data.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_value.CheckBits(EnabledMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(EnabledMask) : m_value.ClearBits(EnabledMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MetadataRecord"/> is being used (need to be verify).
        /// </summary>
        public bool Used
        {
            get
            {
                return m_value.CheckBits(UsedMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(UsedMask) : m_value.ClearBits(UsedMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether alarm notifications are enabled for the <see cref="MetadataRecord"/>.
        /// </summary>
        public bool AlarmEnabled
        {
            get
            {
                return m_value.CheckBits(AlarmEnabledMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(AlarmEnabledMask) : m_value.ClearBits(AlarmEnabledMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether email alarm notifications are to be sent for the <see cref="MetadataRecord"/>.
        /// </summary>
        public bool NotifyByEmail
        {
            get
            {
                return m_value.CheckBits(NotifyByEmailMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(NotifyByEmailMask) : m_value.ClearBits(NotifyByEmailMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether text alarm notifications are to be sent for the <see cref="MetadataRecord"/>.
        /// </summary>
        public bool NotifyByPager
        {
            get
            {
                return m_value.CheckBits(NotifyByPagerMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(NotifyByPagerMask) : m_value.ClearBits(NotifyByPagerMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether voice alarm notifications are to be sent for the <see cref="MetadataRecord"/>.
        /// </summary>
        public bool NotifyByPhone
        {
            get
            {
                return m_value.CheckBits(NotifyByPhoneMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(NotifyByPhoneMask) : m_value.ClearBits(NotifyByPhoneMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether alarm notifications for the <see cref="MetadataRecord"/> are to be logged to a text file.
        /// </summary>
        public bool LogToFile
        {
            get
            {
                return m_value.CheckBits(LogToFileMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(LogToFileMask) : m_value.ClearBits(LogToFileMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MetadataRecord"/> has been modified.
        /// </summary>
        public bool Changed
        {
            get
            {
                return m_value.CheckBits(ChangedMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(ChangedMask) : m_value.ClearBits(ChangedMask);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether a "step change" operation is to be performed on incoming time series data for the <see cref="MetadataRecord"/>.
        /// </summary>
        public bool StepCheck
        {
            get
            {
                return m_value.CheckBits(StepCheckMask);
            }
            set
            {
                m_value = value ? m_value.SetBits(StepCheckMask) : m_value.ClearBits(StepCheckMask);
            }
        }

        /// <summary>
        /// Gets or sets the 32-bit integer value used for defining general boolean settings for a <see cref="MetadataRecord"/>.
        /// </summary>
        public int Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        #endregion
    }
}
