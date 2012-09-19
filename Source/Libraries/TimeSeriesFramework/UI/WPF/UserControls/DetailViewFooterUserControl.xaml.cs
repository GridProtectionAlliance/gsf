//******************************************************************************************************
//  DetailViewFooterUserControl.xaml.cs - Gbtc
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
//  04/12/2011 - mthakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DetailViewFooterUserControl.xaml
    /// </summary>
    public partial class DetailViewFooterUserControl : UserControl
    {
        #region [ Properties ]

        //Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> for delete operation.
        /// </summary>
        public static DependencyProperty DeleteCommandProperty = DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(DetailViewFooterUserControl), new PropertyMetadata(null));
        
        /// <summary>
        /// <see cref="DependencyProperty"/> for clear form operation.
        /// </summary>
        public static DependencyProperty ClearCommandProperty = DependencyProperty.Register("ClearCommand", typeof(ICommand), typeof(DetailViewFooterUserControl), new PropertyMetadata(null));
        
        /// <summary>
        /// <see cref="DependencyProperty"/> for save operation.
        /// </summary>
        public static DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(DetailViewFooterUserControl), new PropertyMetadata(null));
        
        /// <summary>
        /// <see cref="DependencyProperty"/> to determine if Save button is enabled on UI.
        /// </summary>
        public static DependencyProperty SaveEnabledProperty = DependencyProperty.Register("SaveEnabled", typeof(bool), typeof(DetailViewFooterUserControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to delete selected object.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return (ICommand)GetValue(DeleteCommandProperty);
            }
            set
            {
                SetValue(DeleteCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to reset selection.
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                return (ICommand)GetValue(ClearCommandProperty);
            }
            set
            {
                SetValue(ClearCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to save selected object information into backend datastore.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                return (ICommand)GetValue(SaveCommandProperty);
            }
            set
            {
                SetValue(SaveCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets boolean value indicating IsEnabled property of Save button.
        /// </summary>
        public bool SaveEnabled
        {
            get
            {
                return (bool)GetValue(SaveEnabledProperty);
            }
            set
            {
                SetValue(SaveEnabledProperty, value);
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of <see cref="DetailViewFooterUserControl"/>.
        /// </summary>
        public DetailViewFooterUserControl()
        {
            InitializeComponent();
        }

        #endregion
    }
}
