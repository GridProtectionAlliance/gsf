//******************************************************************************************************
//  PhasorValue.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Units;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IPhasorValue"/>.
    /// </summary>
    [Serializable]
    public class PhasorValue : PhasorValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition)
            : base(parent, phasorDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="real">The real value of this <see cref="PhasorValue"/>.</param>
        /// <param name="imaginary">The imaginary value of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, double real, double imaginary)
            : base(parent, phasorDefinition, real, imaginary)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="angle">The <see cref="Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValue"/>, in radians.</param>
        /// <param name="magnitude">The magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, Angle angle, double magnitude)
            : base(parent, phasorDefinition, angle, magnitude)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.
        /// </summary>
        public new virtual DataCell Parent
        {
            get => base.Parent as DataCell;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.
        /// </summary>
        public new virtual PhasorDefinition Definition
        {
            get => base.Definition as PhasorDefinition;
            set => base.Definition = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValue"/>, in radians.
        /// </summary>
        public override Angle Angle
        {
            get => Angle.FromDegrees(base.Angle.ToDegrees() + Definition.Offset);
            set => base.Angle = Angle.FromDegrees(value.ToDegrees() - Definition.Offset);
        }

        /// <summary>
        /// Gets or sets the magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValue"/>.
        /// </summary>
        public override double Magnitude
        {
            get => base.Magnitude * PhasorDefinition.CustomConversionFactor(Definition);
            set => base.Magnitude = value / PhasorDefinition.CustomConversionFactor(Definition);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Calculates binary length of a phasor value based on its definition
        internal static uint CalculateBinaryLength(IPhasorDefinition definition)
        {
            // The phasor definition will determine the binary length based on data format
            return (uint)new PhasorValue(null, definition).BinaryLength;
        }

        // Delegate handler to create a new BPA PDCstream phasor value
        internal static IPhasorValue CreateNewValue(IDataCell parent, IPhasorDefinition definition, byte[] buffer, int startIndex, out int parsedLength)
        {
            IPhasorValue phasor = new PhasorValue(parent, definition);

            parsedLength = phasor.ParseBinaryImage(buffer, startIndex, 0);

            return phasor;
        }

        #endregion
    }
}