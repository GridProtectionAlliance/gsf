//******************************************************************************************************
//  Serialization.cs - Gbtc
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
//  06/08/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/09/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/09/2008 - J. Ritchie Carroll
//       Added TryGetObject overloads.
//  02/16/2009 - Josh L. Patterson
//       Edited Code Comments.
//  08/4/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/06/2011 - Pinal C. Patel
//       Modified GetString() method to not check for the presence of Serializable attribute on the 
//       object being serialized since this is not required by the XmlSerializer.
//  04/08/2011 - Pinal C. Patel
//       Moved Serialize() and Deserialize() methods from GSF.Services.ServiceModel.Serialization class
//       in GSF.Services.dll to consolidate serialization methods.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Xml.Serialization;
using GSF.IO;
using GSF.Reflection;

namespace GSF
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the format of <see cref="object"/> serialization or deserialization.
    /// </summary>
    public enum SerializationFormat
    {
        /// <summary>
        /// <see cref="object"/> is serialized or deserialized using <see cref="DataContractSerializer"/> to XML (eXtensible Markup Language) format.
        /// </summary>
        /// <remarks>
        /// <see cref="object"/> can be serialized or deserialized using <see cref="XmlSerializer"/> by adding the <see cref="XmlSerializerFormatAttribute"/> to the <see cref="object"/>.
        /// </remarks>
        Xml,
        /// <summary>
        /// <see cref="object"/> is serialized or deserialized using <see cref="DataContractJsonSerializer"/> to JSON (JavaScript Object Notation) format.
        /// </summary>
        Json,
        /// <summary>
        /// <see cref="object"/> is serialized or deserialized using <see cref="BinaryFormatter"/> to binary format.
        /// </summary>
        Binary
    }

    #endregion

    /// <summary>
    /// Common serialization related functions.
    /// </summary>
    public static class Serialization
    {
        #region [ Obsolete ]

        /// <summary>
        /// Creates a clone of a serializable object.
        /// </summary>
        /// <typeparam name="T">The type of the cloned object.</typeparam>
        /// <param name="sourceObject">The type source to be cloned.</param>
        /// <returns>A clone of <paramref name="sourceObject"/>.</returns>
        [Obsolete("This method will be removed in future builds.")]
        public static T CloneObject<T>(T sourceObject)
        {
            return Deserialize<T>(Serialize(sourceObject, SerializationFormat.Binary), SerializationFormat.Binary);
        }

        /// <summary>
        /// Performs XML deserialization on the XML string and returns the typed object for it.
        /// </summary>
        /// <remarks>
        /// This function will throw an error during deserialization if the input data is invalid,
        /// consider using TryGetObject to prevent needing to implement exception handling.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// An error occurred during deserialization. The original exception is available using 
        /// the InnerException property.
        /// </exception>
        /// <typeparam name="T">The type of the object to create from the serialized string <paramref name="serializedObject"/>.</typeparam>
        /// <param name="serializedObject">A <see cref="string"/> representing the object (<paramref name="serializedObject"/>) to deserialize.</param>
        /// <returns>A type T based on <paramref name="serializedObject"/>.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static T GetObject<T>(string serializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(serializedObject));
        }

        /// <summary>
        /// Attempts XML deserialization on the XML string and returns the typed object for it.
        /// </summary>
        /// <typeparam name="T">The generic type T that is to be deserialized.</typeparam>
        /// <param name="serializedObject"><see cref="string"/> that contains the serialized representation of the object.</param>
        /// <param name="deserializedObject">An object of type T that is passed in as the container to hold the deserialized object from the string <paramref name="serializedObject"/>.</param>
        /// <returns><see cref="bool"/> value indicating if the deserialization was successful.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static bool TryGetObject<T>(string serializedObject, out T deserializedObject)
        {
            try
            {
                deserializedObject = GetObject<T>(serializedObject);
                return true;
            }
            catch
            {
                deserializedObject = default(T);
                return false;
            }
        }

        /// <summary>
        /// Performs binary deserialization on the byte array and returns the typed object for it.
        /// </summary>
        /// <remarks>
        /// This function will throw an error during deserialization if the input data is invalid,
        /// consider using TryGetObject to prevent needing to implement exception handling.
        /// </remarks>
        /// <exception cref="System.Runtime.Serialization.SerializationException">Serialized object data is invalid or length is 0.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <typeparam name="T">The type of the object to create from the serialized byte array <paramref name="serializedObject"/>.</typeparam>
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to deserialize.</param>
        /// <returns>A type T based on <paramref name="serializedObject"/>.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static T GetObject<T>(byte[] serializedObject)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return (T)serializer.Deserialize(new MemoryStream(serializedObject));
        }

        /// <summary>
        /// Attempts binary deserialization on the byte array and returns the typed object for it.
        /// </summary>
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to deserialize.</param>
        /// <param name="deserializedObject">A type T object, passed by reference, that is used to be hold the deserialized object.</param>
        /// <typeparam name="T">The generic type T that is to be deserialized.</typeparam>
        /// <returns>A <see cref="bool"/> which indicates whether the deserialization process was successful.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static bool TryGetObject<T>(byte[] serializedObject, out T deserializedObject)
        {
            try
            {
                deserializedObject = GetObject<T>(serializedObject);
                return true;
            }
            catch
            {
                deserializedObject = default(T);
                return false;
            }
        }

        /// <summary>
        /// Performs binary deserialization on the byte array and returns the object for it.
        /// </summary>
        /// <remarks>
        /// This function will throw an error during deserialization if the input data is invalid,
        /// consider using TryGetObject to prevent needing to implement exception handling.
        /// </remarks>
        /// <exception cref="System.Runtime.Serialization.SerializationException">Serialized object data is invalid or length is 0.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to deserialize.</param>
        /// <returns>An <see cref="object"/> based on <paramref name="serializedObject"/>.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static object GetObject(byte[] serializedObject)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return serializer.Deserialize(new MemoryStream(serializedObject));
        }

        /// <summary>
        /// Attempts binary deserialization on the byte array and returns the typed object for it.
        /// </summary>
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to deserialize.</param>
        /// <param name="deserializedObject">An <see cref="object"/>, passed by reference, that is used to be hold the deserialized object.</param>
        /// <returns>A <see cref="bool"/> which indicates whether the deserialization process was successful.</returns>
        [Obsolete("This method will be removed in future builds, use the Deserialize() method instead.")]
        public static bool TryGetObject(byte[] serializedObject, out object deserializedObject)
        {
            try
            {
                deserializedObject = GetObject(serializedObject);
                return true;
            }
            catch
            {
                deserializedObject = null;
                return false;
            }
        }

        /// <summary>
        /// Performs XML serialization on the serializable object and returns the output as string.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>An XML representation of the object if the specified object can be serialized; otherwise an empty string.</returns>
        [Obsolete("This method will be removed in future builds, use the Serialize() method instead.")]
        public static string GetString(object serializableObject)
        {
            StringWriter serializedObject = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            serializer.Serialize(serializedObject, serializableObject);

            return serializedObject.ToString();
        }

        /// <summary>
        /// Performs binary serialization on the serializable object and returns the output as byte array.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>A byte array representation of the object if the specified object can be serialized; otherwise an empty array.</returns>
        [Obsolete("This method will be removed in future builds, use the Serialize() method instead.")]
        public static byte[] GetBytes(object serializableObject)
        {
            return GetStream(serializableObject).ToArray();
        }

        /// <summary>
        /// Performs binary serialization on the serializable object and returns the serialized object as a stream.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>A memory stream representation of the object if the specified object can be serialized; otherwise an empty stream.</returns>
        [Obsolete("This method will be removed in future builds, use the Serialize() method instead.")]
        public static MemoryStream GetStream(object serializableObject)
        {
            MemoryStream dataStream = new MemoryStream();

            if (serializableObject.GetType().IsSerializable)
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(dataStream, serializableObject);
            }

            return dataStream;
        }

        #endregion

        #region [ Legacy ]

        /// <summary>
        /// Serialization binder used to deserialize files that were serialized using the old library frameworks
        /// (TVA Code Library, Time Series Framework, TVA.PhasorProtocols, and PMU Connection Tester) into classes
        /// in the Grid Solutions Framework.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly SerializationBinder LegacyBinder = new LegacySerializationBinder();

        // Serialization binder used to deserialize files that were serialized using the old library frameworks.
        private class LegacySerializationBinder : SerializationBinder
        {
            /// <summary>
            /// Controls the binding of a serialized object to a type.
            /// </summary>
            /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
            /// <param name="typeName">Specifies the <see cref="Type"/> name of the serialized object.</param>
            /// <returns>The type of the object the formatter creates a new instance of.</returns>
            public override Type BindToType(string assemblyName, string typeName)
            {
                // Perform namespace transformations that occurred when migrating to the Grid Solutions Framework
                // from various older versions of code with different namespaces
                string newTypeName = typeName
                    .Replace("TVA.", "GSF.")
                    .Replace("TimeSeriesFramework.", "GSF.TimeSeries.")
                    .Replace("ConnectionTester.", "GSF.PhasorProtocols.")   // PMU Connection Tester namespace
                    .Replace("TVA.Phasors.", "GSF.PhasorProtocols.")        // 2007 TVA Code Library namespace
                    .Replace("Tva.Phasors.", "GSF.PhasorProtocols.")        // 2008 TVA Code Library namespace
                    .Replace("BpaPdcStream", "BPAPDCstream")                // 2013 GSF uppercase phasor protocol namespace
                    .Replace("FNet", "FNET")                                // 2013 GSF uppercase phasor protocol namespace
                    .Replace("Iec61850_90_5", "IEC61850_90_5")              // 2013 GSF uppercase phasor protocol namespace
                    .Replace("Ieee1344", "IEEE1344")                        // 2013 GSF uppercase phasor protocol namespace
                    .Replace("IeeeC37_118", "IEEEC37_118");                 // 2013 GSF uppercase phasor protocol namespace

                // Check for 2009 TVA Code Library namespace
                if (newTypeName.StartsWith("PhasorProtocols", StringComparison.Ordinal))
                    newTypeName = "GSF." + newTypeName;

                // Check for 2014 LineFrequency type in the GSF phasor protocol namespace
                if (newTypeName.Equals("GSF.PhasorProtocols.LineFrequency", StringComparison.Ordinal))
                    newTypeName = "GSF.Units.EE.LineFrequency";

                try
                {
                    // Search each assembly in the current application domain for the type with the transformed name
                    return AssemblyInfo.FindType(newTypeName);
                }
                catch
                {
                    // Fall back on more brute force search when simple search fails
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            Type newType = assembly.GetType(newTypeName);

                            if ((object)newType != null)
                                return newType;
                        }
                        catch
                        {
                            // Ignore errors that occur when attempting to load
                            // types from assemblies as we may still be able to
                            // load the type from a different assembly
                        }
                    }
                }

                // No type found; return null
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Serializes an <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the <paramref name="serializableObject"/>.</typeparam>
        /// <param name="serializableObject"><see cref="Object"/> to be serialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializableObject"/> is to be serialized.</param>
        /// <returns>An <see cref="Array"/> of <see cref="Byte"/> of the serialized <see cref="Object"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializableObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static byte[] Serialize<T>(T serializableObject, SerializationFormat serializationFormat)
        {
            // FYI, using statement will not work here as this creates a read-only variable that cannot be passed by reference
            Stream stream = null;

            try
            {
                stream = new BlockAllocatedMemoryStream();
                Serialize(serializableObject, serializationFormat, stream);
                return ((BlockAllocatedMemoryStream)stream).ToArray();
            }
            finally
            {
                stream?.Dispose();
            }
        }

        /// <summary>
        /// Serializes an <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the <paramref name="serializableObject"/>.</typeparam>
        /// <param name="serializableObject"><see cref="Object"/> to be serialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializableObject"/> is to be serialized.</param>
        /// <param name="serializedOutput"><see cref="Stream"/> where the <paramref name="serializableObject"/> is to be serialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializedOutput"/> or <paramref name="serializableObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static void Serialize<T>(T serializableObject, SerializationFormat serializationFormat, Stream serializedOutput)
        {
            if (serializedOutput == null)
                throw new ArgumentNullException(nameof(serializedOutput));

            if (serializableObject == null)
                throw new ArgumentNullException(nameof(serializableObject));

            switch (serializationFormat)
            {
                // Serialize object to the provided stream.
                case SerializationFormat.Xml when typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0:
                {
                    // Serialize to XML format using XmlSerializer.
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(serializedOutput, serializableObject);
                    break;
                }
                case SerializationFormat.Xml:
                {
                    // Serialize to XML format using DataContractSerializer.
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(serializedOutput, serializableObject);
                    break;
                }
                case SerializationFormat.Json:
                {
                    // Serialize to JSON format.
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    serializer.WriteObject(serializedOutput, serializableObject);
                    break;
                }
                case SerializationFormat.Binary:
                {
                    // Serialize to binary format.
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(serializedOutput, serializableObject);
                    break;
                }
                default:
                    // Serialization format is not supported.
                    throw new NotSupportedException($"{serializationFormat} serialization is not supported");
            }

            // Seek to the beginning of the serialized output stream.
            serializedOutput.Position = 0;
        }

        /// <summary>
        /// Deserializes a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Array"/> of <see cref="Byte"/>s containing the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <returns>The deserialized <see cref="Object"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializedObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static T Deserialize<T>(byte[] serializedObject, SerializationFormat serializationFormat)
        {
            if (serializedObject == null)
                throw new ArgumentNullException(nameof(serializedObject));

            using MemoryStream stream = new MemoryStream(serializedObject);
            
            return Deserialize<T>(stream, serializationFormat);
        }

        /// <summary>
        /// Deserializes a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Stream"/> containing the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <returns>The deserialized <see cref="Object"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializedObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static T Deserialize<T>(Stream serializedObject, SerializationFormat serializationFormat)
        {
            if (serializedObject == null)
                throw new ArgumentNullException(nameof(serializedObject));

            // Deserialize the serialized object.
            T deserializedObject;
            
            switch (serializationFormat)
            {
                case SerializationFormat.Xml when typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0:
                {
                    // Deserialize from XML format using XmlSerializer.
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    deserializedObject = (T)serializer.Deserialize(serializedObject);
                    break;
                }
                case SerializationFormat.Xml:
                {
                    // Deserialize from XML format using DataContractSerializer.
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    deserializedObject = (T)serializer.ReadObject(serializedObject);
                    break;
                }
                case SerializationFormat.Json:
                {
                    // Deserialize from JSON format.
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    deserializedObject = (T)serializer.ReadObject(serializedObject);
                    break;
                }
                case SerializationFormat.Binary:
                {
                    // Deserialize from binary format.
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Binder = LegacyBinder;
                    deserializedObject = (T)serializer.Deserialize(serializedObject);
                    break;
                }
                default:
                    // Serialization format is not supported.
                    throw new NotSupportedException($"{serializationFormat} serialization is not supported");
            }

            return deserializedObject;
        }

        /// <summary>
        /// Attempts to deserialize a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Array"/> of <see cref="Byte"/>s containing the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <param name="deserializedObject">Deserialized <see cref="Object"/>.</param>
        /// <returns><c>true</c>if deserialization succeeded; otherwise <c>false</c>.</returns>
        public static bool TryDeserialize<T>(byte[] serializedObject, SerializationFormat serializationFormat, out T deserializedObject)
        {
            deserializedObject = default(T);

            try
            {
                deserializedObject = Deserialize<T>(serializedObject, serializationFormat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to deserialize a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Stream"/> containing the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <param name="deserializedObject">Deserialized <see cref="Object"/>.</param>
        /// <returns><c>true</c>if deserialization succeeded; otherwise <c>false</c>.</returns>
        public static bool TryDeserialize<T>(Stream serializedObject, SerializationFormat serializationFormat, out T deserializedObject)
        {
            deserializedObject = default(T);

            try
            {
                deserializedObject = Deserialize<T>(serializedObject, serializationFormat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets <see cref="SerializationInfo"/> value for specified <paramref name="name"/>; otherwise <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="T">Type of parameter to get from <see cref="SerializationInfo"/>.</typeparam>
        /// <param name="info"><see cref="SerializationInfo"/> object that contains deserialized values.</param>
        /// <param name="name">Name of deserialized parameter to retrieve.</param>
        /// <param name="defaultValue">Default value to return if <paramref name="name"/> does not exist or cannot be deserialized.</param>
        /// <returns>Value for specified <paramref name="name"/>; otherwise <paramref name="defaultValue"/></returns>
        /// <remarks>
        /// <see cref="SerializationInfo"/> do not have a direct way of determining if an item with a specified name exists, so when calling
        /// one of the Get(n) functions you will simply get a <see cref="SerializationException"/> if the parameter does not exist; similarly
        /// you will receive this exception if the parameter fails to properly deserialize. This extension method protects against both of
        /// these failures and returns a default value if the named parameter does not exist or cannot be deserialized.
        /// </remarks>
        public static T GetOrDefault<T>(this SerializationInfo info, string name, T defaultValue)
        {
            try
            {
                return (T)info.GetValue(name, typeof(T));
            }
            catch (SerializationException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets values of a <see cref="SerializationInfo"/> instance compatible with Linq operations.
        /// </summary>
        /// <param name="info">Target <see cref="SerializationInfo"/> instance.</param>
        /// <returns>Enumeration of <see cref="SerializationEntry"/> objects from <paramref name="info"/> instance.</returns>
        public static IEnumerable<SerializationEntry> GetValues(this SerializationInfo info)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            {
                while (enumerator.MoveNext())
                    yield return (SerializationEntry)enumerator.Current;
            }
        }
    }
}