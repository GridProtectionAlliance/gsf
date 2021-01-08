//******************************************************************************************************
//  ConfigurationFrameBase.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public abstract class ConfigurationFrameBase : ChannelFrameBase<IConfigurationCell>, IConfigurationFrame
    {
        #region [ Members ]

        // Fields
        private ushort m_frameRate;
        private decimal m_ticksPerFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameBase"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrameBase"/>.</param>
        /// <param name="cells">The reference to the collection of cells for this <see cref="ConfigurationFrameBase"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrameBase"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrameBase"/>.</param>
        protected ConfigurationFrameBase(ushort idCode, ConfigurationCellCollection cells, Ticks timestamp, ushort frameRate)
            : base(idCode, cells, timestamp)
        {
            FrameRate = frameRate;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrameBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            FrameRate = info.GetUInt16("frameRate");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="FundamentalFrameType"/> for this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public override FundamentalFrameType FrameType => FundamentalFrameType.ConfigurationFrame;

        /// <summary>
        /// Gets flag that determines if frame image can be queued for publication or should be processed immediately.
        /// </summary>
        /// <remarks>
        /// Configuration frames are not queued for publication by default.
        /// </remarks>
        public override bool AllowQueuedPublication => false;

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public new virtual ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public new virtual IConfigurationFrameParsingState State
        {
            get => base.State as IConfigurationFrameParsingState;
            set => base.State = value;
        }

        /// <summary>
        /// Gets or sets defined frame rate of this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual ushort FrameRate
        {
            get => m_frameRate;
            set
            {
                m_frameRate = value;

                if (m_frameRate != 0)
                    m_ticksPerFrame = Ticks.PerSecond / (decimal)m_frameRate;
                else
                    m_ticksPerFrame = 0;
            }
        }

        /// <summary>
        /// Gets the defined <see cref="Ticks"/> per frame of this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual decimal TicksPerFrame => m_ticksPerFrame;

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrameBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frame Rate", FrameRate.ToString());
                baseAttributes.Add("Ticks Per Frame", TicksPerFrame.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets a new nominal <see cref="LineFrequency"/> for all <see cref="IFrequencyDefinition"/> elements of each <see cref="IConfigurationCell"/> in the <see cref="Cells"/> collection.
        /// </summary>
        /// <param name="value">New nominal <see cref="LineFrequency"/> for <see cref="IFrequencyDefinition"/> elements.</param>
        public virtual void SetNominalFrequency(LineFrequency value)
        {

            foreach (IConfigurationCell cell in Cells)
            {
                cell.NominalFrequency = value;
            }

        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameRate", m_frameRate);
        }

        #endregion
    }
}