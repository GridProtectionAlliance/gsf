//******************************************************************************************************
//  RelayCommand.cs - Gbtc
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
//       This code was taken from MVVM Design Pattern article by Josh Smith in MSDN magazine February 2009 issue.
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace TimeSeriesFramework.UI.Commands
{
    public class RelayCommand : ICommand
    {
        #region [ Members ]

        private readonly Action<object> m_execute;
        private readonly Predicate<object> m_canExecute;

        #endregion 

        #region [ Constructors ]

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            m_execute = execute;
            m_canExecute = canExecute;
        }

        #endregion

        #region [ ICommand Members ]

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return m_canExecute == null ? true : m_canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            m_execute(parameter);
        }

        #endregion 
    }
}
