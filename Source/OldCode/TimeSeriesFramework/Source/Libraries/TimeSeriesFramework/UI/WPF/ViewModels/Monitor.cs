using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework.UI.Commands;
using System.Windows.Documents;
using TVA;
using TVA.Services.ServiceProcess;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TimeSeriesFramework.UI.ViewModels
{
    internal class Monitor : ViewModelBase
    {

        #region [ Members ]

        private InlineCollection m_statusMessages;
        private WindowsServiceClient m_serviceClient;

        #endregion

        #region [ Properties ]

        public InlineCollection StatusMessages
        {
            get
            {
                return m_statusMessages;
            }
            set
            {
                m_statusMessages = value;
                OnPropertyChanged("StatusMessages");
            }
        }

        #endregion

        #region [ Constructor ]

        public Monitor()
        {
            StatusMessages = (new TextBlock()).Inlines;
            SetupServiceConnection();
        }

        #endregion
        
        #region [ Methods ]

        private void SetupServiceConnection()
        {
            m_serviceClient = null;
            try
            {
                m_serviceClient = CommonFunctions.GetWindowsServiceClient();
                if (m_serviceClient != null && m_serviceClient.Helper != null &&
                   m_serviceClient.Helper.RemotingClient != null && m_serviceClient.Helper.RemotingClient.CurrentState == TVA.Communication.ClientState.Connected)
                {
                    RefreshStatusText(UpdateType.Information, m_serviceClient.CachedStatus);
                    m_serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    m_serviceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Connect to Service", MessageBoxImage.Error);
            }
        }

        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            RefreshStatusText(e.Argument1, e.Argument2);
        }

        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            string response = e.Argument.Type;
            string message = e.Argument.Message;
            string responseToClient = string.Empty;
            UpdateType responseType = UpdateType.Information;                        

            if (!string.IsNullOrEmpty(response))
            {
                // Reponse types are formatted as "Command:Success" or "Command:Failure"
                string[] parts = response.Split(':');
                string action;
                bool success;

                if (parts.Length > 1)
                {
                    action = parts[0].Trim().ToTitleCase();
                    success = (string.Compare(parts[1].Trim(), "Success", true) == 0);
                }
                else
                {
                    action = response;
                    success = true;
                }

                if (success)
                {
                    if (string.IsNullOrEmpty(message))
                        responseToClient = string.Format("{0} command processed successfully.\r\n\r\n", action);
                    else
                        responseToClient = string.Format("{0}\r\n\r\n", message);
                }
                else
                {
                    responseType = UpdateType.Alarm;
                    if (string.IsNullOrEmpty(message))
                        responseToClient = string.Format("{0} failure.\r\n\r\n", action);
                    else
                        responseToClient = string.Format("{0} failure: {1}\r\n\r\n", action, message);
                }

                RefreshStatusText(responseType, responseToClient);
            }
        }

        private void RefreshStatusText(UpdateType updateType, string message)
        {
            Run run;
            if (updateType == UpdateType.Information)
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                run.Text = message;
            }
            else if (updateType == UpdateType.Warning)
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                run.Text = message;
            }
            else
            {
                run = new Run();
                run.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 10, 10));
                run.Text = message;
            }
                        
            StatusMessages.Add(run);
            
            if (StatusMessages.Count > 50)
                StatusMessages.Remove(StatusMessages.FirstInline);
        }

        public void DetachServiceEvents()
        {
            if (m_serviceClient != null)
            {
                m_serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                m_serviceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
            }
        }

        #endregion                
    }
}
