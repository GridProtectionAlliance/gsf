//******************************************************************************************************
//  PolylineWaveFormControl.xaml.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/07/2011 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Code translated from "NAudioWpfDemo" developed by Mark Heath
//  found in the "NAudio" project: http://naudio.codeplex.com/
//
//  Copyright (c) Mark Heath 2008
//  Microsoft Public License (Ms-PL): http://www.opensource.org/licenses/ms-pl
//
//******************************************************************************************************

#endregion

using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WavSubscriptionDemo
{
    /// <summary>
    /// Interaction logic for PolylineWaveFormControl.xaml
    /// </summary>
    public partial class PolylineWaveFormControl : UserControl, IWaveFormRenderer
    {
        private readonly DispatcherTimer renderTimer;
        private ConcurrentQueue<float> maxValues;
        private ConcurrentQueue<float> minValues;

        private readonly Polyline topLine = new Polyline();
        private readonly Polyline bottomLine = new Polyline();

        private int renderPosition;
        private double yTranslate = 40;
        private double yScale = 40;
        private int blankZone = 10;

        public int SampleRate { get; set; }
        
        public PolylineWaveFormControl()
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();

            renderTimer = CreateRenderTimer();
            maxValues = new ConcurrentQueue<float>();
            minValues = new ConcurrentQueue<float>();

            topLine.Stroke = Foreground;
            bottomLine.Stroke = Foreground;
            topLine.StrokeThickness = 1.5;
            bottomLine.StrokeThickness = 1.5;
            mainCanvas.Children.Add(topLine);
            mainCanvas.Children.Add(bottomLine);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            renderPosition = 0;
            ClearAllPoints();

            yTranslate = ActualHeight / 2;
            yScale = ActualHeight / 2;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ForegroundProperty)
            {
                topLine.Stroke = e.NewValue as Brush;
                bottomLine.Stroke = e.NewValue as Brush;
            }
        }

        private DispatcherTimer CreateRenderTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(100.0);
            timer.Tick += RenderTimer_Tick;

            return timer;
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (Visibility != Visibility.Visible)
            {
                renderTimer.Stop();
            }
            else
            {
                float maxValue, minValue;
                int pixelWidth;

                while (maxValues.Count * 200 > SampleRate)
                {
                    pixelWidth = (int)ActualWidth;

                    if (pixelWidth > 0)
                    {
                        maxValues.TryDequeue(out maxValue);
                        minValues.TryDequeue(out minValue);
                        CreatePoint(maxValue, minValue);

                        if (renderPosition > ActualWidth)
                        {
                            renderPosition = 0;
                        }
                        int erasePosition = (renderPosition + blankZone) % pixelWidth;
                        if (erasePosition < topLine.Points.Count)
                        {
                            double yPos = SampleToYPosition(0);
                            topLine.Points[erasePosition] = new Point(erasePosition, yPos);
                            bottomLine.Points[erasePosition] = new Point(erasePosition, yPos);
                        }
                    }
                }
            }
        }

        private void ClearAllPoints()
        {
            topLine.Points.Clear();
            bottomLine.Points.Clear();
        }

        public void AddValue(float maxValue, float minValue)
        {
            if (Visibility == Visibility.Visible)
            {
                maxValues.Enqueue(maxValue);
                minValues.Enqueue(minValue);

                if (!renderTimer.IsEnabled)
                {
                    renderTimer.Start();
                }
            }
        }

        private double SampleToYPosition(float value)
        {
            return yTranslate + value * yScale;
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            double topLinePos = SampleToYPosition(topValue);
            double bottomLinePos = SampleToYPosition(bottomValue);
            if (renderPosition >= topLine.Points.Count)
            {
                topLine.Points.Add(new Point(renderPosition, topLinePos));
                bottomLine.Points.Add(new Point(renderPosition, bottomLinePos));
            }
            else
            {
                topLine.Points[renderPosition] = new Point(renderPosition, topLinePos);
                bottomLine.Points[renderPosition] = new Point(renderPosition, bottomLinePos);
            }
            renderPosition++;
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
        public void Reset()
        {
            renderTimer.Stop();
            maxValues = new ConcurrentQueue<float>();
            minValues = new ConcurrentQueue<float>();
            renderPosition = 0;
            ClearAllPoints();
        }
    }
}
