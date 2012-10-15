//******************************************************************************************************
//  DataPagerUserControl.xaml.cs - Gbtc
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
using System.Windows.Data;
using System.Windows.Input;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for DataPagerUserControl.xaml
    /// </summary>
    public partial class DataPagerUserControl : UserControl
    {
        #region [ Properties ]

        //Dependency Properties

        /// <summary>
        /// <see cref="DependencyProperty"/> to display number of pages.
        /// </summary>
        public static readonly DependencyProperty PageCountProperty = DependencyProperty.Register("PageCount", typeof(int), typeof(DataPagerUserControl), new UIPropertyMetadata(0));

        /// <summary>
        /// <see cref="DependencyProperty"/> for current page number.
        /// </summary>
        public static readonly DependencyProperty CurrentPageNumberProperty = DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(DataPagerUserControl), new UIPropertyMetadata(0));

        /// <summary>
        /// <see cref="DependencyProperty"/> to bind to <see cref="FirstCommand"/>.
        /// </summary>
        public static readonly DependencyProperty FirstCommandProperty = DependencyProperty.Register("FirstCommand", typeof(ICommand), typeof(DataPagerUserControl), new PropertyMetadata(null));

        /// <summary>
        /// <see cref="DependencyProperty"/> to bind to <see cref="PreviousCommand"/>.
        /// </summary>
        public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register("PreviousCommand", typeof(ICommand), typeof(DataPagerUserControl), new PropertyMetadata(null));

        /// <summary>
        /// <see cref="DependencyProperty"/> to bind to <see cref="NextCommand"/>.
        /// </summary>
        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(DataPagerUserControl), new PropertyMetadata(null));

        /// <summary>
        /// <see cref="DependencyProperty"/> to bind to <see cref="LastCommand"/>.
        /// </summary>
        public static readonly DependencyProperty LastCommandProperty = DependencyProperty.Register("LastCommand", typeof(ICommand), typeof(DataPagerUserControl), new PropertyMetadata(null));

        /// <summary>
        /// <see cref="DependencyProperty"/> to bind to <see cref="ShowPageSize"/>.
        /// </summary>
        public static DependencyProperty ShowPageSizeProperty = DependencyProperty.Register("ShowPageSize", typeof(bool), typeof(DataPagerUserControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets an index of the current page displayed in UI.
        /// </summary>
        public int CurrentPageNumber
        {
            get
            {
                return (int)GetValue(CurrentPageNumberProperty);
            }
            set
            {
                SetValue(CurrentPageNumberProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets total number of pages.
        /// </summary>
        public int PageCount
        {
            get
            {
                return (int)GetValue(PageCountProperty);
            }
            set
            {
                SetValue(PageCountProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> for moving to the first page.
        /// </summary>
        public ICommand FirstCommand
        {
            get
            {
                return (ICommand)GetValue(FirstCommandProperty);
            }
            set
            {
                SetValue(FirstCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> for moving to the previous page.
        /// </summary>
        public ICommand PreviousCommand
        {
            get
            {
                return (ICommand)GetValue(PreviousCommandProperty);
            }
            set
            {
                SetValue(PreviousCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> for moving to the next page.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                return (ICommand)GetValue(NextCommandProperty);
            }
            set
            {
                SetValue(NextCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> for moving to the last page.
        /// </summary>
        public ICommand LastCommand
        {
            get
            {
                return (ICommand)GetValue(LastCommandProperty);
            }
            set
            {
                SetValue(LastCommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the boolean flag which determines whether
        /// the page size should be shown on the control.
        /// </summary>
        public bool ShowPageSize
        {
            get
            {
                return (bool)this.GetValue(ShowPageSizeProperty);
            }
            set
            {
                this.SetValue(ShowPageSizeProperty, value);
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of <see cref="DataPagerUserControl"/>.
        /// </summary>
        public DataPagerUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this <see cref="System.Windows.FrameworkElement"/>
        /// has been updated. The specific dependency property that changed is reported in the arguments parameter.
        /// Overrides <see cref="System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)"/>.
        /// </summary>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ShowPageSizeProperty)
            {
                PageSizeTextLabel.Visibility = ShowPageSize ? Visibility.Visible : Visibility.Collapsed;
                PageSizeLabel.Visibility = ShowPageSize ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CurrentPageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                BindingExpression bindingExpression = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);

                if (bindingExpression != null)
                    bindingExpression.UpdateSource();
            }
        }

        #endregion
    }
}
