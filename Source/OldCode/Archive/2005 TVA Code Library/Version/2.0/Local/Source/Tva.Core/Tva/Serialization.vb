'*******************************************************************************************************
'  Tva.Serialization.vb - Common serialization related functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/08/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Public NotInheritable Class Serialization

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Creates a clone of a serializable object.
    ''' </summary>
    ''' <typeparam name="T">Return type of the object.</typeparam>
    ''' <param name="sourceObject">The source object that is to be cloned.</param>
    ''' <returns>A clone of the source serializable object.</returns>
    Public Shared Function CloneObject(Of T)(ByVal sourceObject As T) As T

        Return DirectCast(GetObject(GetBytes(sourceObject)), T)

    End Function

    ''' <summary>
    ''' Gets an instance of the specified type from the bytes of a previously serialized object.
    ''' </summary>
    ''' <typeparam name="T">Return type of the object.</typeparam>
    ''' <param name="serializedObject">The bytes of a previously serialized object.</param>
    ''' <returns>An instance of the specified type if the bytes can be deserialized; otherwise Nothing.</returns>
    Public Shared Function GetObject(Of T)(ByVal serializedObject As Byte()) As T

        Return DirectCast(GetObject(serializedObject), T)

    End Function

    ''' <summary>
    ''' Gets a System.Object instance from the bytes of a previously serialized object.
    ''' </summary>
    ''' <param name="serializedObject">The bytes of a previously serialized object.</param>
    ''' <returns>A System.Object instance if the specified bytes can be deserialized; otherwise Nothing.</returns>
    Public Shared Function GetObject(ByVal serializedObject As Byte()) As Object

        Dim deserializedObject As Object = Nothing
        Try
            Dim deserializer As New BinaryFormatter()
            deserializedObject = deserializer.Deserialize(GetStream(serializedObject))
        Catch ex As Exception

        End Try
        Return deserializedObject

    End Function

    ''' <summary>
    ''' Gets the bytes of a serializable object after serializing it.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object.</param>
    ''' <returns>
    ''' The bytes of an object after serializing it if the object is serializable; otherwise a zero-length byte array.
    ''' </returns>
    Public Shared Function GetBytes(ByVal serializableObject As Object) As Byte()

        Return GetStream(serializableObject).ToArray()

    End Function

    ''' <summary>
    ''' Gets a System.IO.MemoryStream from the bytes of a previously serialized object.
    ''' </summary>
    ''' <param name="serializedObject">The bytes of a previously serialized object.</param>
    ''' <returns>A System.IO.MemoryStream from the bytes of a previously serialized object.</returns>
    Public Shared Function GetStream(ByVal serializedObject As Byte()) As MemoryStream

        Return New MemoryStream(serializedObject)

    End Function

    ''' <summary>
    ''' Gets a System.IO.MemoryStream from a serializable object.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object.</param>
    ''' <returns>A System.IO.MemoryStream if the specified object can be serialized; otherwise an empty stream.</returns>
    Public Shared Function GetStream(ByVal serializableObject As Object) As MemoryStream

        Dim dataStream As New MemoryStream()
        If serializableObject.GetType.IsSerializable() Then
            Dim serializer As New BinaryFormatter()
            serializer.Serialize(dataStream, serializableObject)
        End If
        Return dataStream

    End Function

End Class
