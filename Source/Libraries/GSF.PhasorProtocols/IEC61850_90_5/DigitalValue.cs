//******************************************************************************************************
//  DigitalValue.cs - Gbtc
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
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.IEC61850_90_5
{
    /// <summary>
    /// Represents the IEC 61850-90-5 implementation of a <see cref="IDigitalValue"/>.
    /// </summary>
    [Serializable]
    public class DigitalValue : DigitalValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="DigitalValue"/>.</param>
        /// <param name="digitalDefinition">The <see cref="IDigitalDefinition"/> associated with this <see cref="DigitalValue"/>.</param>
        public DigitalValue(IDataCell parent, IDigitalDefinition digitalDefinition)
            : base(parent, digitalDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="DigitalValue"/>.</param>
        /// <param name="digitalDefinition">The <see cref="DigitalDefinition"/> associated with this <see cref="DigitalValue"/>.</param>
        /// <param name="value">The unsigned 16-bit integer value (composed of digital bits) that represents this <see cref="DigitalValue"/>.</param>
        public DigitalValue(DataCell parent, DigitalDefinition digitalDefinition, ushort value)
            : base(parent, digitalDefinition, value)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="DigitalValue"/>.
        /// </summary>
        public new virtual DataCell Parent
        {
            get => base.Parent as DataCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DigitalDefinition"/> associated with this <see cref="DigitalValue"/>.
        /// </summary>
        public new virtual DigitalDefinition Definition
        {
            get => base.Definition as DigitalDefinition;
            set => base.Definition = value;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEC 61850-90-5 digital value
        internal static IDigitalValue CreateNewValue(IDataCell parent, IDigitalDefinition definition, byte[] buffer, int startIndex, out int parsedLength)
        {
            IDigitalValue digital = new DigitalValue(parent, definition);

            parsedLength = digital.ParseBinaryImage(buffer, startIndex, 0);

            return digital;
        }

        #endregion
    }
}