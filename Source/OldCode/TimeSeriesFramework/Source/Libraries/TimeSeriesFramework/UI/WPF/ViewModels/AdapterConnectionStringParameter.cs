using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using TVA;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// View-model for a key-value pair in an adapter connection string.
    /// This can also represent key-value pairs which aren't necessarily
    /// in the connection string, but rather are defined by properties
    /// in an adapter class. This view-model is used by the
    /// <see cref="TimeSeriesFramework.UI.UserControls.AdapterUserControl"/>.
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
        private FontWeight m_boldness;
        private Brush m_color;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="PropertyInfo"/> of the
        /// <see cref="TimeSeriesFramework.Adapters.ConnectionStringParameterAttribute"/>
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
        /// <see cref="TimeSeriesFramework.Adapters.ConnectionStringParameterAttribute"/>
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
                object value = m_value ?? m_defaultValue;

                // Numeric type validation
                if (m_info != null && value != null)
                {
                    if (m_info.PropertyType.IsNumeric() && !Common.IsNumeric(value))
                        throw new ArgumentException("Not a number.");
                }

                return value;
            }
            set
            {
                // Numeric type validation
                if (m_info != null && value != null)
                {
                    if (m_info.PropertyType.IsNumeric() && !Common.IsNumeric(value))
                        throw new ArgumentException("Not a number.");
                }

                m_value = value;
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets or sets the default value of the <see cref="AdapterConnectionStringParameter"/>
        /// obtained through the <see cref="Info"/> using reflection. A property annotated with
        /// <see cref="TimeSeriesFramework.Adapters.ConnectionStringParameterAttribute"/> must
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
        /// Gets or sets the color of the item in the <see cref="System.Windows.Controls.ListBox"/>
        /// that the <see cref="AdapterConnectionStringParameter"/> is modeling.
        /// </summary>
        public Brush Color
        {
            get
            {
                return m_color;
            }
            set
            {
                m_color = value;
                OnPropertyChanged("Color");
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
                return m_boldness;
            }
            set
            {
                m_boldness = value;
                OnPropertyChanged("Boldness");
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
        /// Gets a value that indicates whether the type of this <see cref="AdapterConnectionStringParameter"/>
        /// is defined to be a <see cref="bool"/> in the adapter type. This determines whether the
        /// <see cref="System.Windows.Controls.RadioButton"/>s labeled "True" and "False" are visible.
        /// </summary>
        public bool IsBoolean
        {
            get
            {
                return (m_info == null) ? false : (m_info.PropertyType == typeof(bool));
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
                return (m_info == null) ? false : m_info.PropertyType.IsEnum;
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
                return !IsBoolean && !IsEnum;
            }
        }

        #endregion
    }
}
