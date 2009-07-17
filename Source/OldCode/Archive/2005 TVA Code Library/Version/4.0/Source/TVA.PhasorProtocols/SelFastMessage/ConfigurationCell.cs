//*******************************************************************************************************
//  ConfigurationCell.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/08/2007 - James R. Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
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
        public new uint IDCode
        {
            // SEL Fast Message only allows one device, so we share ID code with parent frame...
            get
            {
                return Parent.IDCode;
            }
            set
            {
                Parent.IDCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; SEL Fast Message doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">SEL Fast Message only supports floating-point data.</exception>
        public override DataFormat PhasorDataFormat
        {
            get
            {
                return DataFormat.FloatingPoint;
            }
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("SEL Fast Message only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports polar phasor data; SEL Fast Message doesn't transport rectangular phasor values.
        /// </remarks>
        /// <exception cref="NotSupportedException">SEL Fast Message only supports polar phasor data.</exception>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get
            {
                return CoordinateFormat.Polar;
            }
            set
            {
                if (value != CoordinateFormat.Polar)
                    throw new NotSupportedException("SEL Fast Message only supports polar phasor data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; SEL Fast Message doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">SEL Fast Message only supports floating-point data.</exception>
        public override DataFormat FrequencyDataFormat
        {
            get
            {
                return DataFormat.FloatingPoint;
            }
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("SEL Fast Message only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports floating-point data; SEL Fast Message doesn't transport scaled values.
        /// </remarks>
        /// <exception cref="NotSupportedException">SEL Fast Message only supports floating-point data.</exception>
        public override DataFormat AnalogDataFormat
        {
            get
            {
                return DataFormat.FloatingPoint;
            }
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("SEL Fast Message only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int MaximumStationNameLength
        {
            get
            {
                // The station name is defined external to the protocol, so there is no set limit
                return int.MaxValue;
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