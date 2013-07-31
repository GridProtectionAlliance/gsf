//******************************************************************************************************
//  AdapterConnectionStringParameter.cs - Gbtc
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
//  06/09/2011 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// View-model for a key-value pair in an adapter connection string.
    /// This can also represent key-value pairs which aren't necessarily
    /// in the connection string, but rather are defined by properties
    /// in an adapter class. This view-model is used by the
    /// <see cref="GSF.TimeSeries.UI.UserControls.AdapterUserControl"/>.
    /// </summary>
    public class AdapterConnectionStringParameter : ViewModelBase
    {
        #region [ Members ]

        // Fields
        private PropertyInfo m_info;
        private string m_name;
        private string m_description;
        private object m_value;
        private object m_defaultValue;
        private bool m_isRequired;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="PropertyInfo"/> of the
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/>
        /// associated with this <see cref="AdapterConnectionStringParameter"/>.
        /// </summary>
        public PropertyInfo Info
        {
            get
            {
                return m_info;
            }
            set
            {
                m_info = value;
                OnPropertyChanged("Info");
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="AdapterConnectionStringParameter"/>
        /// which is either a key in the connection string or the name of a property in
        /// the adapter class.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the description of the <see cref="AdapterConnectionStringParameter"/>
        /// obtained through the <see cref="Info"/> using reflection. A property annotated with
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/>
        /// must also define a <see cref="System.ComponentModel.DescriptionAttribute"/> for this
        /// to become populated.
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="AdapterConnectionStringParameter"/> as defined
        /// by either the connection string or the <see cref="DefaultValue"/> of the parameter.
        /// </summary>
        public object Value
        {
            get
            {
                return m_value ?? m_defaultValue;
            }
            set
            {
                m_value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("Color");
                OnPropertyChanged("Boldness");
            }
        }

        /// <summary>
        /// Gets or sets the default value of the <see cref="AdapterConnectionStringParameter"/>
        /// obtained through the <see cref="Info"/> using reflection. A property annotated with
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/> must
        /// also define a <see cref="System.ComponentModel.DefaultValueAttribute"/> for this to
        /// be populated.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return m_defaultValue;
            }
            set
            {
                m_defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this parameter is defined by a property
        /// that is annotated with the <see cref="System.ComponentModel.DefaultValueAttribute"/>.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return m_isRequired;
            }
            set
            {
                m_isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        /// <summary>
        /// Gets or sets the color of the item in the <see cref="System.Windows.Controls.ListBox"/>
        /// that the <see cref="AdapterConnectionStringParameter"/> is modeling.
        /// </summary>
        public Brush Color
        {
            get
            {
                bool red = m_isRequired && (m_value == null);
                return red ? Brushes.Red : Brushes.Black;
            }
        }

        /// <summary>
        /// Gets or sets the boldness of the item in the <see cref="System.Windows.Controls.ListBox"/> that the
        /// <see cref="AdapterConnectionStringParameter"/> is modeling.
        /// </summary>
        public FontWeight Boldness
        {
            get
            {
                return (m_value == null) ? FontWeights.Normal : FontWeights.Bold;
            }
        }

        /// <summary>
        /// Gets a list of enum types if this <see cref="AdapterConnectionStringParameter"/>'s type is an enum.
        /// If it is not an enum, this returns null.
        /// </summary>
        public List<string> EnumValues
        {
            get
            {
                if (!IsEnum)
                    return null;

                return Enum.GetValues(m_info.PropertyType)
                    .Cast<object>()
                    .Select(obj => obj.ToString())
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="System.Windows.Controls.RadioButton"/>
        /// labeled "False" is checked. Since the actual value of this <see cref="AdapterConnectionStringParameter"/>
        /// is represented by <see cref="Value"/>, and that value is what goes into the connection string
        /// this simply returns true if the value in the Value property is the word "false".
        /// </summary>
        public bool IsFalseChecked
        {
            get
            {
                return Value.ToNonNullString().Equals(false.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the value of this parameter can be configured via a
        /// custom control. This determines whether the hyperlink that links to the custom configuration
        /// popup is visible.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                try
                {
                    return (m_info != null) && (m_info.GetCustomAttribute<CustomConfigurationEditorAttribute>() != null);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is defined to be a <see cref="bool"/> in the adapter type. This determines whether the
        /// <see cref="System.Windows.Controls.RadioButton"/>s labeled "True" and "False" are visible.
        /// </summary>
        public bool IsBoolean
        {
            get
            {
                return !IsCustom && (m_info != null) && (m_info.PropertyType == typeof(bool));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is defined to be an enum in the adapter type. This determines whether the
        /// <see cref="System.Windows.Controls.ComboBox"/> bound to the enum values is visible.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                return !IsCustom && (m_info != null) && m_info.PropertyType.IsEnum;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is something other than a <see cref="bool"/> or an enum. This determines whether the
        /// <see cref="System.Windows.Controls.TextBox"/> bound to the <see cref="Value"/> of the parameters is
        /// visible. The value is true for most parameters, including those which are not defined by an adapter type.
        /// </summary>
        public bool IsOther
        {
            get
            {
                return !IsCustom && !IsBoolean && !IsEnum;
            }
        }

        #endregion
    }
}
