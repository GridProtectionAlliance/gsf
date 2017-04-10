//******************************************************************************************************
//  ConnectionStringParser.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  10/14/2013 - Stephen C. Wills
//       Generated original version of source code.
//  03/10/2017 - J. Ritchie Carroll
//       Added checks for validation attributes.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using ExpressionEvaluator;
using GSF.ComponentModel;
using GSF.Reflection;

// ReSharper disable StaticMemberInGenericType
namespace GSF.Configuration
{
    /// <summary>
    /// Parses connection strings based on a settings object whose properties
    /// are annotated with the <see cref="SerializeSettingAttribute"/>.
    /// </summary>
    public class ConnectionStringParser
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Stores reflected information from a <see cref="PropertyInfo"/>
        /// object used to parse connection strings.
        /// </summary>
        protected class ConnectionStringProperty
        {
            /// <summary>
            /// The <see cref="PropertyInfo"/> object.
            /// </summary>
            public PropertyInfo PropertyInfo;

            /// <summary>
            /// The type converter used to convert the value
            /// of this property to and from a string.
            /// </summary>
            public TypeConverter Converter;

            /// <summary>
            /// The name of the property as it appears in the connection string.
            /// </summary>
            public string[] Names;

            /// <summary>
            /// The default value of the property if its value
            /// is not explicitly specified in the connection string.
            /// </summary>
            public object DefaultValue;

            /// <summary>
            /// Indicates whether or not the property is required
            /// to be explicitly defined in the connection string.
            /// </summary>
            public bool Required;

            /// <summary>
            /// Gets all validation attributes that may be applied to the property.
            /// </summary>
            public ValidationAttribute[] ValidationAttributes;

            /// <summary>
            /// Creates a new instance of the <see cref="ConnectionStringProperty"/> class.
            /// </summary>
            /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object.</param>
            /// <param name="typeRegistry">
            /// Type registry to use when parsing <see cref="DefaultValueExpressionAttribute"/>
            /// instances. Set to <c>null</c> for default registry.
            /// </param>
            public ConnectionStringProperty(PropertyInfo propertyInfo, TypeRegistry typeRegistry = null)
            {
                SettingNameAttribute settingNameAttribute;
                DefaultValueAttribute defaultValueAttribute;
                DefaultValueExpressionAttribute defaultValueExpressionAttribute;
                TypeConverterAttribute typeConverterAttribute;
                Type converterType;

                PropertyInfo = propertyInfo;
                Names = propertyInfo.TryGetAttribute(out settingNameAttribute) ? settingNameAttribute.Names : new[] { propertyInfo.Name };

                bool hasDefaultValue = propertyInfo.TryGetAttribute(out defaultValueAttribute);
                bool hasDefaultValueExpression = propertyInfo.TryGetAttribute(out defaultValueExpressionAttribute);

                Required = !hasDefaultValue && !hasDefaultValueExpression;

                if (Required)
                {
                    DefaultValue = null;
                }
                else
                {
                    if ((object)defaultValueAttribute == null)
                    {
                        DefaultValueExpressionParser parser = new DefaultValueExpressionParser(defaultValueExpressionAttribute.Expression);                        

                        if ((object)typeRegistry != null)
                            parser.TypeRegistry = typeRegistry;

                        DefaultValue = parser.Eval();
                    }
                    else
                    {
                        DefaultValue = defaultValueAttribute.Value;
                    }
                }

                if (propertyInfo.TryGetAttribute(out typeConverterAttribute))
                {
                    converterType = Type.GetType(typeConverterAttribute.ConverterTypeName);

                    if ((object)converterType != null)
                        Converter = (TypeConverter)Activator.CreateInstance(converterType);
                }

                propertyInfo.TryGetAttributes(out ValidationAttributes);
            }
        }

        // Constants

        /// <summary>
        /// Default value for the <see cref="ParameterDelimiter"/> property.
        /// </summary>
        public const char DefaultParameterDelimiter = ';';

        /// <summary>
        /// Default value for the <see cref="KeyValueDelimiter"/> property.
        /// </summary>
        public const char DefaultKeyValueDelimiter = '=';

        /// <summary>
        /// Default value for the <see cref="StartValueDelimiter"/> property.
        /// </summary>
        public const char DefaultStartValueDelimiter = '{';

        /// <summary>
        /// Default value for the <see cref="EndValueDelimiter"/> property.
        /// </summary>
        public const char DefaultEndValueDelimiter = '}';

        /// <summary>
        /// Default value for the <see cref="ExplicitlySpecifyDefaults"/> property.
        /// </summary>
        public const bool DefaultExplicitlySpecifyDefaults = false;

        /// <summary>
        /// Default value for the <see cref="SerializeUnspecifiedProperties"/> property.
        /// </summary>
        public const bool DefaultSerializeUnspecifiedProperties = true;

        // Fields
        private char m_parameterDelimiter;
        private char m_keyValueDelimiter;
        private char m_startValueDelimiter;
        private char m_endValueDelimiter;
        private bool m_explicitlySpecifyDefaults;
        private bool m_serializeUnspecifiedProperties;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ConnectionStringParser"/> class.
        /// </summary>
        public ConnectionStringParser()
        {
            m_parameterDelimiter = DefaultParameterDelimiter;
            m_keyValueDelimiter = DefaultKeyValueDelimiter;
            m_startValueDelimiter = DefaultStartValueDelimiter;
            m_endValueDelimiter = DefaultEndValueDelimiter;
            m_serializeUnspecifiedProperties = DefaultSerializeUnspecifiedProperties;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the parameter delimiter used to
        /// separate key-value pairs in the connection string.
        /// </summary>
        public char ParameterDelimiter
        {
            get
            {
                return m_parameterDelimiter;
            }
            set
            {
                m_parameterDelimiter = value;
            }
        }

        /// <summary>
        /// Gets or sets the key-value delimiter used to
        /// separate keys from values in the connection string.
        /// </summary>
        public char KeyValueDelimiter
        {
            get
            {
                return m_keyValueDelimiter;
            }
            set
            {
                m_keyValueDelimiter = value;
            }
        }

        /// <summary>
        /// Gets or sets the start value delimiter used to denote the
        /// start of a value in the cases where the value contains one
        /// of the delimiters defined for the connection string.
        /// </summary>
        public char StartValueDelimiter
        {
            get
            {
                return m_startValueDelimiter;
            }
            set
            {
                m_startValueDelimiter = value;
            }
        }

        /// <summary>
        /// Gets or sets the end value delimiter used to denote the
        /// end of a value in the cases where the value contains one
        /// of the delimiters defined for the connection string.
        /// </summary>
        public char EndValueDelimiter
        {
            get
            {
                return m_endValueDelimiter;
            }
            set
            {
                m_endValueDelimiter = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to explicitly
        /// specify parameter values that match their defaults when
        /// serializing settings to a connection string.
        /// </summary>
        public bool ExplicitlySpecifyDefaults
        {
            get
            {
                return m_explicitlySpecifyDefaults;
            }
            set
            {
                m_explicitlySpecifyDefaults = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to include properties which are not
        /// annotated with the <see cref="SerializeSettingAttribute"/> in the connection string.
        /// </summary>
        public bool SerializeUnspecifiedProperties
        {
            get
            {
                return m_serializeUnspecifiedProperties;
            }
            set
            {
                m_serializeUnspecifiedProperties = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Serializes the given <paramref name="settingsObject"/> into a connection string.
        /// </summary>
        /// <param name="settingsObject">The object whose properties are to be serialized.</param>
        /// <returns>A connection string containing the serialized properties.</returns>
        public virtual string ComposeConnectionString(object settingsObject)
        {
            ConnectionStringProperty[] connectionStringProperties;
            Dictionary<string, string> settings;

            // Null objects don't have properties
            if (settingsObject == null)
                return string.Empty;

            // Get the set of properties which are part of the connection string
            connectionStringProperties = GetConnectionStringProperties(settingsObject.GetType());

            // Create a dictionary of key-value pairs which
            // can easily be converted to a connection string
            settings = connectionStringProperties
                .Select(property => Tuple.Create(property, property.PropertyInfo.GetValue(settingsObject)))
                .Where(tuple => tuple.Item2 != null && (m_explicitlySpecifyDefaults || !tuple.Item2.Equals(tuple.Item1.DefaultValue)))
                .ToDictionary(tuple => tuple.Item1.Names.First(), tuple => ConvertToString(tuple.Item2, tuple.Item1), StringComparer.CurrentCultureIgnoreCase);

            // Convert the dictionary to a connection string and return the result
            return settings.JoinKeyValuePairs(m_parameterDelimiter, m_keyValueDelimiter, m_startValueDelimiter, m_endValueDelimiter);
        }

        /// <summary>
        /// Deserializes the connection string parameters into the given <paramref name="settingsObject"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to be parsed.</param>
        /// <param name="settingsObject">The object whose properties are to be populated with values from the connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settingsObject"/> is null.</exception>
        /// <exception cref="ArgumentException">A required connection string parameter cannot be found in the connection string.</exception>
        public virtual void ParseConnectionString(string connectionString, object settingsObject)
        {
            ConnectionStringProperty[] connectionStringProperties;
            Dictionary<string, string> settings;
            string key;
            string value;

            // Null objects don't have properties
            if (settingsObject == null)
                throw new ArgumentNullException(nameof(settingsObject), "Unable to parse connection string because settings object is invalid.");

            // Get the set of properties which are part of the connection string
            connectionStringProperties = GetConnectionStringProperties(settingsObject.GetType());

            // If there are no properties, then our work is done
            if (connectionStringProperties.Length <= 0)
                return;

            // Parse the connection string into a dictionary of key-value pairs for easy lookups
            settings = connectionString.ParseKeyValuePairs(m_parameterDelimiter, m_keyValueDelimiter, m_startValueDelimiter, m_endValueDelimiter);

            foreach (ConnectionStringProperty property in connectionStringProperties)
            {
                value = string.Empty;
                key = property.Names.FirstOrDefault(name => settings.TryGetValue(name, out value));

                if ((object)key != null)
                    property.PropertyInfo.SetValue(settingsObject, ConvertToPropertyType(value, property));
                else if (!property.Required)
                    property.PropertyInfo.SetValue(settingsObject, property.DefaultValue);
                else
                    throw new ArgumentException("Unable to parse required connection string parameter because it does not exist in the connection string.", property.Names.First());

                if ((object)property.ValidationAttributes == null)
                    continue;

                object propertyValue = property.PropertyInfo.GetValue(settingsObject);
                string propertyName = key ?? property.Names.First();

                foreach (ValidationAttribute attr in property.ValidationAttributes)
                    attr.Validate(propertyValue, propertyName);
            }
        }

        /// <summary>
        /// Gets the set of properties which are part of the connection string.
        /// </summary>
        /// <param name="settingsObjectType">The type of the settings object used to look up properties via reflection.</param>
        /// <returns>The set of properties which are part of the connection string.</returns>
        protected virtual ConnectionStringProperty[] GetConnectionStringProperties(Type settingsObjectType)
        {
            return m_serializeUnspecifiedProperties
                ? s_allPropertiesLookup.GetOrAdd(settingsObjectType, s_allPropertiesFactory)
                : s_explicitPropertiesLookup.GetOrAdd(settingsObjectType, s_explicitPropertiesFactory);
        }

        /// <summary>
        /// Converts the given string value to the type of the given property.
        /// </summary>
        /// <param name="value">The string value to be converted.</param>
        /// <param name="property">The property used to determine what type to convert to.</param>
        /// <returns>The given string converted to the type of the given property.</returns>
        protected virtual object ConvertToPropertyType(string value, ConnectionStringProperty property)
        {
            return ((object)property.Converter != null)
                ? property.Converter.ConvertFromString(value)
                : value.ConvertToType<object>(property.PropertyInfo.PropertyType);
        }

        /// <summary>
        /// Converts the given object to a string.
        /// </summary>
        /// <param name="obj">The object to be converted.</param>
        /// <param name="property">The property which defines the type of the object.</param>
        /// <returns>The object converted to a string.</returns>
        protected virtual string ConvertToString(object obj, ConnectionStringProperty property)
        {
            return ((object)property.Converter != null)
                ? property.Converter.ConvertToString(obj)
                : Common.TypeConvertToString(obj);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Type, ConnectionStringProperty[]> s_allPropertiesLookup = new ConcurrentDictionary<Type, ConnectionStringProperty[]>();
        private static readonly ConcurrentDictionary<Type, ConnectionStringProperty[]> s_explicitPropertiesLookup = new ConcurrentDictionary<Type, ConnectionStringProperty[]>();

        private static readonly Func<Type, ConnectionStringProperty[]> s_allPropertiesFactory = t =>
        {
            SerializeSettingAttribute attribute;

            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .Where(property => !property.TryGetAttribute(out attribute) || attribute.Serialize)
                .Select(property => new ConnectionStringProperty(property))
                .ToArray();
        };

        private static readonly Func<Type, ConnectionStringProperty[]> s_explicitPropertiesFactory = t =>
        {
            SerializeSettingAttribute attribute;

            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .Where(property => property.TryGetAttribute(out attribute) && attribute.Serialize)
                .Select(property => new ConnectionStringProperty(property))
                .ToArray();
        };

        // Static Methods

        /// <summary>
        /// Converts XML to a connection string.
        /// </summary>
        /// <param name="element">The root element of the XML to be converted to a connection string.</param>
        /// <returns>A connection string converted from XML.</returns>
        public static string ToConnectionString(XElement element)
        {
            char[] reserved = { ';', '=', '{', '}' };

            string name = element.Name.ToString();

            string value = element.HasElements
                ? string.Join("; ", element.Elements().Select(ToConnectionString))
                : (string)element;

            return value.Any(reserved.Contains)
                ? $"{name}={{{value}}}"
                : $"{name}={value}";
        }

        /// <summary>
        /// Converts a connection string to XML.
        /// </summary>
        /// <param name="connectionString">The connection string to be converted to XML.</param>
        /// <exception cref="InvalidOperationException">The connection string does not define exactly one root element.</exception>
        /// <returns>The XML root element converted from the connection string.</returns>
        public static XElement ToXML(string connectionString)
        {
            XElement root;
            Dictionary<string, string> settings;

            settings = connectionString.ParseKeyValuePairs();

            if (settings.Count != 1)
                throw new InvalidOperationException($"Connection string does not define exactly one root element: {connectionString}");

            root = new XElement(settings.Keys.First());
            SetXMLContent(root, settings.Values.First());

            return root;
        }

        private static void SetXMLContent(XElement parent, string connectionStringValue)
        {
            Dictionary<string, string> settings;
            XElement element;

            settings = connectionStringValue.ParseKeyValuePairs();

            if (!settings.Any())
            {
                parent.Value = connectionStringValue;
            }
            else
            {
                foreach (KeyValuePair<string, string> setting in settings)
                {
                    element = new XElement(setting.Key);
                    SetXMLContent(element, setting.Value);
                    parent.Add(element);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Parses connection strings based on a settings object whose properties
    /// are annotated with <typeparamref name="TParameterAttribute"/>.
    /// </summary>
    /// <typeparam name="TParameterAttribute">
    /// The type of the attribute to search for when determining whether
    /// to serialize a property to the connection string.
    /// </typeparam>
    public class ConnectionStringParser<TParameterAttribute> : ConnectionStringParser where TParameterAttribute : Attribute
    {
        #region [ Properties ]

        /// <summary>
        /// Redefined to throw an exception. This property has no meaning when
        /// property serialization is determined by the existence of the typed parameter.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool SerializeUnspecifiedProperties
        {
            get
            {
                return false;
            }
            set
            {
                throw new InvalidOperationException("Not implemented");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the set of properties which are part of the connection string.
        /// </summary>
        /// <param name="settingsObjectType">The type of the settings object used to look up properties via reflection.</param>
        /// <returns>The set of properties which are part of the connection string.</returns>
        protected override ConnectionStringProperty[] GetConnectionStringProperties(Type settingsObjectType)
        {
            return s_connectionStringPropertiesLookup.GetOrAdd(settingsObjectType, s_valueFactory);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Type, ConnectionStringProperty[]> s_connectionStringPropertiesLookup = new ConcurrentDictionary<Type, ConnectionStringProperty[]>();
        private static TypeRegistry s_typeRegistry;

        private static readonly Func<Type, ConnectionStringProperty[]> s_valueFactory = t =>
        {
            TParameterAttribute attribute;

            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .Where(property => property.TryGetAttribute(out attribute))
                .Select(property => new ConnectionStringProperty(property, s_typeRegistry))
                .ToArray();
        };

        // Static Properties

        /// <summary>
        /// Gets <see cref="ExpressionEvaluator.TypeRegistry"/> instance used for evaluating encountered
        /// instances of the <see cref="DefaultValueExpressionAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Accessing this property will create a unique type registry for the current attribute type
        /// <typeparamref name="TParameterAttribute"/> which will initially contain the values found in
        /// the <see cref="DefaultValueExpressionParser.DefaultTypeRegistry"/> and can be augmented with
        /// custom types. Set to <c>null</c> to restore use of the default type registry.
        /// </remarks>
        public static TypeRegistry TypeRegistry
        {
            get
            {
                if ((object)s_typeRegistry == null)
                {
                    s_typeRegistry = new TypeRegistry();

                    foreach (KeyValuePair<string, object> item in DefaultValueExpressionParser.DefaultTypeRegistry)
                        s_typeRegistry.Add(item.Key, item.Value);
                }

                return s_typeRegistry;
            }
            set
            {
                s_typeRegistry = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Parses connection strings based on a settings object whose properties are annotated with
    /// <typeparamref name="TParameterAttribute"/> and <typeparamref name="TNestedSettingsAttribute"/>.
    /// </summary>
    /// <typeparam name="TParameterAttribute">
    /// The type of the attribute to search for when determining whether
    /// to serialize a property to the connection string.
    /// </typeparam>
    /// <typeparam name="TNestedSettingsAttribute">
    /// The type of the attribute to search for when determining which
    /// parameters are to be parsed recursively as connection strings.
    /// </typeparam>
    public class ConnectionStringParser<TParameterAttribute, TNestedSettingsAttribute> : ConnectionStringParser<TParameterAttribute>
        where TParameterAttribute : Attribute
        where TNestedSettingsAttribute : Attribute
    {
        /// <summary>
        /// Serializes the given <paramref name="settingsObject"/> into a connection string.
        /// </summary>
        /// <param name="settingsObject">The object whose properties are to be serialized.</param>
        /// <returns>A connection string containing the serialized properties.</returns>
        public override string ComposeConnectionString(object settingsObject)
        {
            StringBuilder builder = new StringBuilder();
            object nestedSettingsObject;

            if (settingsObject == null)
                return string.Empty;

            builder.Append(base.ComposeConnectionString(settingsObject));

            foreach (PropertyInfo property in GetNestedSettingsProperties(settingsObject))
            {
                nestedSettingsObject = property.GetValue(settingsObject);

                if (nestedSettingsObject != null)
                    builder.Append($"; {GetNames(property).First()}={{ {ComposeConnectionString(nestedSettingsObject)} }}");
            }

            return builder.ToString().Trim(';', ' ');
        }

        /// <summary>
        /// Deserializes the connection string parameters into the given <paramref name="settingsObject"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to be parsed.</param>
        /// <param name="settingsObject">The object whose properties are to be populated with values from the connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settingsObject"/> is null.</exception>
        /// <exception cref="ArgumentException">A required connection string parameter cannot be found in the connection string.</exception>
        public override void ParseConnectionString(string connectionString, object settingsObject)
        {
            Dictionary<string, string> settings;
            object nestedSettingsObject;
            string nestedSettings;

            base.ParseConnectionString(connectionString, settingsObject);
            settings = connectionString.ParseKeyValuePairs();

            foreach (PropertyInfo property in GetNestedSettingsProperties(settingsObject))
            {
                nestedSettingsObject = property.GetValue(settingsObject);
                nestedSettings = string.Empty;

                if (nestedSettingsObject != null)
                {
                    nestedSettings = GetNames(property)
                        .Where(name => settings.TryGetValue(name, out nestedSettings))
                        .Select(name => nestedSettings)
                        .DefaultIfEmpty(string.Empty)
                        .First();

                    ParseConnectionString(nestedSettings, nestedSettingsObject);
                }
            }
        }

        // Gets a collection of properties from the settings object which represent the nested connection strings
        private static PropertyInfo[] GetNestedSettingsProperties(object settingsObject)
        {
            TNestedSettingsAttribute nestedSettingsAttribute;

            return settingsObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.TryGetAttribute(out nestedSettingsAttribute))
                .ToArray();
        }

        // Gets a collection of names for the given property which can
        // be used during parsing or composing of connection strings
        private static string[] GetNames(PropertyInfo property)
        {
            SettingNameAttribute settingNameAttribute;

            if (property.TryGetAttribute(out settingNameAttribute))
                return settingNameAttribute.Names;

            return new[] { property.Name };
        }
    }
}
