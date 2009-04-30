//*******************************************************************************************************
//  ConfigurationCell.cs
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

namespace PCS.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        public ConfigurationCell(ConfigurationFrame parent)
            : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame Parent
        {
            get
            {
                return base.Parent as ConfigurationFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override ushort IDCode
        {
            // Macrodyne only allows one device, so we share ID code with parent frame...
            get
            {
                return Parent.IDCode;
            }
            set
            {
                Parent.IDCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; Macrodyne doesn't transport floating-point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        public override DataFormat PhasorDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports rectangular phasor data; Macrodyne doesn't transport polar phasor values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports rectangular phasor data.</exception>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get
            {
                return CoordinateFormat.Rectangular;
            }
            set
            {
                if (value != CoordinateFormat.Rectangular)
                    throw new NotSupportedException("Macrodyne only supports rectangular phasor data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; Macrodyne doesn't transport floating-point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        public override DataFormat FrequencyDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// <para>Macrodyne doesn't define any analog values.</para>
        /// <para>This property only supports scaled data; Macrodyne doesn't transport floating point values.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DataFormat AnalogDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int MaximumStationNameLength
        {
            get
            {
                // The station name is defined as an 8-byte ASCII unit ID
                return 8;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}