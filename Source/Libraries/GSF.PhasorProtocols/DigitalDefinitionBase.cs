//******************************************************************************************************
//  DigitalDefinitionBase.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent definition of a <see cref="IDigitalValue"/>.
    /// </summary>
    [Serializable]
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
        /// Gets the <see cref="GSF.PhasorProtocols.DataFormat"/> of this <see cref="DigitalDefinitionBase"/>.
        /// </summary>
        /// <remarks>
        /// Data format for digital values will always be <see cref="GSF.PhasorProtocols.DataFormat.FixedInteger"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override DataFormat DataFormat => DataFormat.FixedInteger;

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
            get => base.Offset;
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
            get => base.ScalingValue;
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
        public sealed override double ScalePerBit => 1.0D;

    #endregion
    }
}