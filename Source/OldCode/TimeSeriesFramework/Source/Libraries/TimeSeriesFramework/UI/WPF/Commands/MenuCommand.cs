//******************************************************************************************************
//  MenuCommand.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading;
using System.Windows.Input;

namespace TimeSeriesFramework.UI.Commands
{
    /// <summary>
    /// Exposes <see cref="ICommand"/> object to be used to handle menu item click event.
    /// </summary>
    public class MenuCommand : ICommand
    {
        #region [ Members ]

        //Fields
        private string m_roles;

        //Events
        /// <summary>
        /// Raises when value of <see cref="CanExecute"/> changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

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

        #endregion

        #region [ Methods ]

        #region [ ICommand Implementation ]

        /// <summary>
        /// Evaluates if menu item should be visible to current user with access to <see cref="Roles"/>.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute (object parameter)
        {
            if (string.IsNullOrEmpty(Roles) || Roles == "*")
                return true;
            else
                return Thread.CurrentPrincipal.IsInRole(Roles);
        }

        /// <summary>
        /// Handles <see cref="ICommand"/> action.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
        }

        #endregion

        #endregion
    }
}
