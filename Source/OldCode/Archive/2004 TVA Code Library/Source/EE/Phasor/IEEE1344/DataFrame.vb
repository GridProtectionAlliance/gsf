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

        Protected Const SampleCountMask As Int16 = Not FrameTypeMask

        Public Sub New()

            MyBase.New()
            SetFrameType(PMUFrameType.DataFrame)

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

        End Sub

        Public Overridable Property SampleCount() As Int16
            Get
                Return m_sampleCount And SampleCountMask
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumSampleCount Then
                    Throw New OverflowException("Sample count value cannot exceed " & MaximumSampleCount)
                Else
                    m_sampleCount = (m_sampleCount And Not SampleCountMask) Or Value
                End If
            End Set
        End Property

        Public ReadOnly Property MaximumSampleCount() As Int16
            Get
                Return SampleCountMask
            End Get
        End Property

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "IEEE1344.DataFrame"
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer = CommonBinaryLength + 2

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