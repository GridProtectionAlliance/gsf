//*******************************************************************************************************
//  CommonHeaderBase.cs
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

namespace TVA.Parsing
{
    /// <summary>
    /// Represents the base class for a common binary image header implementation.
    /// </summary>
    /// <remarks>
    /// Here is an example of a possible derived class constructor that has an integer TypeID as the
    /// first 4 bytes in the image:
    /// <code>
    /// public CommonHeader(object state, byte[] binaryImage, int startIndex, int length)
    /// {
    ///     if (length > 3)
    ///     {
    ///         TypeID = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
    ///         State = state;
    ///     }
    ///     else
    ///         throw new InvalidOperationException("Malformed image");
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    public abstract class CommonHeaderBase<TTypeIdentifier> : ICommonHeader<TTypeIdentifier>
    {
        private TTypeIdentifier m_typeID;
        private object m_state;
        
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

        /// <summary>
        /// Gets or sets any additional state information that might be needed for parsing.
        /// </summary>
        public object State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }
    }
}