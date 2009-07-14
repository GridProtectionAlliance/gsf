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

namespace TVA.Historian.Notifiers
{
    /// <summary>
    /// A base class for a notifier that can process notification messages.
    /// </summary>
    /// <see cref="NotificationType"/>
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
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the notifier.
        /// </summary>
        /// <param name="notifiesInformation">true if <see cref="NotificationType.Information"/> notification will be processed; otherwise false.</param>
        /// <param name="notifiesWarnings">true if <see cref="NotificationType.Warning"/> notification will be processed; otherwise false.</param>
        /// <param name="notifiesAlarms">true if <see cref="NotificationType.Alarm"/> notification will be processed; otherwise false.</param>
        /// <param name="notifiesHeartbeat">true if <see cref="NotificationType.Heartbeat"/> notification will be processed; otherwise false.</param>
        public NotifierBase(bool notifiesInformation, bool notifiesWarnings, bool notifiesAlarms, bool notifiesHeartbeat)
        {
            m_notifiesInformation = notifiesInformation;
            m_notifiesWarnings = notifiesWarnings;
            m_notifiesAlarms = notifiesAlarms;
            m_notifiesHeartbeat = notifiesHeartbeat;
            m_enabled = true;
            m_persistSettings = true;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the notifier is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~NotifierBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Alarm"/> notifications will be processed.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Warning"/> notifications will be processed.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Information"/> notifications will be processed.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Heartbeat"/> notifications will be processed.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the notifier settings are to be saved to the config file.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the category under which the notifier settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the notifier is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationType.Alarm"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected abstract bool NotifyAlarm(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationType.Warning"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected abstract bool NotifyWarning(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationType.Information"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected abstract bool NotifyInformation(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationType.Heartbeat"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        protected abstract bool NotifyHeartbeat(string subject, string message, string details);

        #endregion
               
        /// <summary>
        /// Releases all the resources used by the notifier.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the notifier.
        /// </summary>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves notifier settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["NotifiesAlarms", true];
                element.Update(m_notifiesAlarms, element.Description, element.Encrypted);
                element = settings["NotifiesWarnings", true];
                element.Update(m_notifiesWarnings, element.Description, element.Encrypted);
                element = settings["NotifiesInformation", true];
                element.Update(m_notifiesInformation, element.Description, element.Encrypted);
                element = settings["NotifiesHeartbeat", true];
                element.Update(m_notifiesHeartbeat, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved notifier settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("NotifiesAlarms", m_notifiesAlarms, "True if alarm notifications are to be processed; otherwise False.");
                settings.Add("NotifiesWarnings", m_notifiesWarnings, "True if warning notifications are to be processed; otherwise False.");
                settings.Add("NotifiesInformation", m_notifiesInformation, "True if information notifications are to be processed; otherwise False.");
                settings.Add("NotifiesHeartbeat", m_notifiesHeartbeat, "True if heartbeat notifications are to be processed; otherwise False.");
                NotifiesAlarms = settings["NotifiesAlarms"].ValueAs(m_notifiesAlarms);
                NotifiesWarnings = settings["NotifiesWarnings"].ValueAs(m_notifiesWarnings);
                NotifiesInformation = settings["NotifiesInformation"].ValueAs(m_notifiesInformation);
                NotifiesHeartbeat = settings["NotifiesHeartbeat"].ValueAs(m_notifiesHeartbeat);
            }
        }

        /// <summary>
        /// Process a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationType"/> values.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        public bool Notify(string subject, string message, string details, NotificationType notificationType)
        {
            if (!m_enabled)
                return false;

            if (notificationType == Notifiers.NotificationType.Alarm && m_notifiesAlarms)
                return NotifyAlarm(subject, message, details);
            else if (notificationType == Notifiers.NotificationType.Warning && m_notifiesWarnings)
                return NotifyWarning(subject, message, details);
            else if (notificationType == Notifiers.NotificationType.Information && m_notifiesInformation)
                return NotifyInformation(subject, message, details);
            else if (notificationType == Notifiers.NotificationType.Heartbeat && m_notifiesHeartbeat)
                return NotifyHeartbeat(subject, message, details);
            else
                return false;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the notifier and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
    }
}