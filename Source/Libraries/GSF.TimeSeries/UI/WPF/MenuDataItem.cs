//******************************************************************************************************
//  MenuDataItem.cs - Gbtc
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
//  03/28/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Serialization;
using GSF.TimeSeries.UI.Commands;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents a menu item in a WPF Menu control.
    /// </summary>           
    public class MenuDataItem
    {
        #region [ Members ]

        // Fields
        private string m_icon;
        private string m_menuText;
        private string m_description;
        private string m_roles;
        private string m_userControlAssembly;
        private string m_userControlPath;
        private MenuCommand m_command;
        private ObservableCollection<MenuDataItem> m_subMenuItems;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets path <see cref="MenuDataItem"/> image icon.
        /// </summary>
        [XmlAttribute]
        public string Icon
        {
            get
            {
                return m_icon;
            }
            set
            {
                m_icon = value;
            }
        }

        /// <summary>
        /// Gets or sets text of <see cref="MenuDataItem"/> to be displayed in menu.
        /// </summary>
        [XmlAttribute]
        public string MenuText
        {
            get
            {
                return m_menuText;
            }
            set
            {
                m_menuText = value;
            }
        }

        /// <summary>
        /// Gets or sets description of the <see cref="MenuDataItem"/>.
        /// </summary>
        [XmlAttribute]
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
            }
        }

        /// <summary>
        /// Gets or sets comma seperated list of roles with access to this <see cref="MenuDataItem"/>.
        /// </summary>                
        [XmlAttribute]
        public string Roles
        {
            get
            {
                return m_roles;
            }
            set
            {
                m_roles = value;
            }
        }

        /// <summary>
        /// Gets or sets the assembly name where user control is defined.
        /// </summary>
        [XmlAttribute]
        public string UserControlAssembly
        {
            get
            {
                return m_userControlAssembly;
            }
            set
            {
                m_userControlAssembly = value;
            }
        }

        /// <summary>
        /// Gets or sets path for the user control to be loaded when <see cref="MenuDataItem"/> is clicked.
        /// </summary>        
        [XmlAttribute]
        public string UserControlPath
        {
            get
            {
                return m_userControlPath;
            }
            set
            {
                m_userControlPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the commnad to execute when <see cref="MenuDataItem"/> is clicked.
        /// </summary> 
        [XmlIgnore]
        public ICommand Command
        {
            get
            {
                if ((object)m_command == null)
                    m_command = new MenuCommand();

                m_command.Roles = Roles;
                m_command.UserControlAssembly = UserControlAssembly;
                m_command.UserControlPath = UserControlPath;
                m_command.Description = Description;

                return m_command;
            }
        }

        /// <summary>
        /// Gets or sets sub menu items for <see cref="MenuDataItem"/>.
        /// </summary>         
        public ObservableCollection<MenuDataItem> SubMenuItems
        {
            get
            {
                return m_subMenuItems;
            }
            set
            {
                m_subMenuItems = value;
            }
        }

        #endregion
    }
}
