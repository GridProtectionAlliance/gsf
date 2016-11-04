//******************************************************************************************************
//  RelayCommand.cs - Gbtc
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
//  03/28/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//       This code is based on MVVM Design Pattern article by Josh Smith - MSDN magazine, Feb 2009.
//  04/11/2011 - J. Ritchie Carroll
//       Created delegate wrapper contructor for common case when methods use no parameters.
//       Added code comments.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Windows.Input;

namespace GSF.TimeSeries.UI.Commands
{
    /// <summary>
    /// Defines a relay <see cref="ICommand"/>.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        // Fields
        private readonly Action<object> m_execute;
        private readonly Predicate<object> m_canExecute;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RelayCommand"/> for the common case delegates.
        /// </summary>
        /// <param name="execute">Execute method pointer.</param>
        /// <param name="canExecute">Can execute method pointer.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            // Create lambda proxies for delegates that use no parameters
            m_execute = param => execute();

            if (canExecute == null)
                m_canExecute = null;
            else
                m_canExecute = param => canExecute();
        }

        /// <summary>
        /// Creates a new <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">Execute method pointer.</param>
        /// <param name="canExecute">Can execute method pointer.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            m_execute = execute;
            m_canExecute = canExecute;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the <see cref="RelayCommand"/>. If the <see cref="RelayCommand"/> does not require
        /// data to be passed, this object can be set to <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if this <see cref="RelayCommand"/> can be executed; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object parameter)
        {
            return m_canExecute == null ? true : m_canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the <see cref="RelayCommand"/>. If the <see cref="RelayCommand"/> does not require
        /// data to be passed, this object can be set to <c>null</c>.
        /// </param>
        public void Execute(object parameter)
        {
            m_execute(parameter);
        }

        #endregion
    }
}
