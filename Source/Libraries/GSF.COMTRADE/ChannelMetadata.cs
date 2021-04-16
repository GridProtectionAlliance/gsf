//******************************************************************************************************
//  ChannelMetadata.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/19/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using GSF.Units.EE;

namespace GSF.COMTRADE
{
    /// <summary>
    /// Defines a minimal set of channel metadata for a COMTRADE config file.
    /// </summary>
    public struct ChannelMetadata
    {
        /// <summary>
        /// Channel name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Channel signal type.
        /// </summary>
        public SignalType SignalType;

        /// <summary>
        /// Determines if channel is a digital.
        /// </summary>
        /// <remarks>
        /// Channel is assumed to be an analog if field is <c>false</c>.
        /// </remarks>
        public bool IsDigital;

        /// <summary>
        /// Defines units to use for channel.
        /// </summary>
        /// <remarks>
        /// Leave <c>null</c> for defaults.
        /// </remarks>
        public string Units;

        /// <summary>
        /// Defines circuit component channel.
        /// </summary>
        /// <remarks>
        /// Leave <c>null</c> for none.
        /// </remarks>
        public string CircuitComponent;
    }

    /// <summary>
    /// Defines a comparison class to property sort metadata.
    /// </summary>
    public class ChannelMetadataSorter : IComparer<ChannelMetadata>
    {
        /// <summary>
        /// Compares one metadata record to another.
        /// </summary>
        /// <param name="left">Left metadata record to compare.</param>
        /// <param name="right">Right metadata record to compare.</param>
        /// <returns>Comparison sort order of metadata record.</returns>
        public int Compare(ChannelMetadata left, ChannelMetadata right)
        {
            // Make sure digitals (status flags first) fall behind analogs. All
            // values not in the dictionary will return 0 thus sorting higher.
            s_sortOrder.TryGetValue(left.SignalType, out int leftIndex);
            s_sortOrder.TryGetValue(right.SignalType, out int rightIndex);

            return leftIndex.CompareTo(rightIndex);
        }

        /// <summary>
        /// Default instance of the metadata record sorter.
        /// </summary>
        public static readonly ChannelMetadataSorter Default;

        private static readonly Dictionary<SignalType, int> s_sortOrder;

        static ChannelMetadataSorter()
        {
            int index = 1;

            // Define proper sort order for key signal types. Status flags are types of digital fields, but
            // they are stored as a 32-bit value with an abstracted set of flags (high order) as well as the
            // original flags (low order), since they are greater than 16-bits they are defined in the historian
            // as an analog value. Even so they need to sort as a digital.
            s_sortOrder = new Dictionary<SignalType, int>
            {
                { SignalType.FLAG, index++ },  // Status Flags
                { SignalType.DIGI, index }     // Digital Value
            };

            Default = new ChannelMetadataSorter();
        }
    }
}
