'***********************************************************************
'  FrameParser.vb - IEEE1344 Frame Parser
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common
Imports System.ComponentModel

Namespace EE.Phasor.IEEE1344

    ' This class will parse a frame and return the appropriate frame type
    Public Class FrameParser

        Inherits BaseFrame

        Private Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(binaryImage, startIndex)

        End Sub

        Public Shared Function Parse(ByRef frame As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer) As PMUFrameType

            Dim parsedImage As New FrameParser(binaryImage, startIndex)

            Select Case parsedImage.FrameType
                Case PMUFrameType.HeaderFrame
                    frame = New HeaderFrame(parsedImage, binaryImage, startIndex + CommonBinaryLength)
                Case PMUFrameType.ConfigurationFrame
                    frame = New ConfigurationFrame(parsedImage, binaryImage, startIndex + CommonBinaryLength)
                Case PMUFrameType.DataFrame
                    frame = New DataFrame(parsedImage, binaryImage, startIndex + CommonBinaryLength)
            End Select

            Return parsedImage.FrameType

        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Throw New NotImplementedException
            End Get
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

    End Class

End Namespace