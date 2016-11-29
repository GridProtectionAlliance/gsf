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
        private readonly string[] m_attributes;
        private readonly string[] m_values;
        private readonly long m_hashCode;

        #endregion

        #region [ Constructors ]

        private LogStackMessages()
        {
            m_attributes = EmptyArray<string>.Empty;
            m_values = EmptyArray<string>.Empty;
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

            int count = keyValuePairs.Length / 2;
            int index = 0;

            m_attributes = new string[count];
            m_values = new string[count];

            for (int i = 0; i < keyValuePairs.Length; i += 2)
            {
                string key = keyValuePairs[i];
                string value = keyValuePairs[i + 1];

                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key), "Specified key was null or empty.");

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), "Specified value was null or empty.");

                m_attributes[index] = key.Trim();
                m_values[index] = value.Trim();
                index++;
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

            m_attributes = new [] { key };
            m_values = new [] { value };
            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        /// <param name="messages">the messages</param>
        public LogStackMessages(List<KeyValuePair<string, string>> messages)
        {
            m_attributes = new string[messages.Count];
            m_values = new string[messages.Count];

            for (int x = 0; x < messages.Count; x++)
            {
                KeyValuePair<string, string> pair = messages[x];

                if (string.IsNullOrWhiteSpace(pair.Key))
                    throw new ArgumentNullException(nameof(messages), "A key is null or whitespace");

                if (string.IsNullOrWhiteSpace(pair.Value))
                    throw new ArgumentNullException(nameof(messages), "A value is null or whitespace");

                m_attributes[x] = pair.Key;
                m_values[x] = pair.Value;
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        /// <param name="messages">the messages</param>
        public LogStackMessages(List<LogStackMessages> messages)
        {
            int count = 0;

            for (int x = 0; x < messages.Count; x++)
                count += messages[x].m_attributes.Length;

            m_attributes = new string[count];
            m_values = new string[count];

            count = 0;

            for (int x = 0; x < messages.Count; x++)
            {
                LogStackMessages item = messages[x];
                for (int y = 0; y < item.m_attributes.Length; y++)
                {
                    m_attributes[count] = item.m_attributes[y];
                    m_values[count] = item.m_values[y];
                    count++;
                }
            }

            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Appends stack messages together.
        /// </summary>
        private LogStackMessages(LogStackMessages a, LogStackMessages b)
        {
            int count = a.m_attributes.Length + b.m_attributes.Length;

            m_attributes = new string[count];
            m_values = new string[count];

            count = 0;
            LogStackMessages item = a;

            for (int y = 0; y < item.m_attributes.Length; y++)
            {
                m_attributes[count] = item.m_attributes[y];
                m_values[count] = item.m_values[y];
                count++;
            }

            item = b;

            for (int y = 0; y < item.m_attributes.Length; y++)
            {
                m_attributes[count] = item.m_attributes[y];
                m_values[count] = item.m_values[y];
                count++;
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

            if (version == 0)
            {
                int count = stream.ReadInt32();

                m_attributes = new string[count];
                m_values = new string[count];

                for (int x = 0; x < count; x++)
                {
                    m_attributes[x] = stream.ReadString();
                    m_values[x] = stream.ReadString();
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
            int count = oldMessage.m_attributes.Length + 1;

            m_attributes = new string[count];
            m_values = new string[count];

            oldMessage.m_attributes.CopyTo(m_attributes, 0);
            oldMessage.m_values.CopyTo(m_values, 0);

            m_attributes[count - 1] = key;
            m_values[count - 1] = value;
            m_hashCode = ComputeHashCode();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the number of Key/Value pairs.
        /// </summary>
        public int Count => m_attributes.Length;

        /// <summary>
        /// Gets the KeyValue for the provided index.
        /// </summary>
        /// <param name="index">The Index</param>
        public KeyValuePair<string, string> this[int index] => new KeyValuePair<string, string>(m_attributes[index], m_values[index]);

        /// <summary>
        /// Gets the first match of the provided <see pref="key"/> in this dictionary. Returns
        /// null if none can be found.
        /// </summary>
        /// <param name="key">The Index</param>
        public string this[string key]
        {
            get
            {
                for (int x = 0; x < m_attributes.Length; x++)
                {
                    if (m_attributes[x] == key)
                        return m_values[x];
                }

                return null;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// returns the union of this instance and the specified key/value
        /// </summary>
        /// <param name="key">a key</param>
        /// <param name="value">a value</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if Key or Value are null or whitespace.</exception>
        public LogStackMessages ConcatenateWith(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            return new LogStackMessages(this, key, value);
        }

        /// <summary>
        /// returns the union of this instance and the specified <see pref="stackMessage"/>
        /// </summary>
        /// <returns></returns>
        public LogStackMessages ConcatenateWith(LogStackMessages stackMessage)
        {
            if ((object)stackMessage == null)
                return this;

            if (ReferenceEquals(stackMessage, Empty))
                return this;

            if (ReferenceEquals(this, Empty))
                return stackMessage;

            return new LogStackMessages(this, stackMessage);
        }

        /// <summary>
        /// Saves this instance to the provided stream
        /// </summary>
        /// <param name="stream">the stream to save.</param>
        public void Save(Stream stream)
        {
            stream.WriteByte(0);
            stream.Write(m_attributes.Length);

            for (int x = 0; x < m_attributes.Length; x++)
            {
                stream.Write(m_attributes[x]);
                stream.Write(m_values[x]);
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
                || m_attributes.Length != other.m_attributes.Length)
                return false;

            for (int x = 0; x < m_attributes.Length; x++)
            {
                if (m_attributes[x] != other.m_attributes[x])
                    return false;

                if (m_values[x] != other.m_values[x])
                    return false;
            }

            return true;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (m_attributes.Length == 0)
                return string.Empty;

            StringBuilder image = new StringBuilder();

            for (int x = 0; x < m_attributes.Length; x++)
            {
                image.Append(m_attributes[x]);
                image.Append('=');
                image.Append(m_values[x]);
                image.AppendLine();
            }

            return image.ToString();
        }

        private int ComputeHashCode()
        {
            int hashSum = m_attributes.Length;

            for (int x = 0; x < m_attributes.Length; x++)
            {
                hashSum ^= m_attributes[x].GetHashCode();
                hashSum ^= m_values[x].GetHashCode();
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
