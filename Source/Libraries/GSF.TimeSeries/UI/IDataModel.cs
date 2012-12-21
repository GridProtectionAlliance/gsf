//******************************************************************************************************
//  IDataModel.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/11/2011 - Ritchie
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ComponentModel;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents a data model entity.
    /// </summary>
    public interface IDataModel : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Indicates if the values associated with this object are valid.
        /// </summary>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Gets the default value specified by <see cref="DefaultValueAttribute"/>, if any, applied to the specified property. 
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Default value applied to specified property; or null if one does not exist.</returns>
        object GetDefaultValue(string propertyName);
    }
}
