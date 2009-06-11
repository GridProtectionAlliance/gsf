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

namespace DatAWare.Notifiers
{
    #region [ Enumerations ]

    public enum NotificationType
    {
        Information,
        Warning,
        Alarm,
        Heartbeat
    }

    #endregion

    public interface INotifier : IPersistSettings
    {
        #region [ Properties ]

        bool NotifiesAlarms { get; set; }

        bool NotifiesWarnings { get; set; }

        bool NotifiesInformation { get; set; }

        bool NotifiesHeartbeat { get; set; }

        #endregion

        #region [ Methods ]

        bool Notify(string subject, string message, string details, NotificationType notificationType);

        #endregion
    }
}