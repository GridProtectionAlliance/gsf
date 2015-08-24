//*******************************************************************************************************
//  Notifiers.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/05/2009 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************


namespace TVA.Historian.Notifiers
{
    /// <summary>
    /// A class that loads all <see cref="INotifier">Notifier</see> adapters.
    /// </summary>
    /// <seealso cref="INotifier"/>
    public class Notifiers : AdapterLoader<INotifier>
    {
        #region [ Members ]

        // Nested Types

        private class Notification
        {
            public Notification(string subject, string message, string details, NotificationTypes notificationType)
            {
                this.Subject = subject;
                this.Message = message;
                this.Details = details;
                this.NotificationType = notificationType;
            }

            public string Subject;
            public string Message;
            public string Details;
            public NotificationTypes NotificationType;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationTypes"/> values.</param>
        public void SendNotification(string subject, string message, NotificationTypes notificationType)
        {
            SendNotification(subject, message, "", notificationType);
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationTypes"/> values.</param>
        public void SendNotification(string subject, string message, string details, NotificationTypes notificationType)
        {
            OperationQueue.Add(new Notification(subject, message, details, notificationType));
        }

        /// <summary>
        /// Executes <see cref="INotifier.Notify"/> on the specified notifier <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter">An <see cref="INotifier"/> object.</param>
        /// <param name="data">Data about the notification.</param>
        protected override void ExecuteAdapterOperation(INotifier adapter, object data)
        {
            Notification notification = (Notification)data;
            adapter.Notify(notification.Subject, notification.Message, notification.Details, notification.NotificationType);
        }

        #endregion
    }
}
