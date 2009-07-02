//*******************************************************************************************************
//  DigitalDefinitionBase.cs
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
using System.Runtime.Serialization;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of a <see cref="IDigitalValue"/>.
    /// </summary>
    [Serializable()]
    public abstract class DigitalDefinitionBase : ChannelDefinitionBase, IDigitalDefinition
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinitionBase"/>.</param>
        protected DigitalDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
            ScalingValue = 1;
            Offset = 0.0D;
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinitionBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="DigitalDefinitionBase"/>.</param>
        /// <param name="label">The label of this <see cref="DigitalDefinitionBase"/>.</param>
        protected DigitalDefinitionBase(IConfigurationCell parent, string label)
            : base(parent, label, 1, 0.0D)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Data format for digital values will always be <see cref="PhasorProtocols.DataFormat.FixedInteger"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override DataFormat DataFormat
        {
            get
            {
                return PhasorProtocols.DataFormat.FixedInteger;
            }
        }

        /// <summary>
        /// Gets or sets the offset of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Offset for digital values will always be 0; assigning a value other than 0 will thrown an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">Digital values represent bit flags and thus do not support an offset.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override double Offset
        {
            get
            {
                return base.Offset;
            }
            set
            {
                if (value == 0)
                    base.Offset = value;
                else
                    throw new NotImplementedException("Digital values represent bit flags and thus do not support an offset");
            }
        }

        /// <summary>
        /// Gets or sets the integer scaling value of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Scaling value for digital values will always be 1; assigning a value other than 1 will thrown an exception.
        /// </remarks>
        /// <exception cref="NotImplementedException">Digital values represent bit flags and thus are not scaled.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override uint ScalingValue
        {
            get
            {
                return base.ScalingValue;
            }
            set
            {
                if (value == 1)
                    base.ScalingValue = value;
                else
                    throw new NotImplementedException("Digital values represent bit flags and thus are not scaled");
            }
        }

        /// <summary>
        /// Gets the scale/bit for the <see cref="ScalingValue"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Scale/bit for digital values will always be 1.0.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override double ScalePerBit
        {
            get
            {
                return 1.0D;
            }
        }

        #endregion
    }
}