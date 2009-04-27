//*******************************************************************************************************
//  DataCellCollection.cs
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
//  04/27/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents a SEL Fast Message implementation of a collection of <see cref="IDataCell"/> objects.
    /// </summary>
    [Serializable()]
    public class DataCellCollection : PhasorProtocols.DataCellCollection
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/>.
        /// </summary>
        public DataCellCollection()
            : base(0, true)
        {
            // SEL Fast Message only supports a single device - so there should only be one cell - since there's only one cell, cell lengths will be constant :)
        }

        /// <summary>
        /// Creates a new <see cref="DataCellCollection"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataCell"/> at specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of value to get or set.</param>
        public new DataCell this[int index]
        {
            get
            {
                return base[index] as DataCell;
            }
            set
            {
                base[index] = value;
            }
        }

        #endregion
    }
}