//******************************************************************************************************
//  Events.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/28/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/29/2008 - Pinal C. Patel
//       Entered code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GSF.Console
{
    /// <summary>
    /// Defines a set of consumable events that can be raised by a console application.
    /// </summary>
    /// <remarks>
    /// Note that no events will be raised by this class when running under Mono deployments, the class
    /// remains available in the Mono build as a stub to allow existing code to still compile and run.
    /// </remarks>
    /// <example>
    /// This example shows how to subscribe to console application events:
    /// <code>
    /// using System;
    /// using GSF.Console;
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

        private delegate bool ConsoleWindowEventHandler(ConsoleEventType controlType);
        private static ConsoleWindowEventHandler s_handler;

#if !MONO
        // TODO: See if there a standard POSIX way to pickup similar events
        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetConsoleWindowEventRaising(ConsoleWindowEventHandler handler, [MarshalAs(UnmanagedType.Bool)] bool enable);
#endif

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
        /// <remarks>
        /// This method is currently ignored under Mono deployments.
        /// </remarks>
        public static void EnableRaisingEvents()
        {
            s_handler = HandleConsoleWindowEvents;
#if !MONO

            // Member variable is used here so that the delegate is not garbage collected by the time it is called
            // by WIN API when any of the control events take place.
            // http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=996045&SiteID=1
            SetConsoleWindowEventRaising(s_handler, true);
#endif
        }

        /// <summary>
        /// Enables the raising of console application <see cref="Events"/>. 
        /// </summary>
        /// <remarks>
        /// This method is currently ignored under Mono deployments.
        /// </remarks>
        public static void DisableRaisingEvents()
        {
            s_handler = HandleConsoleWindowEvents;
#if !MONO
            SetConsoleWindowEventRaising(s_handler, false);
#endif
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

                    if ((object)CancelKeyPress != null)
                        CancelKeyPress(null, ctrlCKeyPressEventData);

                    if (ctrlCKeyPressEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.BreakKeyPress:
                    CancelEventArgs ctrlBreakKeyPressEventData = new CancelEventArgs();

                    if ((object)BreakKeyPress != null)
                        BreakKeyPress(null, ctrlBreakKeyPressEventData);

                    if (ctrlBreakKeyPressEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.ConsoleClosing:
                    CancelEventArgs consoleClosingEventData = new CancelEventArgs();

                    if ((object)ConsoleClosing != null)
                        ConsoleClosing(null, consoleClosingEventData);

                    if (consoleClosingEventData.Cancel)
                        return true;

                    break;
                case ConsoleEventType.UserLoggingOff:
                    if ((object)UserLoggingOff != null)
                        UserLoggingOff(null, EventArgs.Empty);

                    break;
                case ConsoleEventType.SystemShutdown:
                    if ((object)SystemShutdown != null)
                        SystemShutdown(null, EventArgs.Empty);

                    break;
            }

            return false;
        }
    }
}