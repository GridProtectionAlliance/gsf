'*******************************************************************************************************
'  TVA.IO.Common.vb - Common IO Related Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/24/2006 - J. Ritchie Carroll
'       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Common).
'  03/06/2007 - J. Ritchie Carroll
'       Added "CompareBuffers" method to compare to binary buffers.
'  08/22/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Imports TVA.Common

Namespace IO

    ''' <summary>Defines common IO related functions (e.g., common stream and buffer functions).</summary>
    Public NotInheritable Class Common

        Private Const BufferSize As Integer = 32768

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated.

        End Sub

        ''' <summary>Copies input stream onto output stream.</summary>
        Public Shared Sub CopyStream(ByVal inStream As System.IO.Stream, ByVal outStream As System.IO.Stream)

            Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim bytesRead As Integer = inStream.Read(buffer, 0, BufferSize)

            Do While bytesRead > 0
                outStream.Write(buffer, 0, bytesRead)
                bytesRead = inStream.Read(buffer, 0, BufferSize)
            Loop

        End Sub

        ''' <summary>Reads entire stream contents, and returns byte array of data.</summary>
        ''' <remarks>Note: You should only use this on streams where you know the data size is small.</remarks>
        Public Shared Function ReadStream(ByVal inStream As System.IO.Stream) As Byte()

            Dim outStream As New System.IO.MemoryStream

            CopyStream(inStream, outStream)

            Return outStream.ToArray()

        End Function

        ''' <summary>Returns a copy of the specified portion of the source buffer.</summary>
        ''' <remarks>Grows or shrinks returned buffer, as needed, to make it the desired length.</remarks>
        Public Shared Function CopyBuffer(ByVal buffer As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Byte()

            Dim copiedBytes As Byte() = CreateArray(Of Byte)(IIf(buffer.Length - startIndex < length, buffer.Length - startIndex, length))

            System.Buffer.BlockCopy(buffer, startIndex, copiedBytes, 0, copiedBytes.Length)

            Return copiedBytes

        End Function

        ''' <summary>Returns comparision results of two binary buffers.</summary>
        Public Shared Function CompareBuffers(ByVal buffer1 As Byte(), ByVal buffer2 As Byte()) As Boolean

            If buffer1 Is Nothing AndAlso buffer2 Is Nothing Then
                ' Both buffers are assumed equal if both are nothing.
                Return 0
            ElseIf buffer1 Is Nothing Then
                ' Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                Return 1
            ElseIf buffer2 Is Nothing Then
                ' Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                Return -1
            Else
                ' Code replicated here as an optimization, instead of calling overloaded CompareBuffers
                ' to prevent duplicate "Is Nothing" checks for empty buffers. This function needs to
                ' execute as quickly as possible given possible intended uses.
                Dim length1 As Integer = buffer1.Length
                Dim length2 As Integer = buffer2.Length

                If length1 = length2 Then
                    Dim comparision As Integer

                    ' Compares elements of buffers that are of equal size.
                    For x As Integer = 0 To length1 - 1
                        comparision = buffer1(x).CompareTo(buffer2(x))
                        If comparision <> 0 Then Exit For
                    Next

                    Return comparision
                Else
                    ' Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                    Return length1.CompareTo(length2)
                End If
            End If

        End Function

        Public Shared Function CompareBuffers(ByVal buffer1 As Byte(), ByVal offset1 As Integer, ByVal length1 As Integer, ByVal buffer2 As Byte(), ByVal offset2 As Integer, ByVal length2 As Integer) As Boolean

            If buffer1 Is Nothing AndAlso buffer2 Is Nothing Then
                ' Both buffers are assumed equal if both are nothing.
                Return 0
            ElseIf buffer1 Is Nothing Then
                ' Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                Return 1
            ElseIf buffer2 Is Nothing Then
                ' Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                Return -1
            Else
                If length1 = length2 Then
                    Dim comparision As Integer

                    ' Compares elements of buffers that are of equal size.
                    For x As Integer = 0 To length1 - 1
                        comparision = buffer1(offset1 + x).CompareTo(buffer2(offset2 + x))
                        If comparision <> 0 Then Exit For
                    Next

                    Return comparision
                Else
                    ' Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                    Return length1.CompareTo(length2)
                End If
            End If

        End Function

    End Class

End Namespace
