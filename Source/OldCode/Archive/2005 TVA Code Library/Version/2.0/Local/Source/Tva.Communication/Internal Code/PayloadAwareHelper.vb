Imports TVA.Common

Friend NotInheritable Class PayloadAwareHelper

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Size of the header that is prepended to the payload. This header has information about the payload.
    ''' </summary>
    Public Const PayloadHeaderSize As Integer = 8

    ''' <summary>
    ''' A sequence of bytes that will mark the beginning of a payload.
    ''' </summary>
    Public Shared PayloadBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Public Shared Function AddPayloadHeader(ByVal payload As Byte()) As Byte()

        ' The resulting buffer will be 8 bytes bigger than the payload.
        ' Resulting buffer = 4 bytes for payload marker + 4 bytes for the payload size + The payload
        Dim result As Byte() = CreateArray(Of Byte)(payload.Length + PayloadHeaderSize)

        ' First, copy the the payload marker to the buffer.
        Buffer.BlockCopy(PayloadBeginMarker, 0, result, 0, 4)
        ' Then, copy the payload's size to the buffer after the payload marker.
        Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, result, 4, 4)
        ' At last, copy the payload after the payload marker and payload size.
        Buffer.BlockCopy(payload, 0, result, 8, payload.Length)

        Return result

    End Function

    Public Shared Function HasPayloadBeginMarker(ByVal data As Byte()) As Boolean

        For i As Integer = 0 To PayloadBeginMarker.Length - 1
            If Data(i) <> PayloadBeginMarker(i) Then Return False
        Next
        Return True

    End Function

    Public Shared Function GetPayloadSize(ByVal data As Byte()) As Integer

        If data.Length >= PayloadHeaderSize AndAlso HasPayloadBeginMarker(data) Then
            ' We have a buffer that's at least as big as the payload header and has the payload marker.
            Return BitConverter.ToInt32(data, PayloadBeginMarker.Length)
        Else
            Return -1
        End If

    End Function

    Public Shared Function GetPayload(ByVal data As Byte()) As Byte()

        If data.Length > PayloadHeaderSize AndAlso HasPayloadBeginMarker(data) Then
            Dim payloadSize As Integer = GetPayloadSize(data)
            If payloadSize > (data.Length - PayloadHeaderSize) Then
                payloadSize = data.Length - PayloadHeaderSize
            End If

            Return TVA.IO.Common.CopyBuffer(data, PayloadHeaderSize, payloadSize)
        Else
            Return New Byte() {}
        End If

    End Function

End Class
