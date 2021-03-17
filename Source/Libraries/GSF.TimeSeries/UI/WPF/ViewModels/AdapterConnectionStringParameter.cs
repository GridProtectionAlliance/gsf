//******************************************************************************************************
//  AdapterConnectionStringParameter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GSF.TimeSeries.Adapters;

// ReSharper disable ValueParameterNotUsed
namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// View-model for a key-value pair in an adapter connection string.
    /// This can also represent key-value pairs which aren't necessarily
    /// in the connection string, but rather are defined by properties
    /// in an adapter class. This view-model is used by the
    /// <see cref="UserControls.AdapterUserControl"/>.
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
        /// <see cref="ConnectionStringParameterAttribute"/>
        /// associated with this <see cref="AdapterConnectionStringParameter"/>.
        /// </summary>
        public PropertyInfo Info
        {
            get => m_info;
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
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the description of the <see cref="AdapterConnectionStringParameter"/>
        /// obtained through the <see cref="Info"/> using reflection. A property annotated with
        /// <see cref="ConnectionStringParameterAttribute"/>
        /// must also define a <see cref="DescriptionAttribute"/> for this
        /// to become populated.
        /// </summary>
        public string Description
        {
            get => m_description;
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
            get => m_value ?? m_defaultValue;
            set
            {
                m_value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("Color");
                OnPropertyChanged("Boldness");
                OnPropertyChanged("EnumDescription");
            }
        }

        /// <summary>
        /// Gets or sets the default value of the <see cref="AdapterConnectionStringParameter"/>
        /// obtained through the <see cref="Info"/> using reflection. A property annotated with
        /// <see cref="ConnectionStringParameterAttribute"/> must
        /// also define a <see cref="DefaultValueAttribute"/> for this to
        /// be populated.
        /// </summary>
        public object DefaultValue
        {
            get => m_defaultValue;
            set
            {
                m_defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this parameter is defined by a property
        /// that is annotated with the <see cref="DefaultValueAttribute"/>.
        /// </summary>
        public bool IsRequired
        {
            get => m_isRequired;
            set
            {
                m_isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        /// <summary>
        /// Gets a list of enum types if this <see cref="AdapterConnectionStringParameter"/>'s type is an enum.
        /// If it is not an enum, this returns an empty array.
        /// </summary>
        public string[] EnumValues
        {
            get => IsEnum ?
                Enum.GetNames(m_info.PropertyType) :
                Array.Empty<string>();
            set { }
        }

        /// <summary>
        /// Gets enum description, if defined.
        /// </summary>
        public string EnumDescription
        {
            get
            {
                try
                {
                    return IsEnum ? 
                        ((Enum)Enum.Parse(m_info.PropertyType, Value.ToString(), true)).GetDescription(false) :
                        null;
                }
                catch (Exception ex)
                {
                #if DEBUG
                    return ex.Message;
                #else
                    return null;
                #endif
                }
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the color of the item in the <see cref="ListBox"/>
        /// that the <see cref="AdapterConnectionStringParameter"/> is modeling.
        /// </summary>
        public Brush Color
        {
            get
            {
                bool red = m_isRequired && m_value is null;
                return red ? Brushes.Red : Brushes.Black;
            }
        }

        /// <summary>
        /// Gets or sets the boldness of the item in the <see cref="ListBox"/> that the
        /// <see cref="AdapterConnectionStringParameter"/> is modeling.
        /// </summary>
        public FontWeight Boldness => m_value is null ? FontWeights.Normal : FontWeights.Bold;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="RadioButton"/>
        /// labeled "False" is checked. Since the actual value of this <see cref="AdapterConnectionStringParameter"/>
        /// is represented by <see cref="Value"/>, and that value is what goes into the connection string
        /// this simply returns true if the value in the Value property is the word "false".
        /// </summary>
        public bool IsFalseChecked => Value.ToNonNullString().Equals(false.ToString(), StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets a value that indicates whether the value of this parameter can be configured via a
        /// custom control. This determines whether the hyper-link that links to the custom configuration
        /// pop-up is visible.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                try
                {
                    return !(m_info?.GetCustomAttribute<CustomConfigurationEditorAttribute>() is null);
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
        /// <see cref="RadioButton"/>s labeled "True" and "False" are visible.
        /// </summary>
        public bool IsBoolean => !(IsCustom || m_info is null || m_info.PropertyType != typeof(bool));

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is defined to be an enum in the adapter type. This determines whether the
        /// <see cref="ComboBox"/> bound to the enum values is visible.
        /// </summary>
        public bool IsEnum => !(IsCustom || m_info is null || !m_info.PropertyType.IsEnum);

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is something other than a <see cref="bool"/> or an enum. This determines whether the
        /// <see cref="TextBox"/> bound to the <see cref="Value"/> of the parameters is
        /// visible. The value is true for most parameters, including those which are not defined by an adapter type.
        /// </summary>
        public bool IsOther => !(IsCustom || IsBoolean || IsEnum);

        #endregion
    }
}
