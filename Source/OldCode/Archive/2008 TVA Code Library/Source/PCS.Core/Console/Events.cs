//*******************************************************************************************************
//  Events.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/28/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//  09/29/2008 - Pinal C. Patel
//      Entered code comments.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PCS.Console
{
    /// <summary>
    /// A helper class that can be used to subscribe to events raised by a console application.
    /// </summary>
    /// <example>
    /// This example shows how to subscribe to console application events:
    /// <code>
    /// using System;
    /// using PCS.Console;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Subscribe to console events.
    ///         Events.CancelKeyPress += Events_CancelKeyPress;
    ///         Events.ConsoleClosing += Events_ConsoleClosing;
    ///         Events.EnableRaisingEvents();
    ///        
    ///         Console.ReadLine();
    ///     }
    ///    
    ///     static void Events_CancelKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
    ///     {
    ///         // Abort processing.
    ///     }
    ///    
    ///     static void Events_ConsoleClosing(object sender, System.ComponentModel.CancelEventArgs e)
    ///     {
    ///         // Put clean-up code.
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class Events
    {
        private enum ConsoleEventType
        {
            CancelKeyPress = 0,
            BreakKeyPress = 1,
            ConsoleClosing = 2,
            UserLoggingOff = 5,
            SystemShutdown = 6
        }

        private static ConsoleWindowEventHandler m_handler;

        private delegate bool ConsoleWindowEventHandler(ConsoleEventType controlType);

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler")]
        private static extern bool SetConsoleWindowEventRaising(ConsoleWindowEventHandler handler, bool enable);

        /// <summary>
        /// Occurs when CTRL+C signal is received from keyboard input.
        /// </summary>
        /// <remarks>
        /// <see cref="EnableRaisingEvents"/> method must be called to enable event publication.
        /// </remarks>
        public static event EventHandler<CancelEventArgs> CancelKeyPress;

        /// <summary>
        /// Occurs when CTRL+BREAK signal is received from keyboard input.
        /// </summary>
        /// <remarks>
        /// <see cref="EnableRaisingEvents"/> method must be called to enable event publication.
        /// </remarks>
        public static event EventHandler<CancelEventArgs> BreakKeyPress;

        /// <summary>
        /// Occurs when the user closes the console application window.
        /// </summary>
        /// <remarks>
        /// <see cref="EnableRaisingEvents"/> method must be called to enable event publication.
        /// </remarks>
        public static event EventHandler<CancelEventArgs> ConsoleClosing;

        /// <summary>
        /// Occurs when the user is logging off.
        /// </summary>
        /// <remarks>
        /// <see cref="EnableRaisingEvents"/> method must be called to enable event publication.
        /// </remarks>
        public static event EventHandler UserLoggingOff;

        /// <summary>
        /// Occurs when the system is shutting down.
        /// </summary>
        /// <remarks>
        /// <see cref="EnableRaisingEvents"/> method must be called to enable event publication.
        /// </remarks>
        public static event EventHandler SystemShutdown;

        /// <summary>
        /// Enables the raising of console application <see cref="Events"/>. Prior to calling this method, handlers 
        /// must be defined for the <see cref="Events"/> raised by a console application.
        /// </summary>
        public static void EnableRaisingEvents()
        {
            // Member variable is used here so that the delegate is not garbage collected by the time it is called
            // by WIN API when any of the control events take place.
            // http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=996045&SiteID=1
            m_handler = HandleConsoleWindowEvents;
            SetConsoleWindowEventRaising(m_handler, true);
        }

        /// <summary>
        /// Enables the raising of console application <see cref="Events"/>. 
        /// </summary>
        public static void DisableRaisingEvents()
        {
            m_handler = HandleConsoleWindowEvents;
            SetConsoleWindowEventRaising(m_handler, false);
        }

        /// <summary>
        /// Delegate method that gets called when console application events occur.
        /// </summary>
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
    }
}