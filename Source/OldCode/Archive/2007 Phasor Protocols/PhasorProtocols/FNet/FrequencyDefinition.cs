//*******************************************************************************************************
//  FrequencyDefinition.cs
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
//  02/08/2007 - James R Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.FNet
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable()]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(ConfigurationCell parent, string label)
            : base(parent, label, 1, 1, 0.0D)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.
        /// </summary>
        public virtual new ConfigurationCell Parent
        {
            get
            {
                return base.Parent as ConfigurationCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        #endregion
    }
}