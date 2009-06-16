//*******************************************************************************************************
//  NotificationDispatcher.cs
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
//  07-12-2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using TVA.Collections;
using TVA.IO;

namespace TVA.Historian.Notifiers
{
    /// <summary>
    /// A class that can be used to dispath notifications using all available <see cref="INotifier">Notifiers</see>.
    /// </summary>
    /// <seealso cref="INotifier"/>
    public class NotificationDispatcher
    {
        #region [ Members ]

        // Nested Types

        private class Notification
        {
            public Notification(string subject, string message, string details, NotificationType notificationType)
            {
                this.Subject = subject;
                this.Message = message;
                this.Details = details;
                this.NotificationType = notificationType;
            }

            public string Subject;
            public string Message;
            public string Details;
            public NotificationType NotificationType;
        }

        // Events

        /// <summary>
        /// Occurs when a new <see cref="INotifier">Notifier</see> is added.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="INotifier">Notifier</see> that was added.
        /// </remarks>
        public event EventHandler<EventArgs<INotifier>> NotifierAdded;

        /// <summary>
        /// Occurs when a notification is sent successfully using a <see cref="INotifier">Notifier</see>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="INotifier">Notifier</see> used for sending the notification.
        /// </remarks>
        public event EventHandler<EventArgs<INotifier>> NotificationSendSuccess;

        /// <summary>
        /// Occurs when a notification cannot be sent using a <see cref="INotifier">Notifier</see>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="INotifier">Notifier</see> used for sending the notification.
        /// </remarks>
        public event EventHandler<EventArgs<INotifier>> NotificationSendFailure;

        /// <summary>
        /// Occurs when sending a notification using a <see cref="INotifier">Notifier</see> times out.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="INotifier">Notifier</see> used for sending the notification.
        /// </remarks>
        public event EventHandler<EventArgs<INotifier>> NotificationSendTimeout;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending a notification.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when sending a notification.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> NotificationSendException;

        // Fields
        private int m_dispatchTimeout;
        private List<INotifier> m_notifiers;
        private ProcessQueue<Notification> m_dispatchQueue;
        private FileSystemWatcher m_fileSystemWatcher;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDispatcher"/> class.
        /// </summary>
        public NotificationDispatcher()
        {
            m_dispatchTimeout = 30000;
            m_notifiers = new List<INotifier>();
            m_dispatchQueue = ProcessQueue<Notification>.CreateSynchronousQueue(ProcessNotification);

            m_fileSystemWatcher = new FileSystemWatcher();
            m_fileSystemWatcher.Filter = "*.dll";
            m_fileSystemWatcher.Created += FileSystemWatcher_Created;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of milliseconds to wait for a notification to be sent.
        /// </summary>
        /// <remarks>Set <see cref="DispatchTimeout"/> to -1 to wait indefinitely.</remarks>
        /// <exception cref="ArgumentException">The value being assigned is not positive or -1.</exception>
        public int DispatchTimeout
        {
            get
            {
                return m_dispatchTimeout;
            }
            set
            {
                if (value < 1 && value != -1)
                    throw new ArgumentException("Value must be positive or -1.");

                m_dispatchTimeout = value;
            }
        }

        /// <summary>
        /// Gets all available <see cref="INotifier">Notifiers</see>.
        /// </summary>
        public IList<INotifier> Notifiers
        {
            get
            {
                return m_notifiers.AsReadOnly();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Starts the <see cref="NotificationDispatcher"/>.
        /// </summary>
        public void Start()
        {
            // Load all available notifiers.
            foreach (Type type in typeof(INotifier).LoadImplementations())
            {
                ProcessNotifier(type);
            }

            // Start notification queue and notifier file watcher.
            m_dispatchQueue.ProcessTimeout = m_dispatchTimeout;
            m_dispatchQueue.Start();
            m_fileSystemWatcher.Path = FilePath.GetAbsolutePath("");
            m_fileSystemWatcher.Filter = "*.dll";
            m_fileSystemWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stops the <see cref="NotificationDispatcher"/>.
        /// </summary>
        public void @Stop()
        {
            m_fileSystemWatcher.EnableRaisingEvents = false;
            m_dispatchQueue.Flush();

            foreach (INotifier notifier in m_notifiers)
            {
                notifier.SaveSettings();
            }
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationType"/> values.</param>
        public void SendNotification(string subject, string message, NotificationType notificationType)
        {
            SendNotification(subject, message, "", notificationType);
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationType"/> values.</param>
        public void SendNotification(string subject, string message, string details, NotificationType notificationType)
        {
            m_dispatchQueue.Add(new Notification(subject, message, details, notificationType));
        }

        /// <summary>
        /// Raises the <see cref="NotifierAdded"/> event.
        /// </summary>
        /// <param name="notifier"><see cref="INotifier"/> to send to <see cref="NotifierAdded"/> event.</param>
        protected virtual void OnNotifierAdded(INotifier notifier)
        {
            if (NotifierAdded != null)
                NotifierAdded(this, new EventArgs<INotifier>(notifier));
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendSuccess"/> event.
        /// </summary>
        /// <param name="notifier"><see cref="INotifier"/> to send to <see cref="NotificationSendSuccess"/> event.</param>
        protected virtual void OnNotificationSendSuccess(INotifier notifier)
        {
            if (NotificationSendSuccess != null)
                NotificationSendSuccess(this, new EventArgs<INotifier>(notifier));
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendFailure"/> event.
        /// </summary>
        /// <param name="notifier"><see cref="INotifier"/> to send to <see cref="NotificationSendFailure"/> event.</param>
        protected virtual void OnNotificationSendFailure(INotifier notifier)
        {
            if (NotificationSendFailure != null)
                NotificationSendFailure(this, new EventArgs<INotifier>(notifier));
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendTimeout"/> event.
        /// </summary>
        /// <param name="notifier"><see cref="INotifier"/> to send to <see cref="NotificationSendTimeout"/> event.</param>
        protected virtual void OnNotificationSendTimeout(INotifier notifier)
        {
            if (NotificationSendTimeout != null)
                NotificationSendTimeout(this, new EventArgs<INotifier>(notifier));
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="NotificationSendException"/> event.</param>
        protected virtual void OnNotificationSendException(Exception ex)
        {
            if (NotificationSendException != null)
                NotificationSendException(this, new EventArgs<Exception>(ex));
        }

        private void ProcessNotifier(Type type)
        {
            INotifier notifier = (INotifier)(Activator.CreateInstance(type));
            notifier.LoadSettings();
            m_notifiers.Add(notifier);

            OnNotifierAdded(notifier);
        }

        private void ProcessNotification(Notification notification)
        {
            // Send notification using all available notifiers.
            foreach (INotifier notifier in m_notifiers)
            {
                try
                {
                    // Send the notification.
                    if (notifier.Notify(notification.Subject, notification.Message, notification.Details, notification.NotificationType))
                        OnNotificationSendSuccess(notifier);
                    else
                        OnNotificationSendFailure(notifier);
                }
                catch (ThreadAbortException)
                {
                    // Sending the notification took too long.
                    OnNotificationSendTimeout(notifier);
                }
                catch (Exception ex)
                {
                    // Exception encountered when sending the notification.
                    OnNotificationSendException(ex);
                }
            }
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            // Process assemblies at runtime for notifiers.
            foreach (Type type in typeof(INotifier).LoadImplementations(e.FullPath))
            {
                ProcessNotifier(type);
            }
        }

        #endregion
    }
}
