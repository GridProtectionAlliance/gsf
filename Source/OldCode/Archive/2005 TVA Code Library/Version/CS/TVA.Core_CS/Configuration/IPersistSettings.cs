using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 03/21/2007

namespace TVA
{
    namespace Configuration
    {

        public interface IPersistSettings
        {

            bool PersistSettings
            {
                get;
                set;
            }

            string SettingsCategoryName
            {
                get;
                set;
            }

            void SaveSettings();

            void LoadSettings();

        }

    }
}
