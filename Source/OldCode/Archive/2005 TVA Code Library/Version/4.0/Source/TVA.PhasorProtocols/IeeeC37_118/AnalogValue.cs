//*******************************************************************************************************
//  AnalogValue.cs
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

namespace TVA.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of an <see cref="IAnalogValue"/>.
    /// </summary>
    [Serializable()]
    public class AnalogValue : AnalogValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="AnalogValue"/>.</param>
        /// <param name="analogDefinition">The <see cref="IAnalogDefinition"/> associated with this <see cref="AnalogValue"/>.</param>
        public AnalogValue(IDataCell parent, IAnalogDefinition analogDefinition)
            : base(parent, analogDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="AnalogValue"/>.</param>
        /// <param name="analogDefinition">The <see cref="AnalogDefinition"/> associated with this <see cref="AnalogValue"/>.</param>
        /// <param name="value">The floating point value that represents this <see cref="AnalogValue"/>.</param>
        public AnalogValue(DataCell parent, AnalogDefinition analogDefinition, double value)
            : base(parent, analogDefinition, value)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="AnalogValue"/>.
        /// </summary>
        public virtual new DataCell Parent
        {
            get
            {
                return base.Parent as DataCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AnalogDefinition"/> associated with this <see cref="AnalogValue"/>.
        /// </summary>
        public virtual new AnalogDefinition Definition
        {
            get
            {
                return base.Definition as AnalogDefinition;
            }
            set
            {
                base.Definition = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 analog value
        internal static IAnalogValue CreateNewValue(IDataCell parent, IAnalogDefinition definition, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IAnalogValue analog = new AnalogValue(parent, definition);

            parsedLength = analog.Initialize(binaryImage, startIndex, 0);

            return analog;
        }

        #endregion
    }
}