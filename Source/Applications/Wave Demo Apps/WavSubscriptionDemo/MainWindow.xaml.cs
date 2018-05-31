//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
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

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GSF;
using GSF.Reflection;
using GSF.Windows.Forms;

namespace WavSubscriptionDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ControlPanelViewModel m_viewModel;

        /// <summary>
        /// Creates the main window for the Wave Subscription Demo.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            m_viewModel = new ControlPanelViewModel(this.WaveForm);
            this.ControlPanel.DataContext = m_viewModel;
            this.Stat.DataContext = new StatViewModel(m_viewModel.AudioGraph);
            m_viewModel.AudioGraph.PlaybackStateChanged += AudioGraph_PlaybackStateChanged;
        }

        // Gets the header name of the selected visualization.
        private string GetSelectedVisualizationHeaderName()
        {
            MenuItem selectedItem = VisualizationMenu.Items.Cast<MenuItem>()
                .Where(item => item.Icon is RadioButton)
                .SingleOrDefault(item => ((RadioButton)item.Icon).IsChecked == true);

            if (selectedItem != null)
                return selectedItem.Header.ToString();

            return null;
        }

        // Sets the selected visualization to the option matching the given header name.
        private void SetSelectedVisualization(string headerName)
        {
            MenuItem selectedItem;

            if (headerName == null)
            {
                selectedItem = VisualizationMenu.Items.Cast<MenuItem>().First();
            }
            else
            {
                selectedItem = VisualizationMenu.Items.Cast<MenuItem>()
                    .Where(item => item.Header != null)
                    .SingleOrDefault(item => item.Header.ToString() == headerName);
            }

            if (selectedItem != null)
            {
                RadioButton itemIcon = selectedItem.Icon as RadioButton;

                if (itemIcon != null)
                    itemIcon.IsChecked = true;
            }
        }

        // Handles the main window's Loaded event. Restores application settings from last run.
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string connectionUri = ConfigurationManager.AppSettings["ConnectionUri"];
            string enableCompression = ConfigurationManager.AppSettings["EnableCompression"];
            string enableEncryption = ConfigurationManager.AppSettings["EnableEncryption"];
            string ipv6Enabled = ConfigurationManager.AppSettings["IPv6Enabled"];
            string visualization = ConfigurationManager.AppSettings["Visualization"];
            string useZeroMQChannel = ConfigurationManager.AppSettings["UseZeroMQChannel"];

            if (connectionUri != null)
                m_viewModel.ConnectionUri = connectionUri;

            if (enableCompression != null)
                m_viewModel.EnableCompression = enableCompression.ParseBoolean();

            if (enableEncryption != null)
                m_viewModel.EnableEncryption = enableEncryption.ParseBoolean();

            if (ipv6Enabled != null)
                m_viewModel.IPv6Enabled = ipv6Enabled.ParseBoolean();

            if (useZeroMQChannel != null)
                m_viewModel.UseZeroMQChannel = useZeroMQChannel.ParseBoolean();

            SetSelectedVisualization(visualization);
        }

        // Handles the main window's Closing event. Stores application settings from this run.
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings.Remove("ConnectionUri");
            config.AppSettings.Settings.Remove("EnableCompression");
            config.AppSettings.Settings.Remove("EnableEncryption");
            config.AppSettings.Settings.Remove("IPv6Enabled");
            config.AppSettings.Settings.Remove("Visualization");
            config.AppSettings.Settings.Remove("UseZeroMQChannel");

            config.AppSettings.Settings.Add("ConnectionUri", m_viewModel.ConnectionUri);
            config.AppSettings.Settings.Add("EnableCompression", m_viewModel.EnableCompression.ToString());
            config.AppSettings.Settings.Add("EnableEncryption", m_viewModel.EnableEncryption.ToString());
            config.AppSettings.Settings.Add("IPv6Enabled", m_viewModel.IPv6Enabled.ToString());
            config.AppSettings.Settings.Add("Visualization", GetSelectedVisualizationHeaderName());
            config.AppSettings.Settings.Add("UseZeroMQChannel", m_viewModel.UseZeroMQChannel.ToString());
            config.Save(ConfigurationSaveMode.Modified);

            m_viewModel.Dispose();
        }

        // Handles the audio graph's PlaybackStateChanged event. Updates the
        // user interface to reflect what's going on in the playback.
        private void AudioGraph_PlaybackStateChanged(object sender, EventArgs<PlaybackState, string> e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new Action<object, EventArgs<PlaybackState, string>>(AudioGraph_PlaybackStateChanged), sender, e);
            }
            else
            {
                string defaultMessage = null;

                // Restore the various UI elements in case they
                // changed the last time the state was updated.
                Cursor = Cursors.Arrow;
                this.ControlPanel.IsEnabled = true;
                this.PlaybackStateLabel.Foreground = Brushes.White;
                this.PlaybackStateLabel.Content = "";

                switch (e.Argument1)
                {
                    case PlaybackState.Connected:
                    case PlaybackState.Playing:
                        this.ControlPanel.DisabledButtons.Visibility = Visibility.Collapsed;
                        break;
                    case PlaybackState.Connecting:
                    case PlaybackState.Buffering:
                        Cursor = Cursors.Wait;
                        this.ControlPanel.IsEnabled = false;
                        this.ControlPanel.DisabledButtons.Visibility = Visibility.Visible;
                        defaultMessage = e.Argument1 + "...";
                        break;
                    case PlaybackState.Disconnected:
                        this.ControlPanel.DisabledButtons.Visibility = Visibility.Visible;
                        break;
                    case PlaybackState.TimedOut:
                        this.PlaybackStateLabel.Foreground = Brushes.Red;
                        defaultMessage = "Connection timed out";
                        break;
                    case PlaybackState.Exception:
                        this.PlaybackStateLabel.Foreground = Brushes.Red;
                        defaultMessage = "Exception encountered";
                        break;
                }

                if (e.Argument2 != null)
                    this.PlaybackStateLabel.Content = e.Argument2;
                else if (defaultMessage != null)
                    this.PlaybackStateLabel.Content = defaultMessage;
            }
        }

        // Handles the "Tools > Options" menu item's Click event.
        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            optionsWindow.DataContext = m_viewModel;
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();
        }

        // Handles the "Help > Online Help" menu item's Click event.
        private void OnlineHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/GridProtectionAlliance/gsf/");
        }

        // Handles the "Help > About" menu item's Click event.
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string localNamespace = this.GetType().Namespace;
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.SetCompanyUrl("http://www.gridprotectionalliance.org/");
            aboutDialog.SetCompanyLogo(AssemblyInfo.EntryAssembly.GetEmbeddedResource(localNamespace + ".Resources.HelpAboutLogo.png"));
            aboutDialog.SetCompanyDisclaimer(AssemblyInfo.EntryAssembly.GetEmbeddedResource(localNamespace + ".Resources.Disclaimer.txt"));
            aboutDialog.ShowDialog();
        }

        // Handles the "Tools > Visualization > ..." menu items' Click event.
        private void VisualizationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem source = e.Source as MenuItem;
            RadioButton sourceIcon = (source == null) ? null : source.Icon as RadioButton;

            if (sourceIcon != null)
                sourceIcon.IsChecked = true;
        }
    }
}
