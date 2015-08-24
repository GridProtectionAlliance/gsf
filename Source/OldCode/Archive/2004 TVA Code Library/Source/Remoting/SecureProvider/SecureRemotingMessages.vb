' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.ComponentModel
Imports System.Runtime.Serialization.Formatters.Binary
Imports TVA.Shared.Crypto
Imports TVA.Shared.String

Namespace Remoting.SecureProvider

    ' If interversion .NET compatibiliy is needed for remoting, then this class should exist as an independent
    ' .NET 1.0 assembly that allows the following common remoting interfaces to be shared between clients
    ' written using different frameworks...
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class SecureRemotingMessages

        <EditorBrowsable(EditorBrowsableState.Never)> Public Const RequestInfoHeader As String = "SecRequestInfo"
        <EditorBrowsable(EditorBrowsableState.Never)> Public Const RequestAuthHeader As String = "SecRequestAuth"
        <EditorBrowsable(EditorBrowsableState.Never)> Public Const ResponseInfoHeader As String = "SecResponseInfo"
        <EditorBrowsable(EditorBrowsableState.Never)> Public Const ResponseErrorHeader As String = "SecResponseError"
        <EditorBrowsable(EditorBrowsableState.Never)> Public Const MessageEncryptionLevel As EncryptLevel = EncryptLevel.Level4

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Enum ClientRequestType
            Undetermined
            ConnectionRequest
            NewSharedKeyRequest
            AuthenticationRequest
            Execution
        End Enum

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Enum ServerResponseType
            Undetermined
            ConnectionGranted
            NewSharedKeyAccepted
            AuthenticationSucceeded
            AuthenticationFailed
        End Enum

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Enum ServerErrorResponseType
            Undetermined
            RequestUnidentified
            ClientUnidentified
            ClientUnauthenticated
        End Enum

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Class ClientRequest

            Public RequestID As Guid
            Public Type As ClientRequestType = ClientRequestType.Undetermined
            Public Key As Byte()

            Public Shared Function Encode(ByVal Request As ClientRequest, ByVal EncodeKey As Byte()) As String

                Try
                    With New BinaryFormatter
                        Dim WorkStream As New MemoryStream
                        .Serialize(WorkStream, Request)
                        WorkStream.Position = 0
                        Return Base64Encode(Encrypt(WorkStream.ToArray(), EncodeKey, EncodeKey, (EncodeKey(0) Mod 2) + 1))
                    End With
                Catch
                    Return ""
                End Try

            End Function

            Public Shared Function Decode(ByVal Request As String, ByVal DecodeKey As Byte()) As ClientRequest

                Try
                    Dim WorkStream As New MemoryStream(Decrypt(Base64Decode(Request), DecodeKey, DecodeKey, (DecodeKey(0) Mod 2) + 1))
                    WorkStream.Position = 0
                    Return DirectCast((New BinaryFormatter).Deserialize(WorkStream), ClientRequest)
                Catch
                    Return New ClientRequest
                End Try

            End Function

        End Class

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Class ServerResponse

            Public ResponseID As Guid
            Public Type As ServerResponseType = ServerResponseType.Undetermined
            Public Key As Byte()

            Public Shared Function Encode(ByVal Response As ServerResponse, ByVal EncodeKey As Byte()) As String

                Try
                    With New BinaryFormatter
                        Dim WorkStream As New MemoryStream
                        .Serialize(WorkStream, Response)
                        WorkStream.Position = 0
                        Return Base64Encode(Encrypt(WorkStream.ToArray(), EncodeKey, EncodeKey, (EncodeKey(0) Mod 2) + 1))
                    End With
                Catch
                    Return ""
                End Try

            End Function

            Public Shared Function Decode(ByVal Response As String, ByVal DecodeKey As Byte()) As ServerResponse

                Try
                    Dim WorkStream As New MemoryStream(Decrypt(Base64Decode(Response), DecodeKey, DecodeKey, (DecodeKey(0) Mod 2) + 1))
                    WorkStream.Position = 0
                    Return DirectCast((New BinaryFormatter).Deserialize(WorkStream), ServerResponse)
                Catch
                    Return New ServerResponse
                End Try

            End Function

        End Class

        <Serializable(), EditorBrowsable(EditorBrowsableState.Never)> _
        Public Class ServerErrorResponse

            Public ResponseID As Guid
            Public Type As ServerErrorResponseType = ServerErrorResponseType.Undetermined
            Public Message As String

            Public Shared Function Encode(ByVal Response As ServerErrorResponse, ByVal EncodeKey As Byte()) As String

                Try
                    With New BinaryFormatter
                        Dim WorkStream As New MemoryStream
                        .Serialize(WorkStream, Response)
                        WorkStream.Position = 0
                        Return Base64Encode(Encrypt(WorkStream.ToArray(), EncodeKey, EncodeKey, (EncodeKey(0) Mod 2) + 1))
                    End With
                Catch
                    Return ""
                End Try

            End Function

            Public Shared Function Decode(ByVal Response As String, ByVal DecodeKey As Byte()) As ServerErrorResponse

                Try
                    Dim WorkStream As New MemoryStream(Decrypt(Base64Decode(Response), DecodeKey, DecodeKey, (DecodeKey(0) Mod 2) + 1))
                    WorkStream.Position = 0
                    Return DirectCast((New BinaryFormatter).Deserialize(WorkStream), ServerErrorResponse)
                Catch
                    Return New ServerErrorResponse
                End Try

            End Function

        End Class

        ' This function creates a variable number of encryption cycles based on the first key byte
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Shared Function GetKeyCycles(ByVal FirstKeyByte As Byte) As Integer

            Return (FirstKeyByte Mod 4) + 2 ' 2 thru 6

        End Function

    End Class

End Namespace