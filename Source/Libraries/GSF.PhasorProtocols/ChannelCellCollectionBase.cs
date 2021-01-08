//******************************************************************************************************
//  ChannelCellCollectionBase.cs - Gbtc
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
//  03/07/2005 - J. Ritchie Carroll
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent collection of <see cref="IChannelCell"/> objects.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannelCell"/> type that the <see cref="ChannelCellCollectionBase{T}"/> contains.</typeparam>
    [Serializable]
    public abstract class ChannelCellCollectionBase<T> : ChannelCollectionBase<T>, IChannelCellCollection<T> where T : IChannelCell
    {
        #region [ Members ]

        // Fields

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelCellCollectionBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="lastValidIndex">Last valid index for the collection (i.e., maximum count - 1).</param>
        /// <param name="constantCellLength">Sets flag that determines if the lengths of <see cref="IChannelCell"/> elements in this <see cref="ChannelCellCollectionBase{T}"/> are constant.</param>
        /// <remarks>
        /// <paramref name="lastValidIndex"/> is used instead of maximum count so that maximum type values may
        /// be specified as needed. For example, if the protocol specifies a collection with a signed 16-bit
        /// maximum length you can specify <see cref="short.MaxValue"/> (i.e., 32,767) as the last valid index
        /// for the collection since total number of items supported would be 32,768.
        /// </remarks>
        protected ChannelCellCollectionBase(int lastValidIndex, bool constantCellLength)
            : base(lastValidIndex)
        {
            ConstantCellLength = constantCellLength;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelCellCollectionBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelCellCollectionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize extra elements
            ConstantCellLength = info.GetBoolean("constantCellLength");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if the lengths of <see cref="IChannelCell"/> elements in this <see cref="ChannelCellCollectionBase{T}"/> are constant.
        /// </summary>
        public bool ConstantCellLength { get; }

        /// <summary>
        /// Gets the length of the <see cref="ChannelCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// The length of the <see cref="ChannelCellCollectionBase{T}"/> binary image is the combined length of all the items in the collection.
        /// </remarks>
        public override int BinaryLength
        {
            get
            {
                if (ConstantCellLength)
                {
                    // Cells will be constant length, so we can quickly calculate lengths
                    return base.BinaryLength;
                }

                // Cells will be different lengths, so we must manually sum lengths
                return this.Sum(frame => frame.BinaryLength);
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="ChannelCellCollectionBase{T}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Constant Cell Length", ConstantCellLength.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize extra elements
            info.AddValue("constantCellLength", ConstantCellLength);
        }

        #endregion
    }
}
