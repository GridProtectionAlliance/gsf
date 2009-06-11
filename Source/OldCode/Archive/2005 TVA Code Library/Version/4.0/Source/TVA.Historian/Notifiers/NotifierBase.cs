//*******************************************************************************************************
//  NotifierBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using TVA.Configuration;

namespace DatAWare.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        #region [ Members ]

        // Fields
        private bool m_notifiesAlarms;
        private bool m_notifiesWarnings;
        private bool m_notifiesInformation;
        private bool m_notifiesHeartbeat;
        private bool m_persistSettings;
        private string m_settingsCategory;

        #endregion

        #region [ Constructors ]

        public NotifierBase(bool notifiesInformation, bool notifiesWarnings, bool notifiesAlarms, bool notifiesHeartbeat)
        {
            m_notifiesInformation = notifiesInformation;
            m_notifiesWarnings = notifiesWarnings;
            m_notifiesAlarms = notifiesAlarms;
            m_notifiesHeartbeat = notifiesHeartbeat;
            m_persistSettings = true;
            m_settingsCategory = this.GetType().Name;
        }

        #endregion

        #region [ Properties ]

        public bool NotifiesAlarms
        {
            get
            {
                return m_notifiesAlarms;
            }
            set
            {
                m_notifiesAlarms = value;
            }
        }

        public bool NotifiesWarnings
        {
            get
            {
                return m_notifiesWarnings;
            }
            set
            {
                m_notifiesWarnings = value;
            }
        }

        public bool NotifiesInformation
        {
            get
            {
                return m_notifiesInformation;
            }
            set
            {
                m_notifiesInformation = value;
            }
        }

        public bool NotifiesHeartbeat
        {
            get
            {
                return m_notifiesHeartbeat;
            }
            set
            {
                m_notifiesHeartbeat = value;
            }
        }

        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_settingsCategory = value;
                }
                else
                {
                    throw (new ArgumentNullException("SettingsCategoryName"));
                }
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        protected abstract bool NotifyAlarm(string subject, string message, string details);

        protected abstract bool NotifyWarning(string subject, string message, string details);

        protected abstract bool NotifyInformation(string subject, string message, string details);

        protected abstract bool NotifyHeartbeat(string subject, string message, string details);

        #endregion

        public bool Notify(string subject, string message, string details, NotificationType notificationType)
        {
            if (m_notifiesAlarms && notificationType == Notifiers.NotificationType.Alarm)
            {
                return NotifyAlarm(subject, message, details);
            }
            else if (m_notifiesWarnings && notificationType == Notifiers.NotificationType.Warning)
            {
                return NotifyWarning(subject, message, details);
            }
            else if (m_notifiesInformation && notificationType == Notifiers.NotificationType.Information)
            {
                return NotifyInformation(subject, message, details);
            }
            else if (m_notifiesHeartbeat && notificationType == Notifiers.NotificationType.Heartbeat)
            {
                return NotifyHeartbeat(subject, message, details);
            }
            else
            {
                return false;
            }
        }

        public virtual void LoadSettings()
        {
            try
            {
                TVA.Configuration.CategorizedSettingsElementCollection with_1 = ConfigurationFile.Current.Settings[m_settingsCategory];
                if (with_1.Count > 0)
                {
                    NotifiesAlarms = with_1["NotifiesAlarms"].ValueAs(m_notifiesAlarms);
                    NotifiesWarnings = with_1["NotifiesWarnings"].ValueAs(m_notifiesWarnings);
                    NotifiesInformation = with_1["NotifiesInformation"].ValueAs(m_notifiesInformation);
                    NotifiesHeartbeat = with_1["NotifiesHeartbeat"].ValueAs(m_notifiesHeartbeat);
                }
            }
            catch (Exception)
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    TVA.Configuration.CategorizedSettingsElementCollection with_1 = ConfigurationFile.Current.Settings[m_settingsCategory];
                    with_1.Clear();
                    TVA.Configuration.CategorizedSettingsElement with_2 = with_1["NotifiesAlarms", true];
                    with_2.Value = m_notifiesAlarms.ToString();
                    with_2.Description = "True if alarm notifications are to be sent; otherwise False.";
                    TVA.Configuration.CategorizedSettingsElement with_3 = with_1["NotifiesWarnings", true];
                    with_3.Value = m_notifiesWarnings.ToString();
                    with_3.Description = "True if warning notifications are to be sent; otherwise False.";
                    TVA.Configuration.CategorizedSettingsElement with_4 = with_1["NotifiesInformation", true];
                    with_4.Value = m_notifiesInformation.ToString();
                    with_4.Description = "True if information notifications are to be sent; otherwise False.";
                    TVA.Configuration.CategorizedSettingsElement with_5 = with_1["NotifiesHeartbeat", true];
                    with_5.Value = m_notifiesHeartbeat.ToString();
                    with_5.Description = "True if heartbeat notifications are to be sent; otherwise False.";
                    ConfigurationFile.Current.Save();
                }
                catch (Exception)
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        #endregion
    }
}