//*******************************************************************************************************
//  INotifier.cs
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

using TVA.Configuration;

namespace TVA.Historian.Notifiers
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the type of notification being sent using a <see cref="INotifier">Notifier</see>.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Notification is informational in nature.
        /// </summary>
        Information,
        /// <summary>
        /// Notification is being sent to report a warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Notification is being sent to report an alarm.
        /// </summary>
        Alarm,
        /// <summary>
        /// Notification is being sent to report activity.
        /// </summary>
        Heartbeat
    }

    #endregion

    /// <summary>
    /// Defines a notifier that can process notification messages.
    /// </summary>
    /// <seealso cref="NotificationType"/>
    public interface INotifier : IPersistSettings
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Alarm"/> notifications will be processed.
        /// </summary>
        bool NotifiesAlarms { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Warning"/> notifications will be processed.
        /// </summary>
        bool NotifiesWarnings { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Information"/> notifications will be processed.
        /// </summary>
        bool NotifiesInformation { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="NotificationType.Heartbeat"/> notifications will be processed.
        /// </summary>
        bool NotifiesHeartbeat { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Process a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationType"/> values.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        bool Notify(string subject, string message, string details, NotificationType notificationType);

        #endregion
    }
}