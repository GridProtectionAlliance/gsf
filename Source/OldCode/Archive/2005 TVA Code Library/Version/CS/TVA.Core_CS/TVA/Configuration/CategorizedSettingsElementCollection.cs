//*******************************************************************************************************
//  CategorizedSettingsElementCollection.cs
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
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/25/2006 - Pinal C. Patel
//       Modified the Item(name) property to add a configuration element if it does not exist.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C Patel
//       Reviewed code comments.
//
//*******************************************************************************************************

using System;
using System.Configuration;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="CategorizedSettingsElement"/> objects.
    /// </summary>
    public class CategorizedSettingsElementCollection : ConfigurationElementCollection
    {
        #region [ Members ]

        // Fields
        private string m_cryptoKey;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="CategorizedSettingsElement"/> object at the specified index.
        /// </summary>
        /// <param name="index">Zero-based index for the <see cref="CategorizedSettingsElement"/> object to retrieve.</param>
        /// <returns>The <see cref="CategorizedSettingsElement"/> object at the specified index if it exists; otherwise null.</returns>
        public CategorizedSettingsElement this[int index]
        {
            get
            {
                if (index >= base.Count)
                {
                    throw (new IndexOutOfRangeException());
                }
                CategorizedSettingsElement setting = (CategorizedSettingsElement)base.BaseGet(index);
                setting.SetCryptoKey(m_cryptoKey);
                return setting;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsElement"/> object with the specified name.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object to retrieve.</param>
        /// <returns>The <see cref="CategorizedSettingsElement"/> object with the specified name if it exists; otherwise null.</returns>
        public new CategorizedSettingsElement this[string name]
        {
            get
            {
                return this[name, false];
            }
        }

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
                if (ensureExistance && base.BaseGet(name) == null)
                {
                    Add(name, string.Empty);
                }
                CategorizedSettingsElement setting = (CategorizedSettingsElement)base.BaseGet(name);
                setting.SetCryptoKey(m_cryptoKey);
                return setting;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting setting values.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey)
        {
            m_cryptoKey = cryptoKey;
        }

        /// <summary>
        /// Gets the index of the specified <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object whose index is to be retrieved.</param>
        /// <returns>Index of the specified <see cref="CategorizedSettingsElement"/> object if found; otherwise -1.</returns>
        public int IndexOf(CategorizedSettingsElement setting)
        {
            return base.BaseIndexOf(setting);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, object value)
        {
            Add(name, value.ToString());
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, string value)
        {
            Add(name, value, false);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        public void Add(string name, object value, bool encryptValue)
        {
            Add(name, value.ToString(), encryptValue);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        public void Add(string name, string value, bool encryptValue)
        {
            Add(name, value, "", encryptValue);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, object value, string description)
        {
            Add(name, value.ToString(), description);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        public void Add(string name, string value, string description)
        {
            Add(name, value, description, false);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        public void Add(string name, object value, string description, bool encryptValue)
        {
            Add(name, value.ToString(), description, encryptValue);
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="value">Value of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="description">Description of the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <param name="encryptValue">true if the Value of <see cref="CategorizedSettingsElement"/> object is to be encrypted; otherwise false.</param>
        public void Add(string name, string value, string description, bool encryptValue)
        {
            Add(new CategorizedSettingsElement(name, value, description, encryptValue));
        }

        /// <summary>
        /// Adds a new <see cref="CategorizedSettingsElement"/> object if one does not exist.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object to add.</param>
        public void Add(CategorizedSettingsElement setting)
        {
            if (base.BaseGet(setting.Name) == null)
            {
                // Adds the element only if it does not exist.
                setting.SetCryptoKey(m_cryptoKey);
                base.BaseAdd(setting);
            }
        }

        /// <summary>
        /// Removes a <see cref="CategorizedSettingsElement"/> object if it exists.
        /// </summary>
        /// <param name="name">Name of the <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        /// <summary>
        /// Removes a <see cref="CategorizedSettingsElement"/> object if it exists.
        /// </summary>
        /// <param name="setting">The <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void Remove(CategorizedSettingsElement setting)
        {
            if (base.BaseIndexOf(setting) >= 0)
            {
                Remove(setting.Name);
            }
        }

        /// <summary>
        /// Removes the <see cref="CategorizedSettingsElement"/> object at the specified index location.
        /// </summary>
        /// <param name="index">Index location of the <see cref="CategorizedSettingsElement"/> object to remove.</param>
        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes all existing <see cref="CategorizedSettingsElement"/> objects.
        /// </summary>
        public void Clear()
        {
            base.BaseClear();
        }

        /// <summary>
        /// Creates a new <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <returns>Instance of <see cref="CategorizedSettingsElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CategorizedSettingsElement();
        }

        /// <summary>
        /// Creates a new <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="elementName">Name identifying the <see cref="CategorizedSettingsElement"/> object.</param>
        /// <returns>Instance of <see cref="CategorizedSettingsElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new CategorizedSettingsElement(elementName);
        }

        /// <summary>
        /// Gets the key for a <see cref="CategorizedSettingsElement"/> object.
        /// </summary>
        /// <param name="element"><see cref="CategorizedSettingsElement"/> object whose key is to be retrieved.</param>
        /// <returns>String key value for a <see cref="CategorizedSettingsElement"/> object.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CategorizedSettingsElement)element).Name;
        }

        #endregion      
    }
}
