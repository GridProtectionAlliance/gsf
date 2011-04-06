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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Company"/> collection and selected company for UI.
    /// </summary>
    internal class Companies : ViewModelBase, IViewModel
    {
        #region [ Members ]

        private Company m_currentCompany;
        private ObservableCollection<Company> m_companyList;
        private RelayCommand m_saveCommand, m_deleteCommand, m_clearCommand;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructor to generate collection of <see cref="Company"/> defined in the data source.
        /// </summary>
        public Companies()
        {
            Get();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Represents current company selected in the detail view for modification.
        /// </summary>
        public Company CurrentCompany
        {
            get { return m_currentCompany; }
            set
            {
                m_currentCompany = value;
                NotifyPropertyChanged("CurrentCompany");
            }
        }
        
        /// <summary>
        /// Reperesents list of companies defined in the database.
        /// </summary>
        public ObservableCollection<Company> CompanyList
        {
            get { return m_companyList; }
            set
            {
                m_companyList = value;
                NotifyPropertyChanged("CompanyList");
            }
        }

        #region [ IViewModel Properties ]

        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                    m_saveCommand = new RelayCommand(param => this.Save(), param => this.CanSave);

                return m_saveCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (m_deleteCommand == null)
                    m_deleteCommand = new RelayCommand(param => this.Delete(), param => this.CanDelete);

                return m_deleteCommand;
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                if (m_clearCommand == null)
                    m_clearCommand = new RelayCommand(param => this.Clear(), param => this.CanClear);

                return m_clearCommand;
            }
        }

        public bool CanSave
        {
            get { return m_currentCompany.IsValid; }
        }

        public bool CanDelete
        {
            get { return true; }
        }

        public bool CanClear
        {
            get { return true; }
        }

        #endregion

        #endregion

        #region [ Methods ]

        #region [ IViewModel Implementation ]

        public void Get()
        {
            CompanyList = CommonFunctions.GetCompanyList(null);

            if (CompanyList.Count > 0)
                CurrentCompany = CompanyList[0];
            else
                CurrentCompany = new Company();
        }

        public void Save()
        {
            try
            {
                string result = CommonFunctions.SaveCompany(null, m_currentCompany, m_currentCompany.ID > 0 ? false : true);
                Popup(result, "Save Company", MessageBoxImage.Information);
                Get();
            }
            catch (Exception ex)
            {
                Popup(ex.Message, "Save Company - ERROR!", MessageBoxImage.Error);
            }
        }

        public void Delete()
        {
            if (m_currentCompany.ID > 0 && Confirm("Are you sure you want to delete " + m_currentCompany.Acronym + "?", "Delete Company"))
            {
                try
                {
                    string result = CommonFunctions.DeleteCompany(null, m_currentCompany.ID);                    
                    Get();
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
            CurrentCompany = new Company();            
        }

        #endregion

        #endregion
    }
}
