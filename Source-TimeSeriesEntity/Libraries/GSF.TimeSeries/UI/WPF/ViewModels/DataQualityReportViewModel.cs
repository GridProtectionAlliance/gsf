//******************************************************************************************************
//  DataQualityReportViewModel.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  03/06/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.Console;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI.Commands;
using Microsoft.Win32;

namespace GSF.TimeSeries.UI.ViewModels
{
    public class DataQualityReportViewModel : ViewModelBase, IDisposable
    {
        #region [ Members ]

        // Fields
        private Visibility m_connectivityMessageVisibility;
        private bool m_serviceConnected;
        private bool m_reportingEnabled;

        private string m_reportingConfiguration;
        private string m_originalReportLocation;
        private string m_reportLocation;
        private string m_reportGenerationTimeText;
        private DateTime m_originalReportGenerationTime;
        private DateTime m_reportGenerationTime;
        private DateTime m_reportDate;

        private byte[] m_reportData;

        private string m_responseMessage;
        private AutoResetEvent m_responseComplete;
        private ICommand m_changeReportingEnabledCommand;
        private ICommand m_updateReportingConfigCommand;
        private ICommand m_generateReportCommand;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DataQualityReportViewModel"/> class.
        /// </summary>
        public DataQualityReportViewModel()
        {
            ReportGenerationTime = new DateTime(1, 1, 1, 2, 0, 0);
            ReportDate = DateTime.Now - TimeSpan.FromDays(1);
            m_responseComplete = new AutoResetEvent(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a flag to indicate whether the service is connected.
        /// </summary>
        public bool ServiceConnected
        {
            get
            {
                return m_serviceConnected;
            }
            set
            {
                m_serviceConnected = value;
                m_connectivityMessageVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("ServiceConnected");
                OnPropertyChanged("ConnectivityMessageVisibility");

                if (m_serviceConnected)
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(GetSchedules));
            }
        }

        /// <summary>
        /// Gets a value that determines whether the connectivity message should be displayed.
        /// </summary>
        public Visibility ConnectivityMessageVisibility
        {
            get
            {
                return m_connectivityMessageVisibility;
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether reporting is currently enabled.
        /// </summary>
        public bool ReportingEnabled
        {
            get
            {
                return m_reportingEnabled;
            }
            set
            {
                m_reportingEnabled = value;
                OnPropertyChanged("ReportingEnabled");
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(GetReportingConfiguration));
            }
        }

        /// <summary>
        /// Gets or sets the location to which reports will be generated.
        /// </summary>
        public string ReportLocation
        {
            get
            {
                return m_reportLocation;
            }
            set
            {
                m_reportLocation = value;
                OnPropertyChanged("ReportLocation");
            }
        }
        
        /// <summary>
        /// Gets or sets the text used to display or modify the value of the <see cref="ReportGenerationTime"/> property.
        /// </summary>
        public string ReportGenerationTimeText
        {
            get
            {
                return m_reportGenerationTimeText;
            }
            set
            {
                DateTime reportGenerationTime;

                if (DateTime.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out reportGenerationTime))
                {
                    ReportGenerationTime = reportGenerationTime;
                }
                else
                {
                    m_reportGenerationTimeText = value;
                    OnPropertyChanged("ReportGenerationTimeText");
                }
            }
        }

        /// <summary>
        /// Gets or sets the report generation time.
        /// </summary>
        public DateTime ReportGenerationTime
        {
            get
            {
                return m_reportGenerationTime;
            }
            set
            {
                m_reportGenerationTime = value;
                m_reportGenerationTimeText = ActualReportGenerationTimeText;
                OnPropertyChanged("ReportGenerationTime");
                OnPropertyChanged("ReportGenerationTimeText");
                OnPropertyChanged("ActualReportGenerationTimeText");
            }
        }

        /// <summary>
        /// Gets or sets the text for the actual report generation time, which may not match <see cref="ReportGenerationTimeText"/>.
        /// </summary>
        public string ActualReportGenerationTimeText
        {
            get
            {
                return m_reportGenerationTime.ToString("HH:mm");
            }
        }

        /// <summary>
        /// Gets or sets the date of the report to be generated manually.
        /// </summary>
        public DateTime ReportDate
        {
            get
            {
                return m_reportDate;
            }
            set
            {
                m_reportDate = value;
                OnPropertyChanged("ReportDate");
            }
        }

        /// <summary>
        /// Gets the command to change the enabled state of reporting services.
        /// </summary>
        public ICommand ChangeReportingEnabledCommand
        {
            get
            {
                return m_changeReportingEnabledCommand ?? (m_changeReportingEnabledCommand = new RelayCommand(ChangeReportingEnabled, () => true));
            }
        }

        /// <summary>
        /// Gets the command to update the configuration of the reporting services.
        /// </summary>
        public ICommand UpdateReportingConfigCommand
        {
            get
            {
                return m_updateReportingConfigCommand ?? (m_updateReportingConfigCommand = new RelayCommand(UpdateReportingConfig, () => true));
            }
        }

        /// <summary>
        /// Gets the command to generate a new report manually.
        /// </summary>
        public ICommand GenerateReportCommand
        {
            get
            {
                return m_generateReportCommand ?? (m_generateReportCommand = new RelayCommand(GenerateReport, () => true));
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataQualityReportViewModel"/> object.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    if ((object)m_responseComplete != null)
                    {
                        // Release any waiting threads before disposing wait handle
                        m_responseComplete.Set();
                        m_responseComplete.Dispose();
                    }

                    m_responseComplete = null;
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }

            GC.SuppressFinalize(this);
        }

        private void GetSchedules()
        {
            WindowsServiceClient serviceClient;

            Match match;
            int hour, minute;

            try
            {
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
                serviceClient.Helper.SendRequest("Schedules");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.WaitOne(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                if ((object)m_responseMessage != null)
                {
                    match = Regex.Match(m_responseMessage, "^Reporting +(?<Minute>[^ ]+) +(?<Hour>[^ ]+)", RegexOptions.Multiline);

                    if (match.Success)
                    {
                        ReportingEnabled = true;

                        if (int.TryParse(match.Groups["Hour"].Value, out hour) && int.TryParse(match.Groups["Minute"].Value, out minute))
                        {
                            ReportGenerationTime = DateTime.ParseExact(string.Format("{0:00}:{1:00}", hour, minute), "HH:mm", CultureInfo.InvariantCulture);
                            m_originalReportGenerationTime = m_reportGenerationTime;
                        }
                    }
                    else
                    {
                        ReportingEnabled = false;
                    }

                    m_responseMessage = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to determine whether reporting services are enabled: {0}", ex.Message);
                Popup(message, "GetSchedules Error", MessageBoxImage.Error);
            }
        }

        private void ChangeReportingEnabled()
        {
            WindowsServiceClient serviceClient;

            Match match;
            int hour, minute;

            try
            {
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;

                if (ReportingEnabled)
                    serviceClient.Helper.SendRequest("Unschedule Reporting -save");
                else
                    serviceClient.Helper.SendRequest(string.Format("Reschedule Reporting \"{0} {1} * * *\" -save", m_reportGenerationTime.Minute, m_reportGenerationTime.Hour));

                serviceClient.Helper.SendRequest("Schedules");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.WaitOne(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                if ((object)m_responseMessage != null)
                {
                    match = Regex.Match(m_responseMessage, "^Reporting +(?<Minute>[^ ]+) +(?<Hour>[^ ]+)", RegexOptions.Multiline);

                    if (match.Success)
                    {
                        ReportingEnabled = true;

                        if (int.TryParse(match.Groups["Hour"].Value, out hour) && int.TryParse(match.Groups["Minute"].Value, out minute))
                        {
                            ReportGenerationTime = DateTime.ParseExact(string.Format("{0:00}:{1:00}", hour, minute), "HH:mm", CultureInfo.InvariantCulture);
                            m_originalReportGenerationTime = m_reportGenerationTime;
                        }
                    }
                    else
                    {
                        ReportingEnabled = false;
                    }

                    m_responseMessage = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to {0} reporting services: {1}", m_reportingEnabled ? "disable" : "enable", ex.Message);
                Popup(message, "ChangeReportingEnabled Error", MessageBoxImage.Error);
            }
        }

        private void GetReportingConfiguration()
        {
            WindowsServiceClient serviceClient;

            Arguments args;
            string reportLocation;

            try
            {
                serviceClient = CommonFunctions.GetWindowsServiceClient();
                serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                serviceClient.Helper.SendRequest("ReportingConfig");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.WaitOne(5000))
                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                }

                if (!string.IsNullOrEmpty(m_reportingConfiguration))
                {
                    args = new Arguments(m_reportingConfiguration);
                    reportLocation = args["reportLocation"];

                    if (!string.IsNullOrEmpty(reportLocation))
                    {
                        ReportLocation = reportLocation.Trim();
                        m_originalReportLocation = m_reportLocation;
                    }

                    m_reportingConfiguration = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to retrieve reporting services configuration: {0}", ex.Message);
                Popup(message, "GetReportingConfiguration Error", MessageBoxImage.Error);
            }
        }

        private void UpdateReportingConfig()
        {
            WindowsServiceClient serviceClient;

            Match match;
            int hour, minute;

            try
            {
                serviceClient = CommonFunctions.GetWindowsServiceClient();

                if (m_reportGenerationTime != m_originalReportGenerationTime)
                {
                    serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
                    serviceClient.Helper.SendRequest(string.Format("Reschedule Reporting \"{0} {1} * * *\" -save", m_reportGenerationTime.Minute, m_reportGenerationTime.Hour));
                    serviceClient.Helper.SendRequest("Schedules");

                    // Wait for command response allowing for processing time
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.WaitOne(5000))
                            throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                    }

                    if ((object)m_responseMessage != null)
                    {
                        match = Regex.Match(m_responseMessage, "^Reporting +(?<Minute>[^ ]+) +(?<Hour>[^ ]+)", RegexOptions.Multiline);

                        if (match.Success)
                        {
                            ReportingEnabled = true;

                            if (int.TryParse(match.Groups["Hour"].Value, out hour) && int.TryParse(match.Groups["Minute"].Value, out minute))
                            {
                                ReportGenerationTime = DateTime.ParseExact(string.Format("{0:00}:{1:00}", hour, minute), "HH:mm", CultureInfo.InvariantCulture);
                                m_originalReportGenerationTime = m_reportGenerationTime;
                            }
                        }
                        else
                        {
                            ReportingEnabled = false;
                        }

                        m_responseMessage = null;
                    }
                }

                if (!string.Equals(m_reportLocation, m_originalReportLocation, StringComparison.OrdinalIgnoreCase))
                {
                    serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    serviceClient.Helper.SendRequest(string.Format("ReportingConfig -set --reportLocation=\" {0} \"", m_reportLocation.Replace("\"", "\\\"")));

                    // Wait for command response allowing for processing time
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.WaitOne(5000))
                            throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                    }

                    if ((object)m_reportingConfiguration != null)
                    {
                        m_originalReportLocation = m_reportLocation;
                        m_reportingConfiguration = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to modify reporting services configuration: {0}", ex.Message);
                Popup(message, "UpdateReportingConfig Error", MessageBoxImage.Error);
            }
        }

        private void GenerateReport()
        {
            WindowsServiceClient serviceClient;

            try
            {
                FileDialog fileDialog = new SaveFileDialog();

                fileDialog.DefaultExt = "pdf";
                fileDialog.Filter = "PDF files|*.pdf|All files|*.*";
                fileDialog.FileName = m_reportDate.ToString("yyyy-MM-dd") + ".pdf";

                if (fileDialog.ShowDialog() == true)
                {
                    serviceClient = CommonFunctions.GetWindowsServiceClient();
                    serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    serviceClient.Helper.SendRequest(string.Format("GenerateReport {0:yyyy-MM-dd}", m_reportDate));

                    // Wait for command response allowing for processing time
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.WaitOne(15000))
                            throw new TimeoutException("Timed-out after 15 seconds waiting for service response.");
                    }

                    if ((object)m_reportData != null)
                    {
                        File.WriteAllBytes(fileDialog.FileName, m_reportData);
                        m_reportData = null;

                        try
                        {
                            using (Process.Start(fileDialog.FileName)) { }
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("Report saved, but unable to open: {0}", ex.Message);
                            Popup(message, "GenerateReport", MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to generate report: {0}", ex.Message);
                Popup(message, "GenerateReport", MessageBoxImage.Error);
            }
        }

        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            ClientHelper helper;

            if (Regex.IsMatch(e.Argument2, "^Process schedules"))
            {
                helper = sender as ClientHelper;

                if ((object)helper != null)
                    helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;

                m_responseMessage = e.Argument2;

                if ((object)m_responseComplete != null)
                    m_responseComplete.Set();
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if ((object)e != null)
            {
                ServiceResponse response = e.Argument;

                if ((object)response != null)
                {
                    string sourceCommand;
                    bool responseSuccess;

                    if (ClientHelper.TryParseActionableResponse(response, out sourceCommand, out responseSuccess))
                    {
                        ClientHelper helper = sender as ClientHelper;

                        if ((object)helper != null)
                            helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;

                        switch (sourceCommand.Trim().ToUpper())
                        {
                            case "GENERATEREPORT":
                                if (responseSuccess)
                                {
                                    List<object> attachments = response.Attachments;

                                    if ((object)attachments != null && attachments.Count > 1)
                                        m_reportData = attachments[0] as byte[];
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "GenerateReport", MessageBoxImage.Error)));
                                }

                                break;

                            case "REPORTINGCONFIG":
                                if (responseSuccess)
                                {
                                    m_reportingConfiguration = response.Message;
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Popup(response.Message, "ReportingConfig", MessageBoxImage.Error)));
                                }

                                break;
                        }

                        if ((object)m_responseComplete != null)
                            m_responseComplete.Set();
                    }
                }
            }
        }

        #endregion
    }
}
