//******************************************************************************************************
//  IChannelCollection.cs - Gbtc
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a collection of any <see cref="IChannel"/> objects.<br/>
    /// This is the base interface implemented by all collections classes in the phasor protocols library; it is the root of
    /// the collection interface hierarchy.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IChannel"/> type that the <see cref="IChannelCollection{T}"/> contains.</typeparam>
    public interface IChannelCollection<T> : IChannel, IList<T>, INotifyCollectionChanged, ISerializable where T : IChannel
    {
        // Note that the channel collection interface inherits IChannel hence providing cumulative imaging properties
        
        /// <summary>
        /// Gets flag that indicates if collection elements have a fixed size.
        /// </summary>
        bool FixedElementSize { get; }
    }
}
