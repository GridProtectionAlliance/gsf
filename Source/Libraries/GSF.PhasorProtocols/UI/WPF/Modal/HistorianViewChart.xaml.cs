//******************************************************************************************************
//  HistorianViewChart.xaml.cs - Gbtc
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
//  01/05/2012 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using GSF.PhasorProtocols.UI.UserControls;

namespace GSF.PhasorProtocols.UI.Modal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class HistorianViewChart : Window
    {
        #region[Members]

        private readonly ChartWindowUserControl m_chartWindowUserControl;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="HistorianViewChart"/>.
        /// </summary>
        public HistorianViewChart()
        {
            InitializeComponent();
            m_chartWindowUserControl = new ChartWindowUserControl();
            this.Loaded += HistorianViewChart_Loaded;
            //// added
            //HistorianViewChart m_historianViewChart;
            //m_historianViewChart = new HistorianViewChart();
           
        }
        // Assing a Value 
        /// <param name="chartWindowUserControl"></param>
        public HistorianViewChart(ChartWindowUserControl chartWindowUserControl) : this()
        {
            m_chartWindowUserControl = chartWindowUserControl;
        }

        void HistorianViewChart_Loaded(object sender, RoutedEventArgs e)
        {
            CanvasMain.Children.Add(m_chartWindowUserControl);
        }

        #endregion

    }
}
