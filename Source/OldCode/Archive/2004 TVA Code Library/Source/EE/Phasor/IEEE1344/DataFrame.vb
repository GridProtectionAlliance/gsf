'***********************************************************************
'  DataFrame.vb - IEEE1344 Data Frame
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

Namespace EE.Phasor.IEEE1344

    ' This class represents a data frame that will be sent from a PMU during its real time data transmission.
    Public Class DataFrame

        Inherits BaseFrame

        Public Sub New()

            MyBase.New()
            m_frameType = PMUFrameType.DataFrame

        End Sub

        Friend Sub New(ByVal parsedImage As FrameParser, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

        End Sub

        Protected Overrides ReadOnly Property Name() As String
            Get

            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer = CommonBinaryLength

                Return length
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(MyBase.CommonBinaryImage, 0, buffer, 0, CommonBinaryLength)

                Return buffer
            End Get
        End Property

    End Class

End Namespace