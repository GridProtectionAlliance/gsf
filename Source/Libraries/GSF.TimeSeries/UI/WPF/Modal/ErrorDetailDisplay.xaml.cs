//******************************************************************************************************
//  ErrorDetailDisplay.xaml.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/09/2012 - prasanthgs
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Input;

namespace GSF.TimeSeries.UI.Modal
{
    /// <summary>
    /// Interaction logic for ErrorDetailDisplay.xaml
    /// </summary>
    public partial class ErrorDetailDisplay : Window
    {
        #region [ Members ]

        /// <summary>
        /// Dependency property for <see cref="Message"/>.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(ErrorDetailDisplay), new UIPropertyMetadata(string.Empty));

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of <see cref="ErrorDetailDisplay"/> class.
        /// </summary>
        public ErrorDetailDisplay(string message)
        {
            InitializeComponent();
            this.DataContext = this;
            Message = message;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets and Sets <see cref="ICommand"/> for error message details.
        /// </summary>
        /// 
        public string Message
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        #endregion
    }
}
