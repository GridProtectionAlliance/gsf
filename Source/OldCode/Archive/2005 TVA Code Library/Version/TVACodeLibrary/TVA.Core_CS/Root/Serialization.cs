using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

//*******************************************************************************************************
//  TVA.Serialization.vb - Common serialization related functions
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
//  06/08/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************



namespace TVA
{
	public sealed class Serialization
	{
		
		
		private Serialization()
		{
			
			// This class contains only global functions and is not meant to be instantiated
			
		}
		
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
		public static T GetObject<T>(string serializedObject)
		{
			
			object deserializedObject = null;
			try
			{
				XmlSerializer deserializer = new XmlSerializer(typeof(T));
				deserializedObject = deserializer.Deserialize(new StringReader(serializedObject));
			}
			catch (Exception)
			{
				
			}
			return ((T) deserializedObject);
			
		}
		
		/// <summary>
		/// Performs binary deserialization on the byte array and returns the typed object for it.
		/// </summary>
		public static T GetObject<T>(byte[] serializedObject)
		{
			
			object deserializedObject = GetObject(serializedObject);
			if (deserializedObject is T) // Cannot use TryCast because of the templated parameter restriction.
			{
				return ((T) (GetObject(serializedObject)));
			}
			else
			{
				return null;
			}
			
		}
		
		/// <summary>
		/// Performs binary deserialization on the byte array and returns the object for it.
		/// </summary>
		public static object GetObject(byte[] serializedObject)
		{
			
			object deserializedObject = null;
			try
			{
				BinaryFormatter deserializer = new BinaryFormatter();
				deserializedObject = deserializer.Deserialize(GetStream(serializedObject));
			}
			catch (Exception)
			{
				
			}
			return deserializedObject;
			
		}
		
		/// <summary>
		/// Performs XML serialization on the serializable object and returns the output as string.
		/// </summary>
		public static string GetString(object serializableObject)
		{
			
			StringWriter serializedObject = new StringWriter();
			if (serializableObject.GetType().IsSerializable)
			{
				// The specified object if marked as serializable.
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				serializer.Serialize(serializedObject, serializableObject);
			}
			return serializedObject.ToString();
			
		}
		
		/// <summary>
		/// Performs binary serialization on the serializable object and returns the output as byte array.
		/// </summary>
		public static byte[] GetBytes(object serializableObject)
		{
			
			return GetStream(serializableObject).ToArray();
			
		}
		
		/// <summary>
		/// Gets a System.IO.MemoryStream from the bytes of a previously serialized object.
		/// </summary>
		/// <param name="serializedObject">The bytes of a previously serialized object.</param>
		/// <returns>A System.IO.MemoryStream from the bytes of a previously serialized object.</returns>
		public static MemoryStream GetStream(byte[] serializedObject)
		{
			
			return new MemoryStream(serializedObject);
			
		}
		
		/// <summary>
		/// Gets a System.IO.MemoryStream from a serializable object.
		/// </summary>
		/// <param name="serializableObject">The serializable object.</param>
		/// <returns>A System.IO.MemoryStream if the specified object can be serialized; otherwise an empty stream.</returns>
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
