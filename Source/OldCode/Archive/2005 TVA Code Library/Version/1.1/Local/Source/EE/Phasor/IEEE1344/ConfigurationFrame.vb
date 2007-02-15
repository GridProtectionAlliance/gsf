'*******************************************************************************************************
'  ConfigurationFrame.vb - IEEE1344 Configuration Frame
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime

Namespace EE.Phasor.IEEE1344

    ' This class represents a configuration frame that can be sent from a PMU.
    Public Class ConfigurationFrame

        Inherits BaseFrame

        Protected m_data As Byte()

        Protected Const FrameCountMask As Int16 = Not (FrameTypeMask Or Bit11 Or Bit12)

        Public Const MaximumFrameCount As Int16 = FrameCountMask

        Public Sub New()

            MyBase.New()
            SetFrameType(PMUFrameType.ConfigurationFrame)

        End Sub

        Protected Friend Sub New(ByVal parsedImage As BaseFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            ' No need to reparse data, so we pickup what's already been parsed...
            Clone(parsedImage)

            ' Get configuration data
            m_data = Array.CreateInstance(GetType(Byte), DataLength)
            Buffer.BlockCopy(binaryImage, startIndex, m_data, 0, m_data.Length)

        End Sub

        Public Overrides Property DataImage() As Byte()
            Get
                Return m_data
            End Get
            Set(ByVal Value As Byte())
                If Value.Length > MaximumDataLength Then
                    Throw New OverflowException("Data length cannot exceed " & MaximumDataLength & " per frame")
                Else
                    m_data = Value
                    DataLength = Value.Length
                End If
            End Set
        End Property

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
                Return ((m_sampleCount And Bit11) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_sampleCount = m_sampleCount And Not Bit11
                Else
                    m_sampleCount = m_sampleCount Or Bit11
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

        Protected Overrides ReadOnly Property Name() As String
            Get
                Return "IEEE1344.ConfigurationFrame"
            End Get
        End Property

    End Class

End Namespace