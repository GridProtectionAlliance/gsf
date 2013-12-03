//******************************************************************************************************
//  MenuItemStyleSelector.cs - Gbtc
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
//  07/27/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents wrapper object around <see cref="StyleSelector"/> class.
    /// </summary>
    public class MenuItemStyleSelector : StyleSelector
    {
        /// <summary>
        /// Overrides SelectStyle function and returns proper style element base on menutext.
        /// </summary>
        /// <param name="item"><see cref="MenuDataItem"/> for which style is to be determined.</param>
        /// <param name="container"><see cref="FrameworkElement"/> containing <see cref="MenuDataItem"/>.</param>
        /// <returns><see cref="Style"/> based on MenuText.</returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement frameworkElement = container as FrameworkElement;
            MenuDataItem menuDataItem = item as MenuDataItem;

            if (((object)frameworkElement != null) && ((object)menuDataItem != null))
            {
                if (string.IsNullOrEmpty(menuDataItem.MenuText))
                    return frameworkElement.FindResource("MenuSeparatorStyle") as Style;
                else
                    return frameworkElement.FindResource("MenuItemStyle") as Style;
            }

            return null;
        }
    }
}
