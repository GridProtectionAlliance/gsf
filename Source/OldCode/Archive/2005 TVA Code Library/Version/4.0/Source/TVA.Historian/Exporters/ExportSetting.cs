//*******************************************************************************************************
//  ExportSetting.cs
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
//  06/13/2007 - Pinal C. Patel
//       Original version of source code generated.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Historian.Exporters
{
    /// <summary>
    /// A class that can be used to add custom settings to an <see cref="Export"/>.
    /// </summary>
    /// <seealso cref="Export"/>
    [Serializable()]
    public class ExportSetting
    {
        #region [ Members ]

        // Fields
        private string m_name;
        private string m_value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportSetting"/> class.
        /// </summary>
        public ExportSetting()
            : this("Name", "Value")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportSetting"/> class.
        /// </summary>
        /// <param name="name">Name of the <see cref="ExportSetting"/>.</param>
        /// <param name="value">Value of the <see cref="ExportSetting"/>.</param>
        public ExportSetting(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="ExportSetting"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ExportSetting"/>.
        /// </summary>
        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        #endregion
    }
}