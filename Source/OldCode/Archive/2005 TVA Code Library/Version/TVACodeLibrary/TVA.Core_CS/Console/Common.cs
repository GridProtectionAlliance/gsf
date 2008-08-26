using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Runtime.InteropServices;

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



namespace TVA
{
	namespace Console
	{
		
		public sealed class Common
		{
			
			
			#region " Member Declaration "
			
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
			
			#endregion
			
			#region " Event Declaration "
			
			public  delegate void CancelKeyPressEventHandler(object Of);
			private CancelKeyPressEventHandler CancelKeyPressEvent;
			
			public static event CancelKeyPressEventHandler CancelKeyPress
			{
				add
				{
					CancelKeyPressEvent = (CancelKeyPressEventHandler) System.Delegate.Combine(CancelKeyPressEvent, value);
				}
				remove
				{
					CancelKeyPressEvent = (CancelKeyPressEventHandler) System.Delegate.Remove(CancelKeyPressEvent, value);
				}
			}
			
			
			public  delegate void BreakKeyPressEventHandler(object Of);
			private BreakKeyPressEventHandler BreakKeyPressEvent;
			
			public static event BreakKeyPressEventHandler BreakKeyPress
			{
				add
				{
					BreakKeyPressEvent = (BreakKeyPressEventHandler) System.Delegate.Combine(BreakKeyPressEvent, value);
				}
				remove
				{
					BreakKeyPressEvent = (BreakKeyPressEventHandler) System.Delegate.Remove(BreakKeyPressEvent, value);
				}
			}
			
			
			public  delegate void ConsoleClosingEventHandler(object Of);
			private ConsoleClosingEventHandler ConsoleClosingEvent;
			
			public static event ConsoleClosingEventHandler ConsoleClosing
			{
				add
				{
					ConsoleClosingEvent = (ConsoleClosingEventHandler) System.Delegate.Combine(ConsoleClosingEvent, value);
				}
				remove
				{
					ConsoleClosingEvent = (ConsoleClosingEventHandler) System.Delegate.Remove(ConsoleClosingEvent, value);
				}
			}
			
			
			private EventHandler UserLoggingOffEvent;
			public static event EventHandler UserLoggingOff
			{
				add
				{
					UserLoggingOffEvent = (EventHandler) System.Delegate.Combine(UserLoggingOffEvent, value);
				}
				remove
				{
					UserLoggingOffEvent = (EventHandler) System.Delegate.Remove(UserLoggingOffEvent, value);
				}
			}
			
			
			private EventHandler SystemShutdownEvent;
			public static event EventHandler SystemShutdown
			{
				add
				{
					SystemShutdownEvent = (EventHandler) System.Delegate.Combine(SystemShutdownEvent, value);
				}
				remove
				{
					SystemShutdownEvent = (EventHandler) System.Delegate.Remove(SystemShutdownEvent, value);
				}
			}
			
			
			#endregion
			
			#region " Public Code "
			
			// VB does not provide you with an array of tokenized command line arguments; they are all in one string.
			// So, this function creates the desired tokenized argument array from the VB command line.
			// This function will always return at least one argument, even if it is an empty string.
			public static string[] ParseCommand(string command)
			{
				
				List<string> parsedCommand = new List<string>();
				if (command.Length > 0)
				{
					string quotedArgument = "";
					var encodedQuote = Guid.NewGuid().ToString();
					var encodedSpace = Guid.NewGuid().ToString();
					StringBuilder encodedCommand = new StringBuilder();
					bool argumentInQuotes;
					char currentCharacter;
					
					// Encodes embedded quotes. It allows embedded/nested quotes encoded as \".
					command = command.Replace("\\\"", encodedQuote);
					
					// Combines any quoted strings into a single arg by encoding embedded spaces.
					for (int x = 0; x <= command.Length - 1; x++)
					{
						currentCharacter = command[x];
						
						if (currentCharacter == '\"')
						{
							if (argumentInQuotes)
							{
								argumentInQuotes = false;
							}
							else
							{
								argumentInQuotes = true;
							}
						}
						
						if (argumentInQuotes)
						{
							if (currentCharacter == ' ')
							{
								encodedCommand.Append(encodedSpace);
							}
							else
							{
								encodedCommand.Append(currentCharacter);
							}
						}
						else
						{
							encodedCommand.Append(currentCharacter);
						}
					}
					
					command = encodedCommand.ToString();
					
					// Parses every argument out by space and combine any quoted strings into a single arg.
					foreach (string argument in command.Split(' '))
					{
						// Adds tokenized argument, making sure to unencode any embedded quotes or spaces.
						argument = argument.Replace(encodedQuote, "\"").Replace(encodedSpace, " ").Trim();
						if (argument.Length > 0)
						{
							parsedCommand.Add(argument);
						}
					}
				}
				
				return parsedCommand.ToArray();
				
			}
			
			public static void EnableRaisingEvents()
			{
				
				// Member variable is used here so that the delegate is not garbage collected by the time it is called
				// by WIN API when any of the control events take place.
				// http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=996045&SiteID=1
				m_handler = new System.EventHandler(HandleConsoleWindowEvents);
				SetConsoleWindowEventRaising(m_handler, true);
				
			}
			
			public static void DisableRaisingEvents()
			{
				
				m_handler = new System.EventHandler(HandleConsoleWindowEvents);
				SetConsoleWindowEventRaising(m_handler, false);
				
			}
			
			#endregion
			
			#region " Private Code "
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated
				
			}
			
			[DllImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler")]private static  extern bool SetConsoleWindowEventRaising(ConsoleWindowEventHandler handler, bool enable);
			
			
			private static bool HandleConsoleWindowEvents(ConsoleEventType controlType)
			{
				
				// ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/handlerroutine.htm
				
				// When this function does not return True, the default handler is called and the default action takes
				// place.
				switch (controlType)
				{
					case ConsoleEventType.CancelKeyPress:
						System.ComponentModel.CancelEventArgs ctrlCKeyPressEventData = new System.ComponentModel.CancelEventArgs();
						if (CancelKeyPressEvent != null)
							CancelKeyPressEvent(null, ctrlCKeyPressEventData);
						if (ctrlCKeyPressEventData.Cancel)
						{
							return true;
						}
						break;
					case ConsoleEventType.BreakKeyPress:
						System.ComponentModel.CancelEventArgs ctrlBreakKeyPressEventData = new System.ComponentModel.CancelEventArgs();
						if (BreakKeyPressEvent != null)
							BreakKeyPressEvent(null, ctrlBreakKeyPressEventData);
						if (ctrlBreakKeyPressEventData.Cancel)
						{
							return true;
						}
						break;
					case ConsoleEventType.ConsoleClosing:
						System.ComponentModel.CancelEventArgs consoleClosingEventData = new System.ComponentModel.CancelEventArgs();
						if (ConsoleClosingEvent != null)
							ConsoleClosingEvent(null, consoleClosingEventData);
						if (consoleClosingEventData.Cancel)
						{
							return true;
						}
						break;
					case ConsoleEventType.UserLoggingOff:
						if (UserLoggingOffEvent != null)
							UserLoggingOffEvent(null, EventArgs.Empty);
						break;
					case ConsoleEventType.SystemShutdown:
						if (SystemShutdownEvent != null)
							SystemShutdownEvent(null, EventArgs.Empty);
						break;
				}
				
				return false;
				
			}
			
			#endregion
			
		}
		
	}
}
