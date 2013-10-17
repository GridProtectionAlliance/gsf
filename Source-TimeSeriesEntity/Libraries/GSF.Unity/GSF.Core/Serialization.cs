//******************************************************************************************************
//  Serialization.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************

using GSF.IO;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//using System.ServiceModel;
using System.Xml.Serialization;

#if !MONO
using System.Runtime.Serialization.Json;
#endif

namespace GSF
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the format of <see cref="Object"/> serialization or deserialization.
    /// </summary>
    public enum SerializationFormat
    {
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="DataContractSerializer"/> to XML (eXtensible Markup Language) format.
        /// </summary>
        /// <remarks>
        /// <see cref="Object"/> can be serialized or deserialized using <see cref="XmlSerializer"/> by adding the <see cref="XmlSerializerFormatAttribute"/> to the <see cref="Object"/>.
        /// </remarks>
        Xml,
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="DataContractJsonSerializer"/> to JSON (JavaScript Object Notation) format.
        /// </summary>
        Json,
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="BinaryFormatter"/> to binary format.
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
        /// <param name="serializedObject">A <see cref="string"/> representing the object (<paramref name="serializedObject"/>) to de-serialize.</param>
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
        /// <param name="deserializedObject">An object of type T that is passed in as the container to hold the de-serialized object from the string <paramref name="serializedObject"/>.</param>
        /// <returns><see cref="bool"/> value indicating if the de-serialization was successful.</returns>
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
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to de-serialize.</param>
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
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to de-serialize.</param>
        /// <param name="deserializedObject">A byref type T that is passed in to be hold the de-serialized object.</param>
        /// <typeparam name="T">The generic type T that is to be deserialized.</typeparam>
        /// <returns>A <see cref="bool"/> which indicates whether the de-serialization process was successful.</returns>
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
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to de-serialize.</param>
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
        /// <param name="serializedObject">A <see cref="byte"/> array representing the object (<paramref name="serializedObject"/>) to de-serialize.</param>
        /// <param name="deserializedObject">A byref <see cref="object"></see> that is passed in to be hold the de-serialized object.</param>
        /// <returns>A <see cref="bool"/> which indicates whether the de-serialization process was successful.</returns>
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
            Stream stream = null;
            try
            {
                stream = new MemoryStream();
                Serialize(serializableObject, serializationFormat, ref stream);

                return stream.ReadStream();
            }
            finally
            {
                if ((object)stream != null)
                    stream.Dispose();
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
        public static void Serialize<T>(T serializableObject, SerializationFormat serializationFormat, ref Stream serializedOutput)
        {
            if ((object)serializedOutput == null)
                throw new ArgumentNullException("serializedOutput");

            if ((object)serializableObject == null)
                throw new ArgumentNullException("serializableObject");

            // Serialize object to the provided stream.
//            if (serializationFormat == SerializationFormat.Xml)
//            {
//                if (typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0)
//                {
//                    // Serialize to XML format using XmlSerializer.
//                    XmlSerializer serializer = new XmlSerializer(typeof(T));
//                    serializer.Serialize(serializedOutput, serializableObject);
//                }
//                else
//                {
//                    // Serialize to XML format using DataContractSerializer.
//                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
//                    serializer.WriteObject(serializedOutput, serializableObject);
//                }
//            }
#if !MONO
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Serialize to JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(serializedOutput, serializableObject);
            }
#endif
            else if (serializationFormat == SerializationFormat.Binary)
            {
                // Serialize to binary format.
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(serializedOutput, serializableObject);
            }
            else
            {
                // Serialization format is not supported.
                throw new NotSupportedException(string.Format("{0} serialization is not supported", serializationFormat));
            }

            // Seek to the beginning of the serialized output stream.
            serializedOutput.Position = 0;
        }

        /// <summary>
        /// Deserializes a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Array"/> of <see cref="Byte"/>s contaning the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <returns>The deserialized <see cref="Object"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializedObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static T Deserialize<T>(byte[] serializedObject, SerializationFormat serializationFormat)
        {
            if ((object)serializedObject == null)
                throw new ArgumentNullException("serializedObject");

            Stream stream = null;
            try
            {
                stream = new MemoryStream(serializedObject);

                return Serialization.Deserialize<T>(stream, serializationFormat);
            }
            finally
            {
                if ((object)stream != null)
                    stream.Dispose();
            }
        }

        /// <summary>
        /// Deserializes a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Stream"/> contaning the serialized <see cref="Object"/> that is to be deserialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializedObject"/> was serialized.</param>
        /// <returns>The deserialized <see cref="Object"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializedObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static T Deserialize<T>(Stream serializedObject, SerializationFormat serializationFormat)
        {
            if ((object)serializedObject == null)
                throw new ArgumentNullException("serializedObject");

            // Deserialize the serialized object.
            T deserializedObject;
//            if (serializationFormat == SerializationFormat.Xml)
//            {
//                if (typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0)
//                {
//                    // Deserialize from XML format using XmlSerializer.
//                    XmlSerializer serializer = new XmlSerializer(typeof(T));
//                    deserializedObject = (T)serializer.Deserialize(serializedObject);
//                }
//                else
//                {
//                    // Deserialize from XML format using DataContractSerializer.
//                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
//                    deserializedObject = (T)serializer.ReadObject(serializedObject);
//                }
//            }
#if !MONO
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Deserialize from JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                deserializedObject = (T)serializer.ReadObject(serializedObject);
            }
#endif
            //else 
                if (serializationFormat == SerializationFormat.Binary)
            {
                // Deserialize from binary format.
                BinaryFormatter serializer = new BinaryFormatter();
                deserializedObject = (T)serializer.Deserialize(serializedObject);
            }
            else
            {
                // Serialization format is not supported.
                throw new NotSupportedException(string.Format("{0} serialization is not supported", serializationFormat));
            }

            return deserializedObject;
        }

        /// <summary>
        /// Attempts to deserialize a serialized <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the deserialized <see cref="Object"/> to be returned.</typeparam>
        /// <param name="serializedObject"><see cref="Array"/> of <see cref="Byte"/>s contaning the serialized <see cref="Object"/> that is to be deserialized.</param>
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
        /// <param name="serializedObject"><see cref="Stream"/> contaning the serialized <see cref="Object"/> that is to be deserialized.</param>
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
    }
}