//*******************************************************************************************************
//  FormExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
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
//  10/01/2008 - James R Carroll
//       Converted to C# extensions.
//
//*******************************************************************************************************

using System.Windows.Forms;
using TVA.Configuration;

namespace TVA.Windows.Forms
{
    /// <summary>Extensions applied to all System.Windows.Forms.Form objects.</summary>
    public static class FormExtensions
	{
        private class WindowSetting
        {
            #region [ Members ]

            // Fields
            private int m_paramA;
            private int m_paramB;

            #endregion

            #region [ Constructors ]

            public WindowSetting(string setting)
            {
                if (!string.IsNullOrEmpty(setting))
                {
                    if (setting[0] == '{' && setting[setting.Length - 1] == '}')
                    {
                        // Remove surrounding braces
                        setting = setting.Substring(1, setting.Length - 2);

                        if (!string.IsNullOrEmpty(setting))
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

            #endregion

            #region [ Methods ]

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
                    value = maximumValue;

                if (value < minimumValue)
                    value = minimumValue;

                return value;
            }

            #endregion
        }

        /// <summary>
        /// Config file category under which the window size information will be saved.
        /// </summary>
        public const string LastWindowSizeSettingsCategory = "LastWindowSize";

        /// <summary>
        /// Config file category under which the window location information will be saved.
        /// </summary>
        public const string LastWindowLocationSettingsCategory = "LastWindowLocation";

        /// <summary>
        /// Saves the size and location information of the specified windowsForm to the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size and location information is to be saved.</param>
        /// <remarks>This function simply calls the SaveWindowSize and SaveWindowLocation functions using the default settings categories</remarks>
        public static void SaveLayout(this Form windowsForm)
        {
            windowsForm.SaveSize();
            windowsForm.SaveLocation();
        }

        /// <summary>
        /// Restores the size and location of the specified windowsForm from the size and location information saved in the
        /// application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size and location is to be restored.</param>
        /// <remarks>This function simply calls the RestoreWindowSize and RestoreWindowLocation functions using the default settings categories</remarks>
        public static void RestoreLayout(this Form windowsForm)
        {
            windowsForm.RestoreSize();
            windowsForm.RestoreLocation();
        }

        /// <summary>
        /// Saves the size information of the specified windowsForm to the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size information is to be saved.</param>
        /// <remarks>This function uses the default settings category "LastWindowSize"</remarks>
        public static void SaveSize(this Form windowsForm)
        {
            windowsForm.SaveSize(LastWindowSizeSettingsCategory);
        }

        /// <summary>
        /// Saves the size information of the specified windowsForm to the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size information is to be saved.</param>
        /// <param name="settingsCategory">Settings category used to persist form size information</param>
        public static void SaveSize(this Form windowsForm, string settingsCategory)
        {
            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[settingsCategory];

            if (settings[windowsForm.Name] != null)
                settings[windowsForm.Name].Value = windowsForm.Size.ToString();
            else
                settings.Add(windowsForm.Name, windowsForm.Size.ToString());

            ConfigurationFile.Current.Save();
        }

        /// <summary>
        /// Saves the location information of the specified windowsForm to the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose location information is to be saved.</param>
        /// <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
        public static void SaveLocation(this Form windowsForm)
        {
            windowsForm.SaveLocation(LastWindowLocationSettingsCategory);
        }

        /// <summary>
        /// Saves the location information of the specified windowsForm to the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose location information is to be saved.</param>
        /// <param name="settingsCategory">Settings category used to persist form location information</param>
        public static void SaveLocation(this Form windowsForm, string settingsCategory)
        {
            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[settingsCategory];

            if (settings[windowsForm.Name] != null)
                settings[windowsForm.Name].Value = windowsForm.Location.ToString();
            else
                settings.Add(windowsForm.Name, windowsForm.Location.ToString());

            ConfigurationFile.Current.Save();
        }

        /// <summary>
        /// Restores the size of the specified windowsForm from the size information saved in the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size is to be restored.</param>
        /// <remarks>This function uses the default settings category "LastWindowSize"</remarks>
        public static void RestoreSize(this Form windowsForm)
        {
            windowsForm.RestoreSize(LastWindowSizeSettingsCategory);
        }

        /// <summary>
        /// Restores the size of the specified windowsForm from the size information saved in the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose size is to be restored.</param>
        /// <param name="settingsCategory">Settings category used to persist form size information</param>
        public static void RestoreSize(this Form windowsForm, string settingsCategory)
        {
            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[settingsCategory];

            if (settings[windowsForm.Name] != null)
            {
                // Restore last saved window size
                WindowSetting sizeSetting = new WindowSetting(settings[windowsForm.Name].Value);
                windowsForm.Width = sizeSetting.ParamA(windowsForm.MinimumSize.Width, ScreenArea.MaximumWidth);
                windowsForm.Height = sizeSetting.ParamB(windowsForm.MinimumSize.Height, ScreenArea.MaximumHeight);
            }
        }

        /// <summary>
        /// Restores the location of the specified windowsForm from the location information saved in the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose location is to be restored.</param>
        /// <remarks>This function uses the default settings category "LastWindowLocation"</remarks>
        public static void RestoreLocation(this Form windowsForm)
        {
            windowsForm.RestoreLocation(LastWindowLocationSettingsCategory);
        }

        /// <summary>
        /// Restores the location of the specified windowsForm from the location information saved in the application configuration file.
        /// </summary>
        /// <param name="windowsForm">The Form whose location is to be restored.</param>
        /// <param name="settingsCategory">Settings category used to persist form location information</param>
        public static void RestoreLocation(this Form windowsForm, string settingsCategory)
        {
            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[settingsCategory];

            if (settings[windowsForm.Name] != null)
            {
                // Restore last saved window location
                WindowSetting locationSetting = new WindowSetting(settings[windowsForm.Name].Value);
                windowsForm.Left = locationSetting.ParamA(ScreenArea.LeftMostBound, ScreenArea.RightMostBound - windowsForm.MinimumSize.Width);
                windowsForm.Top = locationSetting.ParamB(ScreenArea.TopMostBound, ScreenArea.BottomMostBound - windowsForm.MinimumSize.Height);
            }
        }
    }
}
