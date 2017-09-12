//******************************************************************************************************
//  MenuCommand.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Modified Execute() method to set main window's title property.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using GSF.IO;
using GSF.Security;

namespace GSF.TimeSeries.UI.Commands
{
    /// <summary>
    /// Exposes <see cref="ICommand"/> object to be used to handle menu item click event.
    /// </summary>
    public class MenuCommand : ICommand
    {
        #region [ Members ]

        // Fields
        private string m_roles;
        private string m_userControlAssembly;
        private string m_userControlPath;
        private string m_description;

        //Events

        /// <summary>
        /// Raises when value of <see cref="CanExecute"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MenuCommand"/> class.
        /// </summary>
        public MenuCommand()
        {
            CommonFunctions.CurrentPrincipalRefreshed += (sender, args) => OnCanExecuteChanged();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets comma seperated values of roles for which menuitem is visible.
        /// </summary>
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
        /// Gets or sets name of the assembly where user control belongs to.
        /// </summary>
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
        /// Gets or sets name of the user control to be loaded.
        /// </summary>
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
        /// Gets or sets the description of associated <see cref="MenuDataItem"/>.
        /// </summary>
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Evaluates if menu item should be visible to current user with access to <see cref="Roles"/>.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the <see cref="MenuCommand"/>. If the <see cref="MenuCommand"/> does not require
        /// data to be passed, this object can be set to <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if this <see cref="MenuCommand"/> can be executed; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object parameter)
        {
            SecurityPrincipal currentPrincipal = CommonFunctions.CurrentPrincipal;

            return currentPrincipal.Identity.IsAuthenticated &&
                (string.IsNullOrEmpty(Roles) || Roles == "*" || currentPrincipal.IsInRole(Roles));
        }

        /// <summary>
        /// Handles <see cref="ICommand"/> action. 
        /// Loads user control as defined in the <see cref="UserControlPath"/> property from assembly name set in the 
        /// <see cref="UserControlAssembly"/> property.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the <see cref="MenuCommand"/>. If the <see cref="MenuCommand"/> does not require
        /// data to be passed, this object can be set to <c>null</c>.
        /// </param>
        public void Execute(object parameter)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(m_userControlAssembly));
                CommonFunctions.LoadUserControl(m_description, assembly.GetType(m_userControlPath));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to create user control {0}: {1}", m_userControlPath, ex.Message), ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion
    }
}
