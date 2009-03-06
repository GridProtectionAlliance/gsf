//*******************************************************************************************************
//  FrequencyDefinitionBase.cs
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of a <see cref="IFrequencyValue"/>.
    /// </summary>
    [Serializable()]
    public abstract class FrequencyDefinitionBase : ChannelDefinitionBase, IFrequencyDefinition
    {
        #region [ Members ]

        // Fields
        private uint m_dfdtScale;
        private double m_dfdtOffset;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinitionBase"/>.</param>
        protected FrequencyDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinitionBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinitionBase"/>.</param>
        /// <param name="label">The label of this <see cref="FrequencyDefinitionBase"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="FrequencyDefinitionBase"/>.</param>
        /// <param name="dfdtScale">The df/dt scaling value of this <see cref="FrequencyDefinitionBase"/>.</param>
        /// <param name="dfdtOffset">The df/dt offset of this <see cref="FrequencyDefinitionBase"/>.</param>
        protected FrequencyDefinitionBase(IConfigurationCell parent, string label, uint scale, uint dfdtScale, double dfdtOffset)
            : base(parent, label, scale, 0.0D)
        {
            m_dfdtScale = dfdtScale;
            m_dfdtOffset = dfdtOffset;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize frequency definition
            m_dfdtScale = info.GetUInt32("dfdtScale");
            m_dfdtOffset = info.GetDouble("dfdtOffset");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat
        {
            get
            {
                return Parent.FrequencyDataFormat;
            }
        }

        /// <summary>
        /// Gets the nominal <see cref="LineFrequency"/> of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Value returned is the <see cref="IConfigurationCell.NominalFrequency"/> and is exposed here just for convenience.
        /// </remarks>
        public virtual LineFrequency NominalFrequency
        {
            get
            {
                return Parent.NominalFrequency;
            }
        }

        /// <summary>
        /// Gets or sets the index of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Phasor protocols only define one frequency measurement per device.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Index
        {
            get
            {
                return base.Index;
            }
            set
            {
                base.Index = value;
            }
        }

        /// <summary>
        /// Gets or sets the offset of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Offset for frequency values will always be the nominal frequency as defined in parent configuration cell; assigning a value is not supported.
        /// </remarks>
        /// <exception cref="NotSupportedException">Frequency offset is read-only; value is determined by nominal frequency specified in containing configuration cell.</exception>
        public override double Offset
        {
            get
            {
                return (double)Parent.NominalFrequency;
            }
            set
            {
                throw new NotSupportedException("Frequency offset is read-only; value is determined by nominal frequency specified in containing configuration cell");
            }
        }

        /// <summary>
        /// Gets or sets the df/dt offset of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        public virtual double DfDtOffset
        {
            get
            {
                return m_dfdtOffset;
            }
            set
            {
                m_dfdtOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the df/dt scaling value of this <see cref="FrequencyDefinitionBase"/>.
        /// </summary>
        public virtual uint DfDtScalingValue
        {
            get
            {
                return m_dfdtScale;
            }
            set
            {
                m_dfdtScale = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="FrequencyDefinitionBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("df/dt Offset", DfDtOffset.ToString());
                baseAttributes.Add("df/dt Scaling Value", DfDtScalingValue.ToString());

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
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize frequency definition
            info.AddValue("dfdtScale", m_dfdtScale);
            info.AddValue("dfdtOffset", m_dfdtOffset);
        }

        #endregion
    }
}