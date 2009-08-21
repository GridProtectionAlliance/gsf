//*******************************************************************************************************
//  Serialization.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;

namespace TVA.Historian.Services
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the format of <see cref="Object"/> serialization or deserialization.
    /// </summary>
    public enum SerializationFormat
    {
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="DataContractJsonSerializer"/> to JSON (JavaScript Object Notation) format.
        /// </summary>
        Json,
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="XmlSerializer"/> to ASMX (.NET Web Service) compatible XML (eXtensible Markup Language) format.
        /// </summary>
        PoxAsmx,
        /// <summary>
        /// <see cref="Object"/> is serialized or deserialized using <see cref="DataContractSerializer"/> to REST (Representational State Transfer) compatible XML (eXtensible Markup Language) format.
        /// </summary>
        PoxRest
    }

    #endregion

    /// <summary>
    /// Helper class to serialize and deserialize <see cref="Object"/>s to web service compatible <see cref="SerializationFormat"/>s.
    /// </summary>
    /// <seealso cref="SerializationFormat"/>
    public static class Serialization
    {
        #region [ Methods ]

        /// <summary>
        /// Serializes an <see cref="Object"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the <paramref name="serializableObject"/>.</typeparam>
        /// <param name="serializedOutput"><see cref="Stream"/> where the <paramref name="serializableObject"/> is to be serialized.</param>
        /// <param name="serializableObject"><see cref="Object"/> to be serialized.</param>
        /// <param name="serializationFormat"><see cref="SerializationFormat"/> in which the <paramref name="serializableObject"/> is to be serialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializedOutput"/> or <paramref name="serializableObject"/> is null.</exception>
        /// <exception cref="NotSupportedException">Specified <paramref name="serializationFormat"/> is not supported.</exception>
        public static void Serialize<T>(ref Stream serializedOutput, T serializableObject, SerializationFormat serializationFormat)
        {
            if (serializedOutput == null)
                throw new ArgumentNullException("serializedOutput");

            if (serializableObject == null)
                throw new ArgumentNullException("serializableObject");

            // Serialize object to the provided stream.
            if (serializationFormat == SerializationFormat.PoxAsmx)
            {
                // Serialize to ASMX XML format.
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(serializedOutput, serializableObject);
            }
            else if (serializationFormat == SerializationFormat.PoxRest)
            {
                // Serialize to REST XML format.
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(serializedOutput, serializableObject);
            }
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Serialize to JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(serializedOutput, serializableObject);
            }
            else
            {
                // Serialization format is not supported.
                throw new NotSupportedException(string.Format("{0} serialization is not supported.", serializationFormat));
            }

            // Seek to the beginning of the serialized output stream.
            serializedOutput.Position = 0;
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
            if (serializedObject == null)
                throw new ArgumentNullException("serializedObject");

            // Deserialize the serialized object.
            T deserializedObject;
            if (serializationFormat == SerializationFormat.PoxAsmx)
            {
                // Object was serialized to ASMX XML format.
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                deserializedObject = (T)serializer.Deserialize(serializedObject);
            }
            else if (serializationFormat == SerializationFormat.PoxRest)
            {
                // Object was serialized to REST XML format.
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                deserializedObject = (T)serializer.ReadObject(serializedObject);
            }
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Object was serialized to JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                deserializedObject = (T)serializer.ReadObject(serializedObject);
            }
            else
            {
                // Serialization format is not supported.
                throw new NotSupportedException(string.Format("{0} serialization is not supported.", serializationFormat));
            }

            return deserializedObject;
        }

        #endregion
    }
}
