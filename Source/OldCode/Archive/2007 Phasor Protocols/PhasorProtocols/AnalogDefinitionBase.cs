//*******************************************************************************************************
//  AnalogDefinitionBase.cs
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

namespace PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of an <see cref="IAnalogValue"/>.
    /// </summary>
    [Serializable()]
    public abstract class AnalogDefinitionBase : ChannelDefinitionBase, IAnalogDefinition
    {
        #region [ Members ]

        // Fields
        private AnalogType m_type;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinitionBase"/>.</param>
        protected AnalogDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="label">The label of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="offset">The offset of this <see cref="AnalogDefinitionBase"/>.</param>
        /// <param name="type">The <see cref="AnalogType"/> of this <see cref="AnalogDefinitionBase"/>.</param>
        protected AnalogDefinitionBase(IConfigurationCell parent, string label, uint scale, double offset, AnalogType type)
            : base(parent, label, scale, offset)
        {
            m_type = type;
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize analog definition
            m_type = (AnalogType)info.GetValue("type", typeof(AnalogType));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> for the <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat
        {
            get
            {
                return Parent.AnalogDataFormat;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="AnalogType"/> of this <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        public virtual AnalogType AnalogType
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
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="AnalogDefinitionBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Analog Type", (int)AnalogType + ": " + AnalogType);

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
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize analog definition
            info.AddValue("type", m_type, typeof(AnalogType));
        }

        #endregion
   }
}