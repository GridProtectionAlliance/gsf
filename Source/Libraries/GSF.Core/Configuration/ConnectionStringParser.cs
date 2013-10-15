//******************************************************************************************************
//  ConnectionStringParser.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  10/14/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GSF.Reflection;

namespace GSF.Configuration
{
    /// <summary>
    /// Parses connection strings based on a settings object whose properties
    /// are annotated with the <see cref="SerializeSettingAttribute"/>.
    /// </summary>
    public class ConnectionStringParser
    {
        #region [ Members ]

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
        /// Default value for the <see cref="SerializeUnspecifiedProperties"/> property.
        /// </summary>
        public const bool DefaultSerializeUnspecifiedProperties = true;

        // Fields
        private char m_parameterDelimiter;
        private char m_keyValueDelimiter;
        private char m_startValueDelimiter;
        private char m_endValueDelimiter;
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
        /// Gets or sets the flag that determines whether to include
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
            PropertyInfo[] connectionStringProperties;
            Dictionary<string, string> settings;

            // Null objects don't have properties
            if (settingsObject == null)
                return string.Empty;

            // Get the set of properties which are part of the connection string
            connectionStringProperties = GetConnectionStringProperties(settingsObject.GetType());

            // Create a dictionary of key-value pairs which
            // can easily be converted to a connection string
            settings = connectionStringProperties
                .Select(property => Tuple.Create(GetSettingName(property), property.GetValue(settingsObject)))
                .Where(tuple => tuple.Item2 != null)
                .ToDictionary(tuple => tuple.Item1, tuple => Common.TypeConvertToString(tuple.Item2), StringComparer.CurrentCultureIgnoreCase);

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
            PropertyInfo[] connectionStringProperties;
            DefaultValueAttribute defaultValueAttribute;
            Dictionary<string, string> settings;
            string key;
            string value;

            // Null objects don't have properties
            if (settingsObject == null)
                throw new ArgumentNullException("settingsObject", "Unable to parse connection string because settings object is invalid.");

            // Get the set of properties which are part of the connection string
            connectionStringProperties = GetConnectionStringProperties(settingsObject.GetType());

            // If there are no properties, then our work is done
            if (connectionStringProperties.Length <= 0)
                return;

            // Parse the connection string into a dictionary of key-value pairs for easy lookups
            settings = connectionString.ParseKeyValuePairs(m_parameterDelimiter, m_keyValueDelimiter, m_startValueDelimiter, m_endValueDelimiter);

            foreach (PropertyInfo property in connectionStringProperties)
            {
                // Connection string parameters may not match property names
                key = GetSettingName(property);

                // If the value exists in the connection string, set the property value to that;
                // If it does not exist in the connection string, set the property to its default value;
                // If it does not have a default value, it is a required parameter and an exception must be thrown
                if (settings.TryGetValue(key, out value))
                    property.SetValue(settingsObject, value.ConvertToType<object>(property.PropertyType));
                else if (property.TryGetAttribute(out defaultValueAttribute))
                    property.SetValue(settingsObject, defaultValueAttribute.Value);
                else
                    throw new ArgumentException("Unable to parse required connection string parameter because it does not exist in the connection string.", key);
            }
        }

        /// <summary>
        /// Gets the set of properties which are part of the connection string.
        /// </summary>
        /// <param name="settingsObjectType">The type of the settings object used to look up properties via reflection.</param>
        /// <returns>The set of properties which are part of the connection string.</returns>
        protected virtual PropertyInfo[] GetConnectionStringProperties(Type settingsObjectType)
        {
            return m_serializeUnspecifiedProperties
                ? s_allPropertiesLookup.GetOrAdd(settingsObjectType, s_allPropertiesFactory)
                : s_explicitPropertiesLookup.GetOrAdd(settingsObjectType, s_explicitPropertiesFactory);
        }

        /// <summary>
        /// Gets the name of the connection string setting for the given property.
        /// </summary>
        /// <param name="property">The property whose setting name is to be looked up.</param>
        /// <returns>The setting name of the given property.</returns>
        protected virtual string GetSettingName(PropertyInfo property)
        {
            SettingNameAttribute settingNameAttribute;
            return property.TryGetAttribute(out settingNameAttribute) ? settingNameAttribute.Name : property.Name;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static ConcurrentDictionary<Type, PropertyInfo[]> s_allPropertiesLookup = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static ConcurrentDictionary<Type, PropertyInfo[]> s_explicitPropertiesLookup = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static Func<Type, PropertyInfo[]> s_allPropertiesFactory = t =>
        {
            SerializeSettingAttribute attribute;

            return t.GetProperties()
                .Where(property => !property.TryGetAttribute(out attribute) || attribute.Serialize)
                .ToArray();
        };

        private static Func<Type, PropertyInfo[]> s_explicitPropertiesFactory = t =>
        {
            SerializeSettingAttribute attribute;

            return t.GetProperties()
                .Where(property => property.TryGetAttribute(out attribute) && attribute.Serialize)
                .ToArray();
        };

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
        protected override PropertyInfo[] GetConnectionStringProperties(Type settingsObjectType)
        {
            return s_connectionStringPropertiesLookup.GetOrAdd(settingsObjectType, s_valueFactory);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static ConcurrentDictionary<Type, PropertyInfo[]> s_connectionStringPropertiesLookup = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static Func<Type, PropertyInfo[]> s_valueFactory = t =>
        {
            TParameterAttribute attribute;

            return t.GetProperties()
                .Where(property => property.TryGetAttribute(out attribute))
                .ToArray();
        };

        #endregion
    }
}
