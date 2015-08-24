//*******************************************************************************************************
//  IPersistSettings.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/21/2007 - Pinal C Patel
//       Generated original version of source code.
//  09/16/2008 - Pinal C Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C Patel
//       Reviewed code comments.
//
//*******************************************************************************************************

namespace TVA.Configuration
{
    /// <summary>
    /// Specifies that this object can persists settings to a config file.
    /// </summary>
    public interface IPersistSettings
    {
        /// <summary>
        /// Determines whether the object settings are to be persisted to the config file.
        /// </summary>
        bool PersistSettings { get; set; }

        /// <summary>
        /// Gets or sets the category name under which the object settings are persisted in the config file.
        /// </summary>
        string SettingsCategoryName { get; set; }

        /// <summary>
        /// Saves settings to the config file.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Loads saved settings from the config file.
        /// </summary>
        void LoadSettings();
    }
}
