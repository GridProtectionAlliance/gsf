'***********************************************************************
'  HeaderFrame.vb - IEEE1344 Header Frame
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
Imports System.Text

Namespace EE.Phasor.IEEE1344

    ' This class represents a header frame that can be sent from a PMU.
    Public Class HeaderFrame

        Inherits BaseFrame

        Protected m_data As String

        Protected Const FrameCountMask As Int16 = Not (FrameTypeMask Or Bit11 Or Bit12)

        Public Sub New()

            MyBase.New()
            SetFrameType(PMUFrameType.HeaderFrame)
            m_data = "<empty header frame>"

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

            ' Get header data
            m_data = Encoding.ASCII.GetString(binaryImage, startIndex, DataLength)

        End Sub

        Public Overridable Property IsFirstFrame() As Boolean
            Get
                Return ((m_sampleCount And Bit12) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount And Not Bit12
                Else
                    m_sampleCount = m_sampleCount Or Bit12
                End If
            End Set
        End Property

        Public Overridable Property IsLastFrame() As Boolean
            Get
                Return ((m_sampleCount And Bit11) > 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount Or Bit11
                Else
                    m_sampleCount = m_sampleCount And Not Bit11
                End If
            End Set
        End Property

        Public Overridable Property FrameCount() As Int16
            Get
                Return m_sampleCount And FrameCountMask
            End Get
            Set(ByVal Value As Int16)
                If Value > MaximumFrameCount Then
                    Throw New OverflowException("Frame count value cannot exceed " & MaximumFrameCount)
                Else
                    m_sampleCount = (m_sampleCount And Not FrameCountMask) Or Value
                End If
            End Set
        End Property

        Public ReadOnly Property MaximumFrameCount() As Int16
            Get
                Return FrameCountMask
            End Get
        End Property

        Public Property Data() As String
            Get
                Return m_data
            End Get
            Set(ByVal Value As String)
                If Len(Value) > MaximumDataLength Then
                    Throw New OverflowException("Data length cannot exceed " & MaximumDataLength & " per frame")
                Else
                    m_data = Value
                End If
            End Set
        End Property

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "IEEE1344.HeaderFrame"
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Dim length As Integer = CommonBinaryLength + 2

                length += m_data.Length

                Return length
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                Array.Copy(MyBase.CommonBinaryImage, 0, buffer, 0, CommonBinaryLength)
                Array.Copy(Encoding.ASCII.GetBytes(m_data), 0, buffer, CommonBinaryLength, m_data.Length)
                AppendCRC16(buffer, 0, CommonBinaryLength + m_data.Length)

                Return buffer
            End Get
        End Property

    End Class

End Namespace