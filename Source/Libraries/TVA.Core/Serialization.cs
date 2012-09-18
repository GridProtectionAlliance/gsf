//*******************************************************************************************************
//  Serialization.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The GSF Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using GSF.IO;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Xml.Serialization;

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
            if (serializationFormat == SerializationFormat.Xml)
            {
                if (typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0)
                {
                    // Serialize to XML format using XmlSerializer.
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(serializedOutput, serializableObject);
                }
                else
                {
                    // Serialize to XML format using DataContractSerializer.
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(serializedOutput, serializableObject);
                }
            }
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Serialize to JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(serializedOutput, serializableObject);
            }
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
            if (serializationFormat == SerializationFormat.Xml)
            {
                if (typeof(T).GetCustomAttributes(typeof(XmlSerializerFormatAttribute), false).Length > 0)
                {
                    // Deserialize from XML format using XmlSerializer.
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    deserializedObject = (T)serializer.Deserialize(serializedObject);
                }
                else
                {
                    // Deserialize from XML format using DataContractSerializer.
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    deserializedObject = (T)serializer.ReadObject(serializedObject);
                }
            }
            else if (serializationFormat == SerializationFormat.Json)
            {
                // Deserialize from JSON format.
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                deserializedObject = (T)serializer.ReadObject(serializedObject);
            }
            else if (serializationFormat == SerializationFormat.Binary)
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