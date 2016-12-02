//******************************************************************************************************
//  LogStackMessages.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using GSF.IO;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents an immutable list of stack messages.
    /// </summary>
    public class LogStackMessages : IEquatable<LogStackMessages>
    {
        #region [ Members ]

        // Fields
        //Since stack messages won't be too large, SortedList consumes less memory and can be faster.
        private readonly SortedList<string, string> m_messages;
        private readonly long m_hashCode;

        #endregion

        #region [ Constructors ]

        private LogStackMessages()
        {
            m_messages = new SortedList<string, string>();
            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Creates a new stack message from the provided list of key/value pairs.
        /// </summary>
        /// <param name="keyValuePairs">Key/value pairs, e.g., key1, value1, key2, value2, ..., key(n), value(n).</param>
        public LogStackMessages(params string[] keyValuePairs)
        {
            if (keyValuePairs.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(keyValuePairs));

            if (keyValuePairs.Length % 2 != 0)
                throw new ArgumentOutOfRangeException(nameof(keyValuePairs), "Key/value pair mismatch");


            m_messages = new SortedList<string, string>();

            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                string key = keyValuePairs[i];
                string value = keyValuePairs[i + 1];

                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key), "Specified key was null or empty.");

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), "Specified value was null or empty.");

                m_messages[key] = value;
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Creates a new stack message from the provided <paramref name="key"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">if key or value are null or whitespace.</exception>
        public LogStackMessages(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "Specified key was null or empty.");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value), "Specified value was null or empty.");

            m_messages = new SortedList<string, string>();
            m_messages[key] = value;
            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        /// <param name="messages">the messages</param>
        public LogStackMessages(List<KeyValuePair<string, string>> messages)
        {
            m_messages = new SortedList<string, string>();

            for (int x = 0; x < messages.Count; x++)
            {
                KeyValuePair<string, string> pair = messages[x];

                if (string.IsNullOrWhiteSpace(pair.Key))
                    throw new ArgumentNullException(nameof(messages), "A key is null or whitespace");

                if (string.IsNullOrWhiteSpace(pair.Value))
                    throw new ArgumentNullException(nameof(messages), "A value is null or whitespace");

                m_messages[pair.Key] = pair.Value;
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        /// <param name="messages">the messages</param>
        public LogStackMessages(List<LogStackMessages> messages)
        {
            m_messages = new SortedList<string, string>();

            for (int x = 0; x < messages.Count; x++)
            {
                LogStackMessages item = messages[x];
                foreach (var kvp in item.m_messages)
                {
                    m_messages[kvp.Key] = kvp.Value;
                }
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        private LogStackMessages(LogStackMessages a, LogStackMessages b)
        {
            m_messages = new SortedList<string, string>();

            foreach (var kvp in a.m_messages)
            {
                m_messages[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in b.m_messages)
            {
                m_messages[kvp.Key] = kvp.Value;
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Loads stack messages from the stream.
        /// </summary>
        /// <param name="stream">the stream to load from</param>
        /// <exception cref="VersionNotFoundException">if the version is not recognized.</exception>
        public LogStackMessages(Stream stream)
        {
            int version = stream.ReadNextByte();
            m_messages = new SortedList<string, string>();

            if (version == 0)
            {
                int count = stream.ReadInt32();
                for (int x = 0; x < count; x++)
                {
                    string key = stream.ReadString();
                    string value = stream.ReadString();
                    m_messages[key] = value;
                }
            }
            else
            {
                throw new VersionNotFoundException();
            }

            m_hashCode = ComputeHashCode();
        }

        private LogStackMessages(LogStackMessages oldMessage, string key, string value)
        {
            m_messages = new SortedList<string, string>();

            foreach (var kvp in oldMessage.m_messages)
            {
                m_messages[kvp.Key] = kvp.Value;
            }
            m_messages[key] = value;
            m_hashCode = ComputeHashCode();

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of Key/Value pairs.
        /// </summary>
        public int Count => m_messages.Count;

        /// <summary>
        /// Gets the KeyValue for the provided index.
        /// </summary>
        /// <param name="index">The Index</param>
        public KeyValuePair<string, string> this[int index] => new KeyValuePair<string, string>(m_messages.Keys[index], m_messages.Values[index]);

        /// <summary>
        /// Gets the first match of the provided <see pref="key"/> in this dictionary. Returns
        /// null if none can be found.
        /// </summary>
        /// <param name="key">The Index</param>
        public string this[string key]
        {
            get
            {
                string value;
                m_messages.TryGetValue(key, out value);
                return value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// returns the union of this instance and the specified key/value. 
        /// If the key already exists. The new one replaces the existing one.
        /// </summary>
        /// <param name="key">a key</param>
        /// <param name="value">a value</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if Key or Value are null or whitespace.</exception>
        public LogStackMessages Union(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            string currentValue = this[key];
            if (currentValue != null && currentValue == value)
                return this;

            return new LogStackMessages(this, key, value);
        }

        /// <summary>
        /// returns the union of this instance and the specified list of key/value pairs. 
        /// If the keys already exists. The new one replaces the existing one.
        /// </summary>
        /// <param name="keyValuePairs">Key/value pairs, e.g., key1, value1, key2, value2, ..., key(n), value(n).</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if Key or Value are null or whitespace.</exception>
        public LogStackMessages Union(params string[] keyValuePairs)
        {
            return Union(new LogStackMessages(keyValuePairs));
        }

        /// <summary>
        /// returns the union of this instance and the specified <see pref="stackMessage"/>
        /// New messages replace existing messages.
        /// </summary>
        /// <returns></returns>
        public LogStackMessages Union(LogStackMessages stackMessage)
        {
            if ((object)stackMessage == null)
                return this;

            if (ReferenceEquals(stackMessage, Empty))
                return this;

            if (ReferenceEquals(this, Empty))
                return stackMessage;

            if (Equals(stackMessage))
                return this;

            return new LogStackMessages(this, stackMessage);
        }

        /// <summary>
        /// Saves this instance to the provided stream
        /// </summary>
        /// <param name="stream">the stream to save.</param>
        public void Save(Stream stream)
        {
            stream.WriteByte(0);
            stream.Write(m_messages.Count);

            foreach (var kvp in m_messages)
            {
                stream.Write(kvp.Key);
                stream.Write(kvp.Value);
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() => (int)m_hashCode;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj) => Equals(obj as LogStackMessages);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(LogStackMessages other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if ((object)other == null
                || m_hashCode != other.m_hashCode
                || m_messages.Count != other.m_messages.Count)
                return false;

            for (int x = 0; x < m_messages.Count; x++)
            {
                if (m_messages.Keys[x] != other.m_messages.Keys[x])
                    return false;

                if (m_messages.Values[x] != other.m_messages.Values[x])
                    return false;
            }

            return true;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (m_messages.Count == 0)
                return string.Empty;

            StringBuilder image = new StringBuilder();

            foreach (var kvp in m_messages)
            {
                image.Append(kvp.Key);
                image.Append('=');
                image.Append(kvp.Value);
                image.AppendLine();
            }

            return image.ToString();
        }

        private int ComputeHashCode()
        {
            int hashSum = m_messages.Count;

            foreach (var kvp in m_messages)
            {
                hashSum ^= kvp.Key.GetHashCode();
                hashSum ^= kvp.Value.GetHashCode();
            }

            return hashSum;
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// An empty stack message.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly LogStackMessages Empty;

        // Static Constructor
        static LogStackMessages()
        {
            Empty = new LogStackMessages();
        }

        // Static Methods

        internal static void Initialize()
        {
        }

        #endregion
    }
}
