//******************************************************************************************************
//  PublisherTypeDefinition.cs - Gbtc
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
//  11/29/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using GSF.Immutable;
using GSF.IO;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Contains all of the metadata associated with a <see cref="Type"/> that will 
    /// be serialized to the disk.
    /// </summary>
    public sealed class PublisherTypeDefinition
          : IEquatable<PublisherTypeDefinition>
    {
        /// <summary>
        /// The <see cref="Type"/> associated with <see cref="LogPublisher"/> that generated the message.
        /// </summary>
        public readonly string TypeName;

        /// <summary>
        /// The <see cref="Assembly"/> associated with <see cref="LogPublisher"/> that generated the message.
        /// </summary>
        public readonly string AssemblyName;

        /// <summary>
        /// Gets the version number of the <see cref="Assembly"/> that this <see cref="LogPublisher"/>'s type 
        /// belongs to.
        /// </summary>
        public readonly string AssemblyVersion;

        /// <summary>
        /// All related types such as interfaces/parent classes for the current type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly ImmutableList<string> RelatedTypes;

        /// <summary>
        /// A hashCode code of this owner.
        /// </summary>
        private readonly int m_hashCode;

        /// <summary>
        /// Creates a <see cref="PublisherTypeDefinition"/> by looking it up from <see pref="type"/>.
        /// </summary>
        /// <param name="type"></param>
        public PublisherTypeDefinition(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            TypeName = TrimAfterFullName(type.AssemblyQualifiedName);
            AssemblyName = Path.GetFileName(type.Assembly.Location);
            AssemblyVersion = type.Assembly.GetName().Version.ToString();
            RelatedTypes = new ImmutableList<string>();

            foreach (var interfaceType in type.GetInterfaces())
            {
                RelatedTypes.Add(TrimAfterFullName(interfaceType.AssemblyQualifiedName));
            }

            Type baseType = type.BaseType;
            while (baseType != null)
            {
                RelatedTypes.Add(TrimAfterFullName(baseType.AssemblyQualifiedName));
                baseType = baseType.BaseType;
            }

            RelatedTypes.IsReadOnly = true;
        }

        /// <summary>
        /// Loads a log messages from the supplied stream
        /// </summary>
        /// <param name="stream">the stream to load the log message from.</param>
        public PublisherTypeDefinition(Stream stream)
        {
            int count;
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    TypeName = stream.ReadString();
                    AssemblyName = stream.ReadString();
                    count = stream.ReadInt32();
                    RelatedTypes = new ImmutableList<string>(count);
                    while (count > 0)
                    {
                        RelatedTypes.Add(stream.ReadString());
                        count--;
                    }
                    AssemblyVersion = string.Empty;
                    break;
                case 2:
                    TypeName = stream.ReadString();
                    AssemblyName = stream.ReadString();
                    count = stream.ReadInt32();
                    RelatedTypes = new ImmutableList<string>(count);
                    while (count > 0)
                    {
                        RelatedTypes.Add(stream.ReadString());
                        count--;
                    }
                    AssemblyVersion = stream.ReadString();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
            m_hashCode = ComputeHashCode();
        }

        /// <summary>
        /// A backwards compatible means of generating this type data if <see cref="Type"/> is not available.
        /// </summary>
        internal PublisherTypeDefinition(string typeName, string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentNullException(nameof(typeName));
            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentNullException(nameof(assemblyName));

            TypeName = typeName;
            AssemblyName = assemblyName;
            AssemblyVersion = string.Empty;
            RelatedTypes = new ImmutableList<string>();
            RelatedTypes.IsReadOnly = true;
            m_hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            //Hashing TypeName and AssemblyName is good enough for a semi-unique hashcode
            return TypeName.GetHashCode() ^ AssemblyName.GetHashCode();
        }

        /// <summary>
        /// Writes the log data to the stream
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            stream.Write((byte)2);
            stream.Write(TypeName);
            stream.Write(AssemblyName);
            stream.Write(RelatedTypes.Count);
            foreach (var type in RelatedTypes)
            {
                stream.Write(type);
            }
            stream.Write(AssemblyVersion);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (TypeName.Length > 0)
            {
                sb.AppendLine("Message Type: " + TypeName);
            }
            if (AssemblyName.Length > 0)
            {
                sb.AppendLine("Message Assembly: " + AssemblyName);
            }
            sb.Length -= Environment.NewLine.Length;
            return sb.ToString();
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (int)m_hashCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as PublisherTypeDefinition);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="obj"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="obj">An object to compare with this object.</param>
        public bool Equals(PublisherTypeDefinition obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return m_hashCode == obj.m_hashCode &&
                   TypeName == obj.TypeName &&
                   AssemblyName == obj.AssemblyName &&
                   AssemblyVersion == obj.AssemblyVersion &&
                   AreEqual(RelatedTypes, obj.RelatedTypes);
        }

        private static bool AreEqual(ImmutableList<string> a, ImmutableList<string> b)
        {
            if (a.Count != b.Count)
                return false;
            for (int x = 0; x < a.Count; x++)
            {
                if (a[x] != b[x])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Trims the unused information after the namespace.class+subclass details.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string TrimAfterFullName(string name)
        {
            int newLength = name.Length;
            int indexOfBracket = name.IndexOf('[');
            int indexOfComma = name.IndexOf(',');

            if (indexOfBracket >= 0)
                newLength = Math.Min(indexOfBracket, newLength);
            if (indexOfComma >= 0)
                newLength = Math.Min(indexOfComma, newLength);
            name = name.Substring(0, newLength).Trim();
            return name;
        }


    }
}
