//*******************************************************************************************************
//  MetadataRecordSecurityFlags.cs
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
//  03/06/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************


namespace TVA.Historian.Files
{
    /// <summary>
    /// Defines the security level for a <see cref="MetadataRecord"/>.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    public class MetadataRecordSecurityFlags
    {
        #region [ Members ]

        // Constants
        private const Bits RecordSecurityMask = Bits.Bit00 | Bits.Bit01 | Bits.Bit02;
        private const Bits AccessSecurityMask = Bits.Bit03 | Bits.Bit04 | Bits.Bit05;

        // Fields
        private int m_value;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the access level required for a user to edit the <see cref="MetadataRecord"/>.
        /// </summary>
        public int RecordSecurity
        {
            get
            {
                return m_value.GetMaskedValue(RecordSecurityMask);
            }
            set
            {
                m_value = m_value.SetMaskedValue(RecordSecurityMask, value);
            }
        }

        /// <summary>
        /// Gets or sets the access level required for a user to retrieve archived data associated to a <see cref="MetadataRecord"/>.
        /// </summary>
        public int AccessSecurity
        {
            get
            {
                return m_value.GetMaskedValue(AccessSecurityMask) >> 3; // <- 1st 3 bits are record security.
            }
            set
            {
                m_value = m_value.SetMaskedValue(AccessSecurityMask, value << 3);
            }
        }

        /// <summary>
        /// Gets or sets the 32-bit integer value used for defining the security level for a <see cref="MetadataRecord"/>.
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
