using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using TVA.Configuration;
//using TVA.Collections.Common;

//*******************************************************************************************************
//  TVA.Windows.Commmon.vb - Common Functions for Windows Applications
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/25/2004 - J. Ritchie Carroll
//       Original version of source code generated
//  05/04/2006 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (TVA.Forms.Common)
//  10/30/2006 - J. Ritchie Carroll
//       Added left-most, right-most, top-most and bottom-most screen bound functions
//       Fixed an issue with forms showing up off the screen (esp. when switching primary monitor)
//  09/11/2007 - Pinal C. Patel
//       Saving and retrieving setting by instantiating local config file variable in order to avoid the
//       the "Config file has been modified" exception that is thrown when using shortcut methods
//
//*******************************************************************************************************


namespace TVA.Windows
{
	public sealed class Common
	{
		
		
		public const string LastWindowSizeSettingsCategory = "LastWindowSize";
		public const string LastWindowLocationSettingsCategory = "LastWindowLocation";
		
		#region " Private Window Setting Helper Class "
		
		private class WindowSetting
		{
			
			
			private int m_paramA;
			private int m_paramB;
			
			public WindowSetting(string setting)
			{
				
				if (! string.IsNullOrEmpty(setting))
				{
					if (setting[0] == '{' && setting[setting.Length- 1] == '}')
					{
						// Remove surrounding braces
						setting = setting.Substring(1, setting.Length- 2);
						
						if (! string.IsNullOrEmpty(setting))
						{
							string[] elements = setting.Split(',');
							if (elements.Length == 2)
							{
								m_paramA = System.Convert.ToInt32(elements[0].Split('=')[1]);
								m_paramB = System.Convert.ToInt32(elements[1].Split('=')[1]);
							}
						}
					}
				}
				
			}
			
			public int ParamA(int minimumValue, int maximumValue)
			{
				return ValidWindowParameter(m_paramA, minimumValue, maximumValue);
			}
			
			public int ParamB(int minimumValue, int maximumValue)
			{
				return ValidWindowParameter(m_paramB, minimumValue, maximumValue);
			}
			
			private int ValidWindowParameter(int value, int minimumValue, int maximumValue)
			{
				
				if (value > maximumValue)
				{
					value = maximumValue;
				}
				if (value < minimumValue)
				{
					value = minimumValue;
				}
				
				return value;
				
			}
			
		}
		
		#endregion
		
		private Common()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
		/// <summary>
		/// Saves the size and location information of the specified windowsForm to the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size and location information is to be saved.</param>
		/// <remarks>This function simply calls the SaveWindowSize and SaveWindowLocation functions using the default settings categories</remarks>
		public static void SaveWindowSettings(Form windowsForm)
		{
			
			SaveWindowSize(windowsForm);
			SaveWindowLocation(windowsForm);
			
		}
		
		/// <summary>
		/// Saves the size information of the specified windowsForm to the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size information is to be saved.</param>
		/// <remarks>This function uses the default settings category "LastWindowSize"</remarks>
		public static void SaveWindowSize(Form windowsForm)
		{
			
			SaveWindowSize(windowsForm, LastWindowSizeSettingsCategory);
			
		}
		
		/// <summary>
		/// Saves the size information of the specified windowsForm to the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size information is to be saved.</param>
		/// <param name="settingsCategory">Settings category used to persist form size information</param>
		public static void SaveWindowSize(Form windowsForm, string settingsCategory)
		{
			
			ConfigurationFile settings = new ConfigurationFile();
			TVA.Configuration.CategorizedSettingsElementCollection with_1 = settings.CategorizedSettings[settingsCategory];
			if (with_1[windowsForm.Name] != null)
			{
				with_1[windowsForm.Name].Value = windowsForm.Size.ToString();
			}
			else
			{
				with_1.Add(windowsForm.Name, windowsForm.Size.ToString());
			}
			
			settings.Save();
			
		}
		
