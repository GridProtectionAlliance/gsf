//*******************************************************************************************************
//  DigitalValue.cs
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
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IDigitalValue"/>.
    /// </summary>
    [Serializable()]
    public class DigitalValue : DigitalValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalValue"/>.
        /// </summary>
        protected DigitalValue()
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

        /// <summary>
        /// Creates a new <see cref="DigitalValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="DigitalValue"/>.</param>
        /// <param name="digitalDefinition">The <see cref="IDigitalDefinition"/> associated with this <see cref="DigitalValue"/>.</param>
        /// <param name="value">The real value of this <see cref="DigitalValue"/>.</param>
        public DigitalValue(IDataCell parent, IDigitalDefinition digitalDefinition, short value)
            : base(parent, digitalDefinition, value)
        {
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 digital value
        internal static IDigitalValue CreateNewValue(IDataCell parent, IDigitalDefinition definition, byte[] binaryImage, int startIndex)
        {
            IDigitalValue digital = new DigitalValue() { Parent = parent, Definition = definition };

            digital.Initialize(binaryImage, startIndex, 0);

            return digital;
        }

        #endregion       
    }
}