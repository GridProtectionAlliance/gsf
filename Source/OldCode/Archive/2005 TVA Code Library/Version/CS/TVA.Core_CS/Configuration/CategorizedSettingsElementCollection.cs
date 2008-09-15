using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Configuration;

//*******************************************************************************************************
//  TVA.Configuration.CategorizedSettingsCollection.vb - Categorized Settings Collection
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
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
//
//*******************************************************************************************************


namespace TVA
{
    namespace Configuration
    {

        /// <summary>
        /// Represents a configuration element containing a collection of
        /// TVA.Configuration.CategorizedSettingsElement within a configuration file.
        /// </summary>
        public class CategorizedSettingsElementCollection : ConfigurationElementCollection
        {


            /// <summary>
            /// Gets or sets the TVA.Configuration.CategorizedSettingsElement object at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the TVA.Configuration.CategorizedSettingsElement to
            /// return.</param>
            /// <returns>The TVA.Configuration.CategorizedSettingsElement at the specified index; otherwise null.</returns>
            public new CategorizedSettingsElement this[int index]
            {
                get
                {
                    if (index >= base.Count)
                    {
                        throw (new IndexOutOfRangeException());
                    }
                    return ((CategorizedSettingsElement)(base.BaseGet(index)));
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
            /// Gets the TVA.Configuration.CategorizedSettingsElement object with the specified name.
            /// </summary>
            /// <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to return.</param>
            /// <returns>The TVA.Configuration.CategorizedSettingsElement with the specified name; otherwise null.</returns>
            public new CategorizedSettingsElement this[string name]
            {
                get
                {
                    return Item[name, false];
                }
            }

            /// <summary>
            /// Gets the TVA.Configuration.CategorizedSettingsElement object with the specified name.
            /// </summary>
            /// <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to return.</param>
            /// <param name="ensureExistance">True, if the setting is to be created if it does not exist;
            /// otherwise, false.</param>
            /// <returns>The TVA.Configuration.CategorizedSettingsElement with the specified name; otherwise null.</returns>
            public new CategorizedSettingsElement this[string name, bool ensureExistance]
            {
                get
                {
                    if (ensureExistance && base.BaseGet(name) == null)
                    {
                        Add(name, "");
                    }

                    return ((CategorizedSettingsElement)(base.BaseGet(name)));
                }
            }

            /// <summary>
            /// Gets the index of the specified TVA.Configuration.CategorizedSettingsElement.
            /// </summary>
            /// <param name="setting">The TVA.Configuration.CategorizedSettingsElement whose index is to be returned.</param>
            /// <returns>The index of the specified TVA.Configuration.CategorizedSettingsElement; otherwise -1.</returns>
            public int IndexOf(CategorizedSettingsElement setting)
            {

                return base.BaseIndexOf(setting);

            }

            /// <summary>
            /// Adds a TVA.Configuration.CategorizedSettingsElement with the specified name and value string.
            /// </summary>
            /// <param name="name">The name string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            public void Add(string name, string value)
            {

                Add(name, value, false);

            }

            /// <summary>
            /// Adds a TVA.Configuration.CategorizedSettingsElement with the specified name and value string.
            /// </summary>
            /// <param name="name">The name string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            /// <param name="encryptValue">True, if the value string of the element is to be encrypted; otherwise,
            /// false.</param>
            public void Add(string name, string value, bool encryptValue)
            {

                Add(name, value, "", encryptValue);

            }

            /// <summary>
            /// Adds a TVA.Configuration.CategorizedSettingsElement with the specified name, value and description
            /// string.
            /// </summary>
            /// <param name="name">The name string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            /// <param name="description">The description string of the element.</param>
            public void Add(string name, string value, string description)
            {

                Add(new CategorizedSettingsElement(name, value, description, false));

            }

            /// <summary>
            /// Adds a TVA.Configuration.CategorizedSettingsElement with the specified name, value and description
            /// string.
            /// </summary>
            /// <param name="name">The name string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            /// <param name="description">The description string of the element.</param>
            /// <param name="encryptValue">True, if the value string of the element is to be encrypted; otherwise,
            /// false.</param>
            public void Add(string name, string value, string description, bool encryptValue)
            {

                Add(new CategorizedSettingsElement(name, value, description, encryptValue));

            }

            /// <summary>
            /// Adds the specified TVA.Configuration.CategorizedSettingsElement to the
            /// TVA.Configuration.CategorizedSettingsCollection.
            /// </summary>
            /// <param name="setting">The TVA.Configuration.CategorizedSettingsElement to add.</param>
            public void Add(CategorizedSettingsElement setting)
            {

                if (base.BaseGet(setting.Name) == null)
                {
                    // Adds the element only if it does not exist.
                    base.BaseAdd(setting);
                }

            }

            /// <summary>
            /// Removes a TVA.Configuration.CategorizedSettingsElement with the specified name from the
            /// TVA.Configuration.CategorizedSettingsCollection.
            /// </summary>
            /// <param name="name">The name of the TVA.Configuration.CategorizedSettingsElement to remove.</param>
            public void Remove(string name)
            {

                base.BaseRemove(name);

            }

            /// <summary>
            /// Removes the specified TVA.Configuration.CategorizedSettingsElement from the
            /// TVA.Configuration.CategorizedSettingsCollection.
            /// </summary>
            /// <param name="setting">The TVA.Configuration.CategorizedSettingsElement to remove.</param>
            public void Remove(CategorizedSettingsElement setting)
            {

                if (base.BaseIndexOf(setting) >= 0)
                {
                    Remove(setting.Name);
                }

            }

            /// <summary>
            /// Removes the TVA.Configuration.CategorizedSettingsElement at the specified location from the
            /// TVA.Configuration.CategorizedSettingsCollection.
            /// </summary>
            /// <param name="index">The index location of the TVA.Configuration.CategorizedSettingsElement to
            /// remove.</param>
            public void RemoveAt(int index)
            {

                base.BaseRemoveAt(index);

            }

            /// <summary>
            /// Removes all TVA.Configuration.CategorizedSettingsElement from the
            /// TVA.Configuration.CategorizedSettingsCollection.
            /// </summary>
            public void Clear()
            {

                base.BaseClear();

            }

            #region " Required ConfigurationElementCollection Overrides "

            protected override System.Configuration.ConfigurationElement CreateNewElement()
            {

                return new CategorizedSettingsElement();

            }

            protected override System.Configuration.ConfigurationElement CreateNewElement(string elementName)
            {

                return new CategorizedSettingsElement(elementName);

            }

            protected override object GetElementKey(System.Configuration.ConfigurationElement element)
            {

                return ((CategorizedSettingsElement)element).Name;

            }

            #endregion

        }

    }
}
