'*******************************************************************************************************
'  TVA.Serialization.vb - Common serialization related functions
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

Option Strict On

Imports System.IO
Imports System.Xml.Serialization
Imports System.Runtime.Serialization.Formatters.Binary

Public NotInheritable Class Serialization

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Creates a clone of a serializable object.
    ''' </summary>
    Public Shared Function CloneObject(Of T)(ByVal sourceObject As T) As T

        Return GetObject(Of T)(GetBytes(sourceObject))

    End Function

    ''' <summary>
    ''' Performs XML deserialization on the XML string and returns the typed object for it.
    ''' </summary>
    Public Shared Function GetObject(Of T)(ByVal serializedObject As String) As T

        Dim deserializedObject As Object = Nothing
        Try
            Dim deserializer As New XmlSerializer(GetType(T))
            deserializedObject = deserializer.Deserialize(New StringReader(serializedObject))
        Catch ex As Exception

        End Try
        Return DirectCast(deserializedObject, T)

    End Function

    ''' <summary>
    ''' Performs binary deserialization on the byte array and returns the typed object for it.
    ''' </summary>
    Public Shared Function GetObject(Of T)(ByVal serializedObject As Byte()) As T

        Dim deserializedObject As Object = GetObject(serializedObject)
        If TypeOf deserializedObject Is T Then  ' Cannot use TryCast because of the templated parameter restriction.
            Return DirectCast(GetObject(serializedObject), T)
        Else
            Return Nothing
        End If

    End Function

    ''' <summary>
    ''' Performs binary deserialization on the byte array and returns the object for it.
    ''' </summary>
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
    ''' Performs XML serialization on the serializable object and returns the output as string.
    ''' </summary>
    Public Shared Function GetString(ByVal serializableObject As Object) As String

        Dim serializedObject As New StringWriter()
        If serializableObject.GetType().IsSerializable Then
            ' The specified object if marked as serializable.
            Dim serializer As New XmlSerializer(serializableObject.GetType())
            serializer.Serialize(serializedObject, serializableObject)
        End If
        Return serializedObject.ToString()

    End Function

    ''' <summary>
    ''' Performs binary serialization on the serializable object and returns the output as byte array.
    ''' </summary>
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
        If serializableObject.GetType().IsSerializable Then
            Dim serializer As New BinaryFormatter()
            serializer.Serialize(dataStream, serializableObject)
        End If
        Return dataStream

    End Function

End Class
