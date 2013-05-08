//******************************************************************************************************
//  ResizableWindow.cs - Gbtc
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
//  10/04/2011 - mthakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GSF.Windows;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents a resizable window.
    /// </summary>
    public class ResizableWindow : SecureWindow
    {
        #region [ Members ]

        private ScaleTransform m_rootScale;
        private const double layoutRootHeight = 800;
        private const double layoutRootWidth = 900;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ScaleTransform"/> associated with <see cref="Canvas"/> element.
        /// </summary>
        public virtual ScaleTransform RootScale
        {
            get
            {
                return m_rootScale;
            }
            set
            {
                m_rootScale = value;
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="ResizableWindow"/>.
        /// </summary>
        public ResizableWindow()
        {
            this.SizeChanged += ResizableWindow_SizeChanged;
        }

        #endregion

        #region [ Methods ]

        private void GetRenderTransform()
        {
            UIElement element = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(Canvas), ref element);
            if (element != null && element is Canvas)
            {
                try
                {
                    RootScale = (ScaleTransform)((Canvas)element).RenderTransform;
                }
                catch
                {
                    RootScale = null;
                }
            }
        }

        /// <summary>
        /// Handles SizeChanged event of the window.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void ResizableWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RootScale == null)
                GetRenderTransform();

            if (RootScale != null)
            {
                RootScale.ScaleX = e.NewSize.Width / layoutRootWidth;
                RootScale.ScaleY = e.NewSize.Height / layoutRootHeight;
            }
        }

        #endregion
    }
}
