//******************************************************************************************************
//  PanAndZoomViewer.xaml.cs - Gbtc
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
//  09/13/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Media.Imaging;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for PanAndZoomViewer.xaml
    /// </summary>
    public partial class PanAndZoomViewer : Window
    {
        #region [ Members ]

        private readonly BitmapImage m_image;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="PanAndZoomViewer"/>.
        /// </summary>
        /// <param name="image">Image to be displayed in <see cref="PanAndZoomViewer"/>.</param>
        /// <param name="title">Title of the <see cref="PanAndZoomViewer"/> window.</param>
        public PanAndZoomViewer(BitmapImage image, string title)
        {
            InitializeComponent();
            m_image = image;
            Title = title + " (Use mouse-wheel to zoom in and out, use mouse left button to drag)";
            Loaded += PanAndZoomViewer_Loaded;
        }

        #endregion

        #region [ Methods ]

        private void PanAndZoomViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //    this.Height = 0.9 * this.Owner.Height;
            //    this.Top = 0.05 * this.Owner.Height;
            //    this.Width = 0.7 * this.Owner.Width;
            //    this.Left = (0.15 * this.Owner.Width) + this.Owner.Left;

            if (m_image != null)
                DisplayImage.Source = m_image;
        }

        #endregion
    }
}
