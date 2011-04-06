//******************************************************************************************************
//  CompanyUserControl.xaml.cs - Gbtc
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
//  03/31/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TimeSeriesFramework.UI.DataModels;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for CompanyUserControl.xaml
    /// </summary>
    public partial class CompanyUserControl : UserControl
    {
        Companies m_companies;

        public CompanyUserControl()
        {
            InitializeComponent();            
            m_companies = new Companies();
            this.DataContext = m_companies;
            m_companies.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_companies_PropertyChanged);        
            DataPager.ItemsSource = new ObservableCollection<object>(m_companies.CompanyList);
        }

        void m_companies_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {        
            if (e.PropertyName == "CompanyList")
                DataPager.ItemsSource = new ObservableCollection<object>(m_companies.CompanyList);            
        }

        private void GridCompanyDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            m_companies.CurrentCompany = (Company)GridCompanyDetail.DataContext;
        }

    }
}