		/// <summary>
		/// Saves the location information of the specified windowsForm to the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose location information is to be saved.</param>
		/// <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
		public static void SaveWindowLocation(Form windowsForm)
		{
			
			SaveWindowLocation(windowsForm, LastWindowLocationSettingsCategory);
			
		}
		
		/// <summary>
		/// Saves the location information of the specified windowsForm to the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose location information is to be saved.</param>
		/// <param name="settingsCategory">Settings category used to persist form location information</param>
		public static void SaveWindowLocation(Form windowsForm, string settingsCategory)
		{
			
			ConfigurationFile settings = new ConfigurationFile();
			TVA.Configuration.CategorizedSettingsElementCollection with_1 = settings.CategorizedSettings[settingsCategory];
			if (with_1[windowsForm.Name] != null)
			{
				with_1[windowsForm.Name].Value = windowsForm.Location.ToString();
			}
			else
			{
				with_1.Add(windowsForm.Name, windowsForm.Location.ToString());
			}
			
			settings.Save();
			
		}
		
		/// <summary>
		/// Restores the size and location of the specified windowsForm from the size and location information saved in the
		/// application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size and location is to be restored.</param>
		/// <remarks>This function simply calls the RestoreWindowSize and RestoreWindowLocation functions using the default settings categories</remarks>
		public static void RestoreWindowSettings(Form windowsForm)
		{
			
			RestoreWindowSize(windowsForm);
			RestoreWindowLocation(windowsForm);
			
		}
		
		/// <summary>
		/// Restores the size of the specified windowsForm from the size information saved in the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size is to be restored.</param>
		/// <remarks>This function uses the default settings category "LastWindowSize"</remarks>
		public static void RestoreWindowSize(Form windowsForm)
		{
			
			RestoreWindowSize(windowsForm, LastWindowSizeSettingsCategory);
			
		}
		
		/// <summary>
		/// Restores the size of the specified windowsForm from the size information saved in the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose size is to be restored.</param>
		/// <param name="settingsCategory">Settings category used to persist form size information</param>
		public static void RestoreWindowSize(Form windowsForm, string settingsCategory)
		{
			
			ConfigurationFile settings = new ConfigurationFile();
			if (settings.CategorizedSettings[settingsCategory][windowsForm.Name] != null)
			{
				// Restore last saved window size
				WindowSetting sizeSetting = new WindowSetting(settings.CategorizedSettings[settingsCategory][windowsForm.Name].Value());
				
				Form with_1 = windowsForm;
				with_1.Width = sizeSetting.ParamA(with_1.MinimumSize.Width, GetTotalScreenWidth());
				with_1.Height = sizeSetting.ParamB(with_1.MinimumSize.Height, GetMaximumScreenHeight());
			}
			
		}
		
		/// <summary>
		/// Restores the location of the specified windowsForm from the location information saved in the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose location is to be restored.</param>
		/// <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
		public static void RestoreWindowLocation(Form windowsForm)
		{
			
			RestoreWindowLocation(windowsForm, LastWindowLocationSettingsCategory);
			
		}
		
		/// <summary>
		/// Restores the location of the specified windowsForm from the location information saved in the application configuration file.
		/// </summary>
		/// <param name="windowsForm">The Form whose location is to be restored.</param>
		/// <param name="settingsCategory">Settings category used to persist form location information</param>
		public static void RestoreWindowLocation(Form windowsForm, string settingsCategory)
		{
			
			ConfigurationFile settings = new ConfigurationFile();
			if (settings.CategorizedSettings[settingsCategory][windowsForm.Name] != null)
			{
				// Restore last saved window location
				WindowSetting locationSetting = new WindowSetting(settings.CategorizedSettings[settingsCategory][windowsForm.Name].Value());
				
				Form with_1 = windowsForm;
				with_1.Left = locationSetting.ParamA(GetLeftMostScreenBound(), GetRightMostScreenBound() - with_1.MinimumSize.Width);
				with_1.Top = locationSetting.ParamB(GetTopMostScreenBound(), GetBottomMostScreenBound() - with_1.MinimumSize.Height);
			}
			
		}
		
