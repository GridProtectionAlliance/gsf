//******************************************************************************************************
//  CategorizedSettingsElementCollection.cs - Gbtc
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
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/25/2006 - Pinal C. Patel
//       Modified the Item(name) property to add a configuration element if it does not exist.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/20/2010 - Pinal C. Patel
//       Added new Name and Section properties for the purpose of managing user scope setting.
//  04/21/2010 - Pinal C. Patel
//       Removed unnecessary overloads of Add() for manageability.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="CategorizedSettingsElement"/> objects.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class CategorizedSettingsElementCollection : ConfigurationElementCollection
    {
        #region [ Members ]

        // Fields
        private string m_cryptoKey;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="CategorizedSettingsElementCollection"/>.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="CategorizedSettingsSection"/> to which this <see cref="CategorizedSettingsElementCollection"/> belongs.
        /// </summary>
        public CategorizedSettingsSection Section { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="CategorizedSettingsElement"/> object at the specified index.
        /// </summary>
        /// <param name="index">Zero-based index for the <see cref="CategorizedSettingsElement"/> object to retrieve.</param>
        /// <returns>The <see cref="CategorizedSettingsElement"/> object at the specified index if it exists; otherwise null.</returns>
        public CategorizedSettingsElement this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();

                CategorizedSettingsElement setting = (CategorizedSettingsElement)BaseGet(index);
                setting.SetCryptoKey(m_cryptoKey);

                return setting;
            }
            set
            {
                if (BaseGet(index) is not null)
                    BaseRemoveAt(index);

                base.BaseAdd(index, value);
                Modified = true;
            }
        }

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsElement"/> object with the specified name.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object to retrieve.</param>
        /// <returns>The <see cref="CategorizedSettingsElement"/> object with the specified name if it exists; otherwise null.</returns>
        public new CategorizedSettingsElement this[string name] => this[name, false];

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsElement"/> object with the specified name.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object to retrieve.</param>
        /// <param name="ensureExistance">true if the <see cref="CategorizedSettingsElement"/> object is to be created if it does not exist; otherwise false.</param>
        /// <returns>The <see cref="CategorizedSettingsElement"/> object with the specified name if it exists; otherwise null.</returns>
        public CategorizedSettingsElement this[string name, bool ensureExistance]
        {
            get
            {
                // Add setting since it's not there
                if (ensureExistance && BaseGet(name) is null)
                    Add(name, string.Empty);

                CategorizedSettingsElement setting = (CategorizedSettingsElement)BaseGet(name);

                // Set the crypto key for the setting
                setting?.SetCryptoKey(m_cryptoKey);

                return setting;
            }
        }

        internal bool Modified
        {
            set
            {
                if (Section is not null)
                    Section.Modified = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting setting values.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey) => 
            m_cryptoKey = cryptoKey;
        
        /// <summary>
        /// Gets the index of the specified <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object whose index is to be retrieved.</param>
        /// <returns>Index of the specified <see cref="CategorizedSettingsElement"/> object if found; otherwise -1.</returns>
        public int IndexOf(CategorizedSettingsElement setting) => 
            BaseIndexOf(setting);

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, object value) => 
            Add(name, value, CategorizedSettingsElement.DefaultDescription, CategorizedSettingsElement.DefaultEncrypted, CategorizedSettingsElement.DefaultScope);

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, object value, string description) => 
            Add(name, value, description, CategorizedSettingsElement.DefaultEncrypted, CategorizedSettingsElement.DefaultScope);

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        public void Add(string name, object value, string description, bool encryptValue) => 
            Add(name, value, description, encryptValue, CategorizedSettingsElement.DefaultScope);

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        /// <param name="scope">One of the <see cref="SettingScope"/> values.</param>
        public void Add(string name, object value, string description, bool encryptValue, SettingScope scope)
        {
            if (BaseGet(name) is not null)
                return;

            // Add the element only if it does not exist.
            CategorizedSettingsElement setting = new(this, name);
            setting.Update(value, description, encryptValue, scope);

            Add(setting);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object to add.</param>
        public void Add(CategorizedSettingsElement setting)
        {
            if (BaseGet(setting.Name) is not null)
                return;

            // Add the element only if it does not exist.
            setting.Category = this;
            setting.SetCryptoKey(m_cryptoKey);
            
            base.BaseAdd(setting);
            Modified = true;
        }

        /// <summary>
        /// Removes a <see cref="CategorizedSettingsElement"/> object if it exists.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void Remove(string name)
        {
            BaseRemove(name);
            Modified = true;
        }

        /// <summary>
        /// Removes a <see cref="CategorizedSettingsElement"/> object if it exists.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void Remove(CategorizedSettingsElement setting)
        {
            if (BaseIndexOf(setting) < 0)
                return;

            Remove(setting.Name);
            Modified = true;
        }

        /// <summary>
        /// Removes the <see cref="CategorizedSettingsElement"/> object at the specified index location.
        /// </summary>
        /// <param name="index">Index location of the <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
            Modified = true;
        }

        /// <summary>
        /// Removes all existing <see cref="CategorizedSettingsElement"/> objects.
        /// </summary>
        public void Clear()
        {
            BaseClear();
            Modified = true;
        }

        /// <summary>
        /// Creates a new <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <returns>Instance of <see cref="CategorizedSettingsElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement() => 
            new CategorizedSettingsElement(this);

        /// <summary>
        /// Creates a new <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="elementName">Name identifying the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <returns>Instance of <see cref="CategorizedSettingsElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement(string elementName) => 
            new CategorizedSettingsElement(this, elementName);

        /// <summary>
        /// Gets the key for a <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="element"><see cref="CategorizedSettingsElement"/> object whose key is to be retrieved.</param>
        /// <returns>String key value for a <see cref="CategorizedSettingsElement"/> object.</returns>
        protected override object GetElementKey(ConfigurationElement element) => 
            ((CategorizedSettingsElement)element).Name;

        #endregion
    }
}
