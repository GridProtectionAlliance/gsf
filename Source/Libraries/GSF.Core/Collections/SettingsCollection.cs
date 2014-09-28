//******************************************************************************************************
//  SettingsCollection.cs - Gbtc
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
//  08/19/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       MOdified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Validation;

namespace GSF.Collections
{
    /// <summary>
    /// A collection of settings that can be represented as a string of key-value pairs for easy persistence and also provide built-in validation support for the settings.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="SettingsCollection"/> for key-value pair type settings and apply validation to them:
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using GSF.Collections;
    /// using GSF.Validation;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize settings.
    ///         SettingsCollection settings = "Server=localhost;Port=5000";
    ///         // Add validation rules.
    ///         settings.Validation.AddValidation("Server", new NonNullStringValidator());
    ///         settings.Validation.AddValidation("Port", new NonNullStringValidator());
    ///         settings.Validation.AddValidation("Port", new NumericRangeValidator(1000, 2000));
    /// 
    ///         // Validate settings.
    ///         string errors;
    ///         if (!settings.Validation.Validate(out errors))
    ///         {
    ///             // Show validation errors.
    ///             Console.WriteLine(string.Format("Settings: {0}\r\n", settings));
    ///             Console.WriteLine(errors);
    ///         }
    ///         else
    ///         {
    ///             // Show stored settings.
    ///             foreach (KeyValuePair&lt;string, string&gt; setting in settings)
    ///             {
    ///                 Console.WriteLine(string.Format("Key={0}; Value={1}", setting.Key, setting.Value));
    ///             }
    ///         }
    ///         
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ValidationService"/>
    [Serializable]
    public class SettingsCollection : Dictionary<string, string>
    {
        #region [ Members ]

        // Fields
        private ValidationService m_validationService;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        public SettingsCollection()
            : this(comparer: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="StringComparer"/> to use when comparing keys or null to use the default <see cref="StringComparer"/>.</param>
        public SettingsCollection(IEqualityComparer<string> comparer)
            : base(comparer)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> whose elements are to be copied to this <see cref="SettingsCollection"/>.</param>
        public SettingsCollection(IDictionary<string, string> dictionary)
            : this(dictionary, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> whose elements are to be copied to this <see cref="SettingsCollection"/>.</param>
        /// <param name="comparer">The <see cref="StringComparer"/> to use when comparing keys or null to use the default <see cref="StringComparer"/>.</param>
        public SettingsCollection(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer)
            : base(dictionary, comparer)
        {
            Initialize();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="ValidationService"/> object used to perform validation on the <see cref="Dictionary{TKey,TValue}.Values"/>.
        /// </summary>
        public ValidationService Validation
        {
            get
            {
                return m_validationService;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds an element with the specified <paramref name="key"/> and <paramref name="value"/> if an element is not present with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns><strong>true</strong> if an element is added; otherwise <strong>false</strong>.</returns>
        public bool TryAdd(string key, string value)
        {
            if (base.ContainsKey(key))
                return false;
            else
                base.Add(key, value);

            return true;
        }

        /// <summary>
        /// Gets the <see cref="string"/> representation of <see cref="SettingsCollection"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents <see cref="SettingsCollection"/>.</returns>
        public override string ToString()
        {
            return this.JoinKeyValuePairs();
        }

        private void Initialize()
        {
            m_validationService = new ValidationService(Lookup);
        }

        private object Lookup(string source)
        {
            string value;
            if (base.TryGetValue(source, out value))
                return value;
            else
                return string.Empty;
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts <see cref="SettingsCollection"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="SettingsCollection"/> to convert.</param>
        /// <returns>A <see cref="string"/> value representing the result.</returns>
        public static implicit operator string(SettingsCollection value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Implicitly converts <see cref="string"/> to <see cref="SettingsCollection"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to convert.</param>
        /// <returns>A <see cref="SettingsCollection"/> object representing the result.</returns>
        public static implicit operator SettingsCollection(string value)
        {
            return new SettingsCollection(value.ParseKeyValuePairs(), StringComparer.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}
