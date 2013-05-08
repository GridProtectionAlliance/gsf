//******************************************************************************************************
//  AdvancedSearchUserControl.xaml.cs - Gbtc
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
//  10/25/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Category to search by.
    /// </summary>
    public class AdvancedSearchCategory : DependencyObject
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the flag that indicates whether the category is selected.
        /// </summary>
        public bool Selected
        {
            get
            {
                return (bool)GetValue(SelectedProperty);
            }
            set
            {
                SetValue(SelectedProperty, value);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Dependency property for the <see cref="Name"/> property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(AdvancedSearchCategory));

        /// <summary>
        /// Dependency property for the <see cref="Selected"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("Selected", typeof(bool), typeof(AdvancedSearchCategory));

        #endregion
    }

    /// <summary>
    /// Interaction logic for AdvancedSearchUserControl.xaml
    /// </summary>
    public partial class AdvancedSearchUserControl : UserControl
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AdvancedSearchUserControl"/>.
        /// </summary>
        public AdvancedSearchUserControl()
        {
            SetValue(CategoriesPropertyKey, new ObservableCollection<AdvancedSearchCategory>());
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the text by which to search.
        /// </summary>
        public string SearchText
        {
            get
            {
                return (string)GetValue(SearchTextProperty);
            }
            set
            {
                SetValue(SearchTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to ignore casing when searching.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return (bool)GetValue(IgnoreCaseProperty);
            }
            set
            {
                SetValue(IgnoreCaseProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to use wildcards in searches.
        /// </summary>
        public bool UseWildcards
        {
            get
            {
                return (bool)GetValue(UseWildcardsProperty);
            }
            set
            {
                SetValue(UseWildcardsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to interpret search tokens as regular expressions.
        /// </summary>
        public bool UseRegex
        {
            get
            {
                return (bool)GetValue(UseRegexProperty);
            }
            set
            {
                SetValue(UseRegexProperty, value);
            }
        }

        /// <summary>
        /// Gets the collection of categories displayed in the second tab of the <see cref="AdvancedSearchUserControl"/>.
        /// </summary>
        public ObservableCollection<AdvancedSearchCategory> Categories
        {
            get
            {
                return (ObservableCollection<AdvancedSearchCategory>)GetValue(CategoriesProperty);
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        private static readonly DependencyPropertyKey CategoriesPropertyKey = DependencyProperty.RegisterReadOnly("Categories", typeof(ObservableCollection<AdvancedSearchCategory>), typeof(AdvancedSearchUserControl), new PropertyMetadata());

        /// <summary>
        /// Dependency property for the <see cref="SearchText"/> property.
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(AdvancedSearchUserControl));

        /// <summary>
        /// Dependency property for the <see cref="IgnoreCase"/> property.
        /// </summary>
        public static readonly DependencyProperty IgnoreCaseProperty = DependencyProperty.Register("IgnoreCase", typeof(bool), typeof(AdvancedSearchUserControl));

        /// <summary>
        /// Dependency property for the <see cref="UseWildcards"/> property.
        /// </summary>
        public static readonly DependencyProperty UseWildcardsProperty = DependencyProperty.Register("UseWildcards", typeof(bool), typeof(AdvancedSearchUserControl));

        /// <summary>
        /// Dependency property for the <see cref="UseRegex"/> property.
        /// </summary>
        public static readonly DependencyProperty UseRegexProperty = DependencyProperty.Register("UseRegex", typeof(bool), typeof(AdvancedSearchUserControl));

        /// <summary>
        /// Dependency property for the <see cref="Categories"/> property.
        /// </summary>
        public static readonly DependencyProperty CategoriesProperty = CategoriesPropertyKey.DependencyProperty;

        #endregion
    }
}
