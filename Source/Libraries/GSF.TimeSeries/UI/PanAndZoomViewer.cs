//******************************************************************************************************
//  PanAndZoomViewer.cs - Gbtc
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
//  09/13/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// <see cref="ContentControl"/> class which allows deep zoom composoer type functionalities in WPF apps.
    /// </summary>
    public class PanAndZoom : ContentControl
    {
        #region [ Members ]

        // Fields
        private readonly double m_defaultZoomFactor;
        private FrameworkElement m_source;
        private Point m_screenStartPoint;
        private TranslateTransform m_translateTransform;
        private ScaleTransform m_zoomTransform;
        private TransformGroup m_transformGroup;
        private Point m_startOffset;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="PanAndZoom"/> class.
        /// </summary>
        public PanAndZoom()
        {
            m_screenStartPoint = new Point(0, 0);
            m_defaultZoomFactor = 1.4;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Overrides OnApplyTemplate method from <see cref="ContentControl"/> class.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Setup(this);
        }

        private void Setup(FrameworkElement control)
        {
            m_source = VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
            m_translateTransform = new TranslateTransform();
            m_zoomTransform = new ScaleTransform();
            m_transformGroup = new TransformGroup();
            m_transformGroup.Children.Add(m_zoomTransform);
            m_transformGroup.Children.Add(m_translateTransform);
            m_source.RenderTransform = m_transformGroup;
            this.Focusable = true;
            this.KeyDown += source_KeyDown;
            this.MouseMove += control_MouseMove;
            this.MouseDown += source_MouseDown;
            this.MouseUp += source_MouseUp;
            this.MouseWheel += source_MouseWheel;
        }

        private void source_KeyDown(object sender, KeyEventArgs e)
        {
            // hit escape to reset everything
            if (e.Key == Key.Escape) Reset();
        }

        private void source_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom into the content.  Calculate the zoom factor based on the direction of the mouse wheel.
            double zoomFactor = m_defaultZoomFactor;
            if (e.Delta <= 0) zoomFactor = 1.0 / m_defaultZoomFactor;

            // DoZoom requires both the logical and physical location of the mouse pointer
            Point physicalPoint = e.GetPosition(this);

            DoZoom(zoomFactor, m_transformGroup.Inverse.Transform(physicalPoint), physicalPoint);

        }

        private void source_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                // We're done.  reset the cursor and release the mouse pointer
                this.Cursor = Cursors.Arrow;
                this.ReleaseMouseCapture();
            }
        }

        private void source_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Save starting point, used later when determining how much to scroll.
            m_screenStartPoint = e.GetPosition(this);
            m_startOffset = new Point(m_translateTransform.X, m_translateTransform.Y);
            this.CaptureMouse();
            this.Cursor = Cursors.ScrollAll;
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                // if the mouse is captured then move the content by changing the translate transform.  
                // use the Pan Animation to animate to the new location based on the delta between the 
                // starting point of the mouse and the current point.
                Point physicalPoint = e.GetPosition(this);
                m_translateTransform.BeginAnimation(TranslateTransform.XProperty, CreatePanAnimation(physicalPoint.X - m_screenStartPoint.X + m_startOffset.X), HandoffBehavior.Compose);
                m_translateTransform.BeginAnimation(TranslateTransform.YProperty, CreatePanAnimation(physicalPoint.Y - m_screenStartPoint.Y + m_startOffset.Y), HandoffBehavior.Compose);
            }
        }

        /// <summary>Helper to create the panning animation for x,y coordinates.</summary>
        /// <param name="toValue">New value of the coordinate.</param>
        /// <returns>Double animation</returns>
        private DoubleAnimation CreatePanAnimation(double toValue)
        {
            DoubleAnimation da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(300)));

            da.AccelerationRatio = 0.1;
            da.DecelerationRatio = 0.9;
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Freeze();

            return da;
        }

        /// <summary>Helper to create the zoom double animation for scaling.</summary>
        /// <param name="toValue">Value to animate to.</param>
        /// <returns>Double animation.</returns>
        private DoubleAnimation CreateZoomAnimation(double toValue)
        {
            DoubleAnimation da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(500)));

            da.AccelerationRatio = 0.1;
            da.DecelerationRatio = 0.9;
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Freeze();

            return da;
        }

        /// <summary>Zoom into or out of the content.</summary>
        /// <param name="deltaZoom">Factor to mutliply the zoom level by. </param>
        /// <param name="mousePosition">Logical mouse position relative to the original content.</param>
        /// <param name="physicalPosition">Actual mouse position on the screen (relative to the parent window)</param>
        public void DoZoom(double deltaZoom, Point mousePosition, Point physicalPosition)
        {
            double currentZoom = m_zoomTransform.ScaleX;
            currentZoom *= deltaZoom;
            m_translateTransform.BeginAnimation(TranslateTransform.XProperty, CreateZoomAnimation(-1 * (mousePosition.X * currentZoom - physicalPosition.X)));
            m_translateTransform.BeginAnimation(TranslateTransform.YProperty, CreateZoomAnimation(-1 * (mousePosition.Y * currentZoom - physicalPosition.Y)));
            m_zoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(currentZoom));
            m_zoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(currentZoom));
        }

        /// <summary>Reset to default zoom level and centered content.</summary>
        public void Reset()
        {
            m_translateTransform.BeginAnimation(TranslateTransform.XProperty, CreateZoomAnimation(0));
            m_translateTransform.BeginAnimation(TranslateTransform.YProperty, CreateZoomAnimation(0));
            m_zoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, CreateZoomAnimation(1));
            m_zoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, CreateZoomAnimation(1));
        }

        #endregion
    }
}
