//******************************************************************************************************
//  AverageFrequencyUserControl.xaml.cs - Gbtc
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
//  08/20/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using GSF.PhasorProtocols.UI.ViewModels;

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AverageFrequencyUserControl.xaml
    /// </summary>
    public partial class AverageFrequencyUserControl : UserControl
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AverageFrequencyUserControl"/> class.
        /// </summary>
        public AverageFrequencyUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]
        
        // Gets the view model from the user control's resources.
        private AverageFrequencyViewModel ViewModel
        {
            get
            {
                return (AverageFrequencyViewModel)Resources["ViewModel"];
            }
        }

        #endregion

        #region [ Methods ]

        // Loads the view model.
        private void AverageFrequencyUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Load();
        }

        // Unloads the view model.
        private void AverageFrequencyUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Unload();
        }

        #endregion
    }
}
