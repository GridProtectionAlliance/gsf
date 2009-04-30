//*******************************************************************************************************
//  ConfigurationFrame.cs
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
//  04/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.Parsing;

namespace PCS.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase
    {
        #region [ Members ]

        // Fields
        OnlineDataFormatFlags m_onlineDataFormatFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <param name="onlineDataFormatFlags">Online data format flags to use in this Macrodyne <see cref="ConfigurationFrame"/>.</param>
        /// <param name="unitID">8 character unit ID to use in this Macrodyne <see cref="ConfigurationFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate a Macrodyne configuration frame.
        /// </remarks>
        public ConfigurationFrame(OnlineDataFormatFlags onlineDataFormatFlags, string unitID)
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
            m_onlineDataFormatFlags = onlineDataFormatFlags;

            ConfigurationCell configCell = new ConfigurationCell(this);

            // Assign station name
            configCell.StationName = unitID;

            // Add a single frequency definition
            configCell.FrequencyDefinition = new FrequencyDefinition(configCell, "Line frequency");

            // Add phasors based on online format flags
            for (int i = 0; i < DerivedPhasorCount; i++)
            {
                configCell.PhasorDefinitions.Add(new PhasorDefinition(configCell, "Phasor " + (i + 1), 1000, 1.0D, PhasorType.Voltage, null));
            }

            // Macrodyne protocol sends data for one device
            Cells.Add(configCell);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_onlineDataFormatFlags = (OnlineDataFormatFlags)info.GetValue("onlineDataFormatFlags", typeof(OnlineDataFormatFlags));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the Macrodyne <see cref="Macrodyne.OnlineDataFormatFlags"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public OnlineDataFormatFlags OnlineDataFormatFlags
        {
            get
            {
                return m_onlineDataFormatFlags;
            }
            set
            {
                m_onlineDataFormatFlags = value;
            }
        }

        /// <summary>
        /// Gets phasor count derived from <see cref="OnlineDataFormatFlags"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public int DerivedPhasorCount
        {
            get
            {
                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor10Enabled) > 0)
                    return 10;
                
                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor9Enabled) > 0)
                    return 9;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor8Enabled) > 0)
                    return 8;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor7Enabled) > 0)
                    return 7;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor6Enabled) > 0)
                    return 6;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor5Enabled) > 0)
                    return 5;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor4Enabled) > 0)
                    return 4;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor3Enabled) > 0)
                    return 3;

                if ((m_onlineDataFormatFlags & Macrodyne.OnlineDataFormatFlags.Phasor2Enabled) > 0)
                    return 2;

                return 1;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Online Data Format Flags", (byte)OnlineDataFormatFlags + ": " + OnlineDataFormatFlags);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// Macrodyne doesn't define a binary configuration frame - so no checksum is defined - this always returns true.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            return true;
        }

        /// <summary>
        /// Method is not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Macrodyne doesn't define a binary configuration frame - so no checksum is defined.</exception>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            
            // Serialize configuration frame
            info.AddValue("onlineDataFormatFlags", m_onlineDataFormatFlags, typeof(OnlineDataFormatFlags));
        }

        #endregion
    }
}