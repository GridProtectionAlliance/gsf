//******************************************************************************************************
//  MenuDataItem.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  03/28/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System;
using TimeSeriesFramework.UI.Commands;

namespace TimeSeriesFramework.UI.Utility
{
    /// <summary>
    /// Represents a menu item in a WPF Menu control.
    /// </summary>           
    public class MenuDataItem
    {
        #region [ Properties ]
        
        /// <summary>
        /// Gets or sets path to icon image.
        /// </summary>
        [XmlAttribute]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets text of menu item to be displayed in menu.
        /// </summary>
        [XmlAttribute]
        public string MenuText { get; set; }

        /// <summary>
        /// Gets or sets comma seperated list of roles with access to this <see cref="MenuDataItem"/>.
        /// </summary>                
        [XmlAttribute]
        public string Roles { get; set; }

        /// <summary>
        /// Gets or sets the assembly name where user control is defined.
        /// </summary>
        [XmlAttribute]
        public string UserControlAssembly { get; set; }

        /// <summary>
        /// Gets or sets path for the user control to be loaded when this <see cref="MenuDataItem"/> is clicked.
        /// </summary>        
        [XmlAttribute]
        public string UserControlPath { get; set; }

        /// <summary>
        /// Gets or sets the commnad to execute when this <see cref="MenuDataItem"/> is clicked.
        /// </summary> 
        [XmlIgnore]
        public ICommand Command 
        {
            get { return new MenuCommand() { Roles = this.Roles, UserControlPath = this.UserControlPath }; }
        }

        /// <summary>
        /// Gets or sets sub menu items for <see cref="MenuDataItem"/>.
        /// </summary>         
        public ObservableCollection<MenuDataItem> SubMenuItems { get; set; }

        #endregion
    }
}
