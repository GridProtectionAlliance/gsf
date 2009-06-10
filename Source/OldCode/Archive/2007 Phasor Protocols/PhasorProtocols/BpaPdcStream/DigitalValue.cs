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

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of an <see cref="IDigitalValue"/>.
    /// </summary>
    [Serializable()]
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
        /// Gets or sets the <see cref="DigitalDefinition"/> associated with this <see cref="DigitalValue"/>.
        /// </summary>
        public virtual new DigitalDefinition Definition
        {
            get
            {
                return base.Definition as DigitalDefinition;
            }
            set
            {
                base.Definition = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream digital value
        internal static IDigitalValue CreateNewValue(IDataCell parent, IDigitalDefinition definition, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IDigitalValue digital = new DigitalValue(parent, definition);

            parsedLength = digital.Initialize(binaryImage, startIndex, 0);

            return digital;
        }

        #endregion
    }
}