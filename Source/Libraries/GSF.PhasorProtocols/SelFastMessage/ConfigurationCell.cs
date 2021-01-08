//******************************************************************************************************
//  ConfigurationCell.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The parent <see cref="ConfigurationFrame"/>.</param>
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
            get => base.Parent as ConfigurationFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new uint IDCode
        {
            // SEL Fast Message only allows one device, so we share ID code with parent frame...
            get => Parent.IDCode;
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
            get => DataFormat.FloatingPoint;
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
            get => CoordinateFormat.Polar;
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
            get => DataFormat.FloatingPoint;
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
            get => DataFormat.FloatingPoint;
            set
            {
                if (value != DataFormat.FloatingPoint)
                    throw new NotSupportedException("SEL Fast Message only supports floating-point data");
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int MaximumStationNameLength =>
            // The station name is defined external to the protocol, so there is no set limit
            int.MaxValue;

    #endregion
    }
}