//*******************************************************************************************************
//  ConfigurationCellCollection.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents a BPA PDCstream implementation of a collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    [Serializable()]
    public class ConfigurationCellCollection : PhasorProtocols.ConfigurationCellCollection
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/>.
        /// </summary>
        public ConfigurationCellCollection()
            : base(ushort.MaxValue, true)
        {
            // Although the number of configuration cells are not restricted in the
            // INI file, the data stream limits the maximum number of associated
            // data cells to 16-bits, so we limit the configurations cells to the same.
            // Also, PDCstream configuration cells are constant length.
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ConfigurationCell"/> at specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of value to get or set.</param>
        public new ConfigurationCell this[int index]
        {
            get
            {
                return base[index] as ConfigurationCell;
            }
            set
            {
                base[index] = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to retrieve a <see cref="ConfigurationCell"/> from this <see cref="ConmfigurationCellCollection"/> with the specified <paramref name="sectionEntry"/>.
        /// </summary>
        /// <param name="sectionEntry"><see cref="ConfigurationCell.SectionEntry"/> value to try to find.</param>
        /// <param name="configurationCell"><see cref="ConfigurationCell"/> with the specified <paramref name="sectionEntry"/> if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if <see cref="ConfigurationCell"/> with the specified <paramref name="sectionEntry"/> is found; otherwise <c>false</c>.</returns>
        public bool TryGetBySectionEntry(string sectionEntry, ref ConfigurationCell configurationCell)
        {
            for (int i = 0; i < Count; i++)
            {
                configurationCell = this[i];
                if (string.Compare(configurationCell.SectionEntry, sectionEntry, true) == 0)
                    return true;
            }

            configurationCell = null;
            return false;
        }

        #endregion
    }
}