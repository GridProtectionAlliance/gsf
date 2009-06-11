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

        // Constants
        private const string NotifierInterface = "DatAWare.Notifiers.INotifier";
        private const int NotificationDispatchTimeout = 30000;

        // Events

        /// <summary>
        /// Occurs when a new notifier is found.
        /// </summary>
        public event EventHandler<EventArgs<INotifier>> NotifierAdded;

        /// <summary>
        /// Occurs when a notification is sent successfully using a notifier.
        /// </summary>
        public event EventHandler<EventArgs<INotifier>> NotificationSendSuccess;

        /// <summary>
        /// Occurs when sending a notification using a notifier times out.
        /// </summary>
        public event EventHandler<EventArgs<INotifier>> NotificationSendTimeout;

        /// <summary>
        /// Occurs when an exception is encountered when sending a notification.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> NotificationSendException;

        // Fields
        private List<INotifier> m_notifiers;
        private ProcessQueue<Notification> m_dispatchQueue;
        private FileSystemWatcher m_fileSystemWatcher;

        #endregion

        #region [ Constructors ]

        public NotificationDispatcher()
        {
            m_notifiers = new List<INotifier>();
            m_dispatchQueue = ProcessQueue<Notification>.CreateSynchronousQueue(ProcessNotification);

            m_fileSystemWatcher = new FileSystemWatcher();
            //m_fileSystemWatcher.EnableRaisingEvents = true; // HACK: Fix this
            m_fileSystemWatcher.Filter = "*.dll";
            m_fileSystemWatcher.Created += FileSystemWatcher_Created;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a list of all available notifiers.
        /// </summary>
        public IList<INotifier> Notifiers
        {
            get
            {
                return m_notifiers;
            }
        }

        #endregion

        #region [ Methods ]

        public void Start()
        {
            foreach (string dll in Directory.GetFiles(FilePath.GetAbsolutePath(""), "*.dll"))
            {
                ProcessDll(dll);
            }

            m_dispatchQueue.Start();
            m_fileSystemWatcher.Path = FilePath.GetAbsolutePath("");
            m_fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void @Stop()
        {
            m_fileSystemWatcher.EnableRaisingEvents = false;
            m_dispatchQueue.Flush();

            foreach (INotifier notifier in m_notifiers)
            {
                notifier.SaveSettings();
            }
        }

        public void SendNotification(string subject, string message, NotificationType notificationType)
        {
            SendNotification(subject, message, "", notificationType);
        }

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

        /// <summary>
        /// Delegate function used by the ProcessQueue for processing notifications.
        /// </summary>
        private void ProcessNotification(Notification notification)
        {
            foreach (INotifier notifier in m_notifiers)
            {
                // We'll send the notification using a loaded notifier on a seperate thread just so that we can
                // timeout on the send if it is taking abnormally long to process.
                Thread processThread = new Thread(ProcessNotificationAsync);
                processThread.Start(new object[] { notification, notifier });
                if (!processThread.Join(NotificationDispatchTimeout))
                {
                    // The notification could not be processed in a timely manner, so we'll abort the send.
                    processThread.Abort();
                    OnNotificationSendTimeout(notifier);
                }
            }
        }

        /// <summary>
        /// Processes a notification using the specified notifier. This method is to executed asynchronously.
        /// </summary>
        private void ProcessNotificationAsync(object state)
        {
            try
            {
                object[] args = (object[])state;
                INotifier notifier = (INotifier)args[1];
                Notification notification = (Notification)args[0];

                if (notifier.Notify(notification.Subject, notification.Message, notification.Details, notification.NotificationType))
                {
                    OnNotificationSendSuccess(notifier);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                OnNotificationSendException(ex);
            }
        }

        private void ProcessDll(string dllName)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(dllName);
                foreach (Type asmType in asm.GetExportedTypes())
                {
                    if (!asmType.IsAbstract && (asmType.GetInterface(NotifierInterface, true) != null))
                    {
                        // The scanned type can be instantiated and implements the required interface.
                        INotifier notifier = (INotifier)(Activator.CreateInstance(asmType));
                        notifier.LoadSettings();
                        m_notifiers.Add(notifier);

                        OnNotifierAdded(notifier);
                    }
                }
            }
            catch (Exception)
            {
                // It is possible that we encounter exception when we come accross DLLs that are not .Net.
            }
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            ProcessDll(e.FullPath);
        }

        #endregion
    }
}
