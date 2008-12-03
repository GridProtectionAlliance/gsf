//*******************************************************************************************************
//  ParsingState.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/17/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.Parsing
{
    /// <summary>
    /// Represents the base class for a binary image header implementation.
    /// </summary>
    /// <remarks>
    /// Typical implementations will have a constructor similar to the following:
    /// <code>
    /// public BinaryImageHeader(byte[] binaryImage, int startIndex, int length)
    /// {
    ///     m_typeID = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of the identifier.</typeparam>
    public abstract class BinaryImageHeaderBase<TTypeIdentifier> : ICommonHeader<TTypeIdentifier>
    {
        /// <summary>
        /// Internal field that stores the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        protected TTypeIdentifier m_typeID;
        
        /// <summary>
        /// Gets or sets the identifier used for identifying the <see cref="Type"/> to be parsed.
        /// </summary>
        public virtual TTypeIdentifier TypeID
        {
            get
            {
                return m_typeID;
            }
            set
            {
                m_typeID = value;
            }
        }
    }
}
