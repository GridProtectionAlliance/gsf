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

using System.Collections.ObjectModel;
using TimeSeriesFramework.UI.DataModel;

namespace TimeSeriesFramework.UI.ViewModel
{
    /// <summary>
    /// Class to hold bindable <see cref="Company"/> collection and selected company for UI.
    /// </summary>
    internal class Companies : ViewModelBase
    {
        #region [ Members ]

        private Company m_currentCompany;
        private ObservableCollection<Company> m_companyList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructor to generate collection of <see cref="Company"/> defined in the data source.
        /// </summary>
        public Companies()
        {
            CompanyList = new ObservableCollection<Company>(){ new Company(){ Acronym = "TVA", Name = "Tennessee Valley Authority", LoadOrder = 0, URL = "http://wwww.tva.gov/"},
                                                                 new Company(){ Acronym = "GPA", Name = "Grid Protection Alliance", LoadOrder = 1}
                                                                };

            //CompanyList = CommonFunctions.GetCompanyList(null);
            //if (CompanyList.Count > 0)
                CurrentCompany = CompanyList[0];

            //CurrentCompany = new Company();
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

        #endregion
    }
}
