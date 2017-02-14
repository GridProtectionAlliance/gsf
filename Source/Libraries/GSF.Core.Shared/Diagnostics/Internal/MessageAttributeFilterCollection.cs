//******************************************************************************************************
//  MessageAttributeFilterCollection.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/28/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Contains a collection of <see cref="MessageAttributeFilter"/>s with the <see cref="LogSubscriberInternal"/> 
    /// that is assigned to each segment.
    /// </summary>
    internal class MessageAttributeFilterCollection
        : MessageAttributeFilter
    {
        /// <summary>
        /// All of the routes that made up this filter collection
        /// </summary>
        public List<Tuple<MessageAttributeFilter, NullableWeakReference>> Routes = new List<Tuple<MessageAttributeFilter, NullableWeakReference>>();

        public void Add(MessageAttributeFilter filter, LogSubscriberInternal subscriber)
        {
            Routes.Add(Tuple.Create(filter, subscriber.Reference));
            Append(filter);
        }
    }
}