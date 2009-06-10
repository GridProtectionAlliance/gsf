//*******************************************************************************************************
//  ConfigurationFrameBase.cs
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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PhasorProtocols
{
    /// <summary>
    /// Represents the protocol independent common implementation of any <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
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
        public override FundamentalFrameType FrameType
        {
            get
            {
                return FundamentalFrameType.ConfigurationFrame;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual new IConfigurationFrameParsingState State
        {
            get
            {
                return base.State as IConfigurationFrameParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets or sets defined frame rate of this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual ushort FrameRate
        {
            get
            {
                return m_frameRate;
            }
            set
            {
                m_frameRate = value;

                if (m_frameRate != 0)
                    m_ticksPerFrame = (decimal)Ticks.PerSecond / (decimal)m_frameRate;
                else
                    m_ticksPerFrame = 0;
            }
        }

        /// <summary>
        /// Gets the defined <see cref="Ticks"/> per frame of this <see cref="ConfigurationFrameBase"/>.
        /// </summary>
        public virtual decimal TicksPerFrame
        {
            get
            {
                return m_ticksPerFrame;
            }
        }

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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameRate", m_frameRate);
        }

        #endregion
    }
}