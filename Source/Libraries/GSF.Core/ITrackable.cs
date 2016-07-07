//******************************************************************************************************
//  ITrackable.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/07/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;

namespace GSF
{
    /// <summary>
    /// Represents the change history for a property.
    /// </summary>
    public interface ITrackable : IChangeTracking
    {
        /// <summary>
        /// Gets the name of the property being tracked.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Gets the original value before the first change was made to the property.
        /// </summary>
        object OriginalValue { get; }

        /// <summary>
        /// Gets the current value after all changes have been applied to the property.
        /// </summary>
        object CurrentValue { get; }

        /// <summary>
        /// Erases all change history and reverts to the
        /// original value before any changes were made.
        /// </summary>
        void Revert();

        /// <summary>
        /// Erases all change history and sets the
        /// original value back to its initial value.
        /// </summary>
        void Reset();
    }
}
