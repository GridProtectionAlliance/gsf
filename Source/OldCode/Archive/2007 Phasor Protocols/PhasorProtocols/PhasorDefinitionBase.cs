//*******************************************************************************************************
//  PhasorDefinitionBase.cs
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
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of a <see cref="IPhasorValue"/>.
    /// </summary>
    [Serializable()]
    public abstract class PhasorDefinitionBase : ChannelDefinitionBase, IPhasorDefinition
    {
        #region [ Members ]

        // Fields
        private PhasorType m_type;
        private IPhasorDefinition m_voltageReference;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinitionBase"/>.</param>
        protected PhasorDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinitionBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinitionBase"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinitionBase"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="PhasorDefinitionBase"/>.</param>
        /// <param name="offset">The offset of this <see cref="PhasorDefinitionBase"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinitionBase"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        protected PhasorDefinitionBase(IConfigurationCell parent, string label, uint scale, double offset, PhasorType type, IPhasorDefinition voltageReference)
            : base(parent, label, scale, offset)
        {
            m_type = type;

            if (type == PhasorType.Voltage)
                m_voltageReference = this;
            else
                m_voltageReference = voltageReference;
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize phasor definition
            m_type = (PhasorType)info.GetValue("type", typeof(PhasorType));
            m_voltageReference = (IPhasorDefinition)info.GetValue("voltageReference", typeof(IPhasorDefinition));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat
        {
            get
            {
                return Parent.PhasorDataFormat;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PhasorProtocols.CoordinateFormat"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public virtual CoordinateFormat CoordinateFormat
        {
            get
            {
                return Parent.PhasorCoordinateFormat;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PhasorType"/> of this <see cref="PhasorDefinitionBase"/>.
        /// </summary>
        public virtual PhasorType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }

        /// <summary>
        /// Gets or sets the associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).
        /// </summary>
        /// <remarks>
        /// This only applies to current phasors.
        /// </remarks>
        public virtual IPhasorDefinition VoltageReference
        {
            get
            {
                return m_voltageReference;
            }
            set
            {
                if (m_type == PhasorType.Voltage)
                    m_voltageReference = this;
                else
                    m_voltageReference = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="PhasorDefinitionBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", (int)Type + ": " + Type);

                return baseAttributes;
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

            // Serialize phasor definition
            info.AddValue("type", m_type, typeof(PhasorType));
            info.AddValue("voltageReference", m_voltageReference, typeof(IPhasorDefinition));
        }

        #endregion
    }
}