'*******************************************************************************************************
'  Tva.IO.Stream.vb - Common Stream Manipulation Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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

Namespace IO.Stream

    ''' <summary>
    ''' <para>Defines common stream related manipulation functions</para>
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

    End Class

End Namespace
