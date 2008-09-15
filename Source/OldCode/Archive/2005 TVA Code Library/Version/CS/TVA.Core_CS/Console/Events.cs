//*******************************************************************************************************
//  TVA.Console.Common.vb - Common Configuration Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/28/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TVA.Console
{
    public static class Common
    {
        private enum ConsoleEventType
        {
            CancelKeyPress = 0,
            BreakKeyPress = 1,
            ConsoleClosing = 2,
            UserLoggingOff = 5,
            SystemShutdown = 6
        }

        private delegate bool ConsoleWindowEventHandler(ConsoleEventType controlType);

        private static ConsoleWindowEventHandler m_handler;

        public static event EventHandler<CancelEventArgs> CancelKeyPress;

        public static event EventHandler<CancelEventArgs> BreakKeyPress;

        public static event EventHandler<CancelEventArgs> ConsoleClosing;

        public static event EventHandler UserLoggingOff;

        public static event EventHandler SystemShutdown;

        #region " Public Code "

        public static void EnableRaisingEvents()
        {
            // Member variable is used here so that the delegate is not garbage collected by the time it is called
            // by WIN API when any of the control events take place.
            // http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=996045&SiteID=1
            m_handler = HandleConsoleWindowEvents;
            SetConsoleWindowEventRaising(m_handler, true);
        }

        public static void DisableRaisingEvents()
        {
            m_handler = HandleConsoleWindowEvents;
            SetConsoleWindowEventRaising(m_handler, false);
        }

        #endregion

        #region " Private Code "

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler")]
        private static extern bool SetConsoleWindowEventRaising(ConsoleWindowEventHandler handler, bool enable);

        private static bool HandleConsoleWindowEvents(ConsoleEventType controlType)
        {
            // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/handlerroutine.htm

            // When this function does not return True, the default handler is called and the default action takes
            // place.
            switch (controlType)
            {
                case ConsoleEventType.CancelKeyPress:
                    CancelEventArgs ctrlCKeyPressEventData = new CancelEventArgs();

                    if (CancelKeyPress != null)
                        CancelKeyPress(null, ctrlCKeyPressEventData);

                    if (ctrlCKeyPressEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.BreakKeyPress:
                    CancelEventArgs ctrlBreakKeyPressEventData = new CancelEventArgs();

                    if (BreakKeyPress != null)
                        BreakKeyPress(null, ctrlBreakKeyPressEventData);

                    if (ctrlBreakKeyPressEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.ConsoleClosing:
                    CancelEventArgs consoleClosingEventData = new CancelEventArgs();

                    if (ConsoleClosing != null)
                        ConsoleClosing(null, consoleClosingEventData);

                    if (consoleClosingEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.UserLoggingOff:
                    if (UserLoggingOff != null)
                        UserLoggingOff(null, EventArgs.Empty);

                    break;
                case ConsoleEventType.SystemShutdown:
                    if (SystemShutdown != null)
                        SystemShutdown(null, EventArgs.Empty);

                    break;
            }

            return false;
        }

        #endregion
    }
}