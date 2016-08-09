//******************************************************************************************************
//  ChartExtensions.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/09/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace GSF.Windows.Forms
{
    /// <summary>
    /// Defines extension methods for <see cref="Chart"/>s.
    /// </summary>
    public static class ChartExtensions
    {
        /// <summary>
        /// Sets the size of the chart while also setting the font size of the
        /// axes and legend as well as the border width of each of the series.
        /// </summary>
        /// <param name="chart">The chart to be resized.</param>
        /// <param name="width">The new width of the chart.</param>
        /// <param name="height">The new height of the chart.</param>
        /// <param name="fontRatio">The ratio of chart height to font size.</param>
        /// <param name="borderRatio">The ratio of chart height to border width.</param>
        public static void SetChartSize(this Chart chart, int width, int height, double fontRatio = 37.0D, double borderRatio = 480.0D)
        {
            int fontSize = (int)Math.Round(height / fontRatio);
            int borderWidth = (int)Math.Round(height / borderRatio);

            chart.Width = width;
            chart.Height = height;

            chart.ChartAreas[0].AxisX.LabelAutoFitMaxFontSize = fontSize;
            chart.ChartAreas[0].AxisY.LabelAutoFitMaxFontSize = fontSize;
            chart.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = fontSize;
            chart.ChartAreas[0].AxisY.LabelAutoFitMinFontSize = fontSize;
            chart.ChartAreas[0].AxisX.TitleFont = new Font(chart.ChartAreas[0].AxisX.TitleFont.FontFamily, fontSize);
            chart.ChartAreas[0].AxisY.TitleFont = new Font(chart.ChartAreas[0].AxisY.TitleFont.FontFamily, fontSize);
            chart.Legends[0].Font = new Font(chart.Legends[0].Font.FontFamily, fontSize, FontStyle.Regular);

            foreach (Series series in chart.Series)
                series.BorderWidth = borderWidth;
        }

        /// <summary>
        /// Saves the chart to a memory stream using the given image
        /// <paramref name="format"/> and returns the stream, ready for reading.
        /// </summary>
        /// <param name="chart">The chart to be saved to the stream.</param>
        /// <param name="format">The format to used to save the image.</param>
        /// <returns>The stream containing the chart as an image.</returns>
        public static Stream ConvertToImageStream(this Chart chart, ChartImageFormat format = ChartImageFormat.Png)
        {
            MemoryStream stream = new MemoryStream();
            chart.SaveImage(stream, format);
            stream.Position = 0;
            return stream;
        }
    }
}
