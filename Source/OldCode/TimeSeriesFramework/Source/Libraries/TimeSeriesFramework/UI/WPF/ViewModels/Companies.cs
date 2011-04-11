//******************************************************************************************************
//  Companies.cs - Gbtc
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

using System;
using System.Windows;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Company"/> collection and selected company for UI.
    /// </summary>
    internal class Companies : PagedViewModelBase<Company, int>, IViewModel
    {
        #region [ Members ]

        // Fields        
        private RelayCommand m_saveCommand, m_deleteCommand, m_clearCommand;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructor to generate collection of <see cref="Company"/> defined in the data source.
        /// </summary>
        public Companies()
        {
            Load();
        }

        #endregion

        #region [ Properties ]

        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        #endregion

        #region [ Methods ]

        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        public override string GetCurrentItemName()
        {
            return CurrentItem.Name;
        }

        public void Delete()
        {
            if (CurrentItem.ID > 0 && Confirm("Are you sure you want to delete " + CurrentItem.Acronym + "?", "Delete Company"))
            {
                try
                {
                    string result = Company.Delete(null, CurrentItem.ID);
                    Load();
                    Popup(result, "Delete Company", MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Delete Company - ERROR!", MessageBoxImage.Error);
                }
            }
        }

        public void Clear()
        {
            CurrentItem = new Company();
        }

        #endregion
    }
}