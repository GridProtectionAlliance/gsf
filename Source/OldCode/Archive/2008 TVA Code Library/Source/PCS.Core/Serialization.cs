//*******************************************************************************************************
//  Serialization.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/08/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/09/2008 - J. Ritchie Carroll
//      Converted to C#
//  09/09/2008 - J. Ritchie Carroll
//      Added TryGetObject overloads
//
//*******************************************************************************************************

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace PCS
{
    /// <summary>
    /// Common serialization related functions.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// Creates a clone of a serializable object.
        /// </summary>
        public static T CloneObject<T>(T sourceObject)
        {
            return GetObject<T>(GetBytes(sourceObject));
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
        public static T GetObject<T>(string serializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(serializedObject));
        }

        /// <summary>
        /// Attempts XML deserialization on the XML string and returns the typed object for it.
        /// </summary>
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
        public static T GetObject<T>(byte[] serializedObject)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return (T)serializer.Deserialize(new MemoryStream(serializedObject));
        }

        /// <summary>
        /// Attempts binary deserialization on the byte array and returns the typed object for it.
        /// </summary>
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
        public static object GetObject(byte[] serializedObject)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return serializer.Deserialize(new MemoryStream(serializedObject));
        }

        /// <summary>
        /// Attempts binary deserialization on the byte array and returns the typed object for it.
        /// </summary>
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
        public static string GetString(object serializableObject)
        {
            StringWriter serializedObject = new StringWriter();

            if (serializableObject.GetType().IsSerializable)
            {
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                serializer.Serialize(serializedObject, serializableObject);
            }

            return serializedObject.ToString();
        }

        /// <summary>
        /// Performs binary serialization on the serializable object and returns the output as byte array.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>A byte array representation of the object if the specified object can be serialized; otherwise an empty array.</returns>
        public static byte[] GetBytes(object serializableObject)
        {
            return GetStream(serializableObject).ToArray();
        }

        /// <summary>
        /// Performs binary serialization on the serializable object and returns the serialized object as a stream.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>A memory stream representation of the object if the specified object can be serialized; otherwise an empty stream.</returns>
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
    }
}