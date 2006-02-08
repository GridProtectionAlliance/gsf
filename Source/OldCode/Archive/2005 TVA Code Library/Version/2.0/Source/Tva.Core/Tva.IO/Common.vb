'*******************************************************************************************************
'  Tva.IO.Common.vb - Common IO Related Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/24/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Namespace IO

    ''' <summary>
    ''' <para>Defines common IO related functions (e.g., common stream and buffer functions)</para>
    ''' </summary>
    Public NotInheritable Class Common

        Private Const BufferSize As Integer = 8192

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para>Copies input stream onto output stream</para>
        ''' </summary>
        Public Shared Sub CopyStream(ByVal inStream As System.IO.Stream, ByVal outStream As System.IO.Stream)

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim bytesRead As Integer = inStream.Read(buffer, 0, BufferSize)

            Do While bytesRead > 0
                outStream.Write(buffer, 0, bytesRead)
                bytesRead = inStream.Read(buffer, 0, BufferSize)
            Loop

        End Sub

        ''' <summary>
        ''' <para>Reads entire stream contents and returns byte array of data</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>Note: you should only use this on streams where you know the data size to be small</para>
        ''' </remarks>
        Public Shared Function ReadStream(ByVal inStream As System.IO.Stream) As Byte()

            Dim outStream As New System.IO.MemoryStream

            CopyStream(inStream, outStream)

            Return outStream.ToArray()

        End Function

        ''' <summary>
        ''' <para>Returns a buffer of the specified length</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function will grow or shrink a buffer as needed to make it the desired length</para>
        ''' </remarks>
        Public Shared Function TruncateBuffer(ByVal dataBuffer As Byte(), ByVal length As Integer) As Byte()

            If dataBuffer.Length = length Then
                Return dataBuffer
            Else
                Dim newBuffer As Byte() = Array.CreateInstance(GetType(Byte), length)
                Buffer.BlockCopy(dataBuffer, 0, newBuffer, 0, length)
                Return newBuffer
            End If

        End Function

    End Class

End Namespace