		/// <summary>
		/// Gets the least "x" coordinate of all screens on the system
		/// </summary>
		/// <returns>The smallest visible "x" screen coordinate</returns>
		public static int GetLeftMostScreenBound()
		{
			
			int leftBound;
			
			// Return the left-most "x" screen coordinate
			foreach (Screen display in Screen.AllScreens)
			{
				if (leftBound > display.Bounds.X)
				{
					leftBound = display.Bounds.X;
				}
			}
			
			return leftBound;
			
		}
		
		/// <summary>
		/// Gets the greatest "x" coordinate of all screens on the system
		/// </summary>
		/// <returns>The largest visible "x" screen coordinate</returns>
		public static int GetRightMostScreenBound()
		{
			
			int rightBound;
			
			// Return the right-most "x" screen coordinate
			foreach (Screen display in Screen.AllScreens)
			{
				if (rightBound < display.Bounds.X + display.Bounds.Width)
				{
					rightBound = display.Bounds.X + display.Bounds.Width;
				}
			}
			
			return rightBound;
			
		}
		
		/// <summary>
		/// Gets the least "y" coordinate of all screens on the system
		/// </summary>
		/// <returns>The smallest visible "y" screen coordinate</returns>
		public static int GetTopMostScreenBound()
		{
			
			int topBound;
			
			// Return the top-most "y" screen coordinate
			foreach (Screen display in Screen.AllScreens)
			{
				if (topBound > display.Bounds.Y)
				{
					topBound = display.Bounds.Y;
				}
			}
			
			return topBound;
			
		}
		
		/// <summary>
		/// Gets the greatest "y" coordinate of all screens on the system
		/// </summary>
		/// <returns>The largest visible "y" screen coordinate</returns>
		public static int GetBottomMostScreenBound()
		{
			
			int bottomBound;
			
			// Return the bottom-most "y" screen coordinate
			foreach (Screen display in Screen.AllScreens)
			{
				if (bottomBound < display.Bounds.Y + display.Bounds.Height)
				{
					bottomBound = display.Bounds.Y + display.Bounds.Height;
				}
			}
			
			return bottomBound;
			
		}
		
		/// <summary>
		/// Gets the total width of all the screens assuming the screens are side-by-side.
		/// </summary>
		/// <returns>The total width of all the screens assuming the screens are side-by-side.</returns>
		public static int GetTotalScreenWidth()
		{
			
			int totalWidth;
			
			// We just assume screens are side-by-side and get cumulative screen widths
			foreach (Screen display in Screen.AllScreens)
			{
				totalWidth += display.Bounds.Width;
			}
			
			return totalWidth;
			
		}
		
		/// <summary>
		/// Gets the height of the screen with the highest resolution.
		/// </summary>
		/// <returns>The height of the screen with the highest resolution.</returns>
		public static int GetMaximumScreenHeight()
		{
			
			int maxHeight;
			
			// In this case we just get the largest screen height
			foreach (Screen display in Screen.AllScreens)
			{
				if (maxHeight < display.Bounds.Height)
				{
					maxHeight = display.Bounds.Height;
				}
			}
			
			return maxHeight;
			
		}
		
		/// <summary>
		/// Gets the height of the screen with the lowest resolution.
		/// </summary>
		/// <returns>The height of the screen with the lowest resolution.</returns>
		public static int GetMinimumScreenHeight()
		{
			
			int minHeight;
			
			// In this case we just get the smallest screen height
			foreach (Screen display in Screen.AllScreens)
			{
				if (minHeight > display.Bounds.Height)
				{
					minHeight = display.Bounds.Height;
				}
			}
			
			return minHeight;
			
		}
		
	}
	
}
