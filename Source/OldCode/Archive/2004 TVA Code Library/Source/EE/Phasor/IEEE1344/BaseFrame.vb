'***********************************************************************
'  BaseFrame.vb - Basic IEEE1344 Frame
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

    Public Enum PMUFrameType
        DataFrame
        HeaderFrame
        ConfigurationFrame
    End Enum

    Public Enum PMUTriggerStatus
        None
        FrequencyTrigger
        DfDtTrigger
        AngleTrigger
        OverCurrentTrigger
        UnderVoltageTrigger
        RateTrigger
        Undetermined
    End Enum

    ' This class represents the common functionality of all the IEEE1344 message frames that can be sent from a PMU.
    Public MustInherit Class BaseFrame

        Protected m_frameType As PMUFrameType
        Protected m_timeTag As NtpTimeTag
        Protected m_triggerStatus As PMUTriggerStatus
        Protected m_sampleCountWord As Int16
        Protected m_statusWord As Int16
        Protected m_crc16 As Int16

        Protected Const CommonBinaryLength As Integer = 8

        Public Sub New()

            m_triggerStatus = PMUTriggerStatus.None
            m_crc16 = -1

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New()

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create " & Name)
            ElseIf binaryImage.Length - startIndex < BinaryLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create " & Name)
            Else
                m_timeTag = New NtpTimeTag(Convert.ToDouble(EndianOrder.ReverseToInt32(binaryImage, startIndex)))
                SampleCountWord = EndianOrder.ReverseToInt16(binaryImage, startIndex + 4)
                StatusWord = EndianOrder.ReverseToInt16(binaryImage, startIndex + 6)
            End If

        End Sub

        Protected Sub Clone(ByVal source As BaseFrame)

            With source
                m_frameType = .m_frameType
                m_timeTag = .m_timeTag
                m_triggerStatus = .m_triggerStatus
                m_sampleCountWord = .m_sampleCountWord
                m_statusWord = .m_statusWord
                m_crc16 = .m_crc16
            End With

        End Sub

        Public ReadOnly Property FrameType() As PMUFrameType
            Get
                Return m_frameType
            End Get
        End Property

        Public Property SecondOfCentury() As NtpTimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Protected Overridable ReadOnly Property Name() As String
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Property SynchronizationIsValid() As Boolean
            Get
                Return ((m_statusWord And Bit15) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_statusWord = m_statusWord Or Not Bit15
                Else
                    m_statusWord = m_statusWord Or Bit15
                End If
            End Set
        End Property

        Public Property DataIsValid() As Boolean
            Get
                Return ((m_statusWord And Bit14) = 0)
            End Get
            Set(ByVal Value As Boolean)
                If Value Then
                    m_statusWord = m_statusWord Or Not Bit14
                Else
                    m_statusWord = m_statusWord Or Bit14
                End If
            End Set
        End Property

        Public Property TriggerStatus() As PMUTriggerStatus
            Get
                Return m_triggerStatus
            End Get
            Set(ByVal Value As PMUTriggerStatus)
                Select Case Value
                    Case PMUTriggerStatus.None
                    Case PMUTriggerStatus.FrequencyTrigger
                    Case PMUTriggerStatus.DfDtTrigger
                    Case PMUTriggerStatus.AngleTrigger
                    Case PMUTriggerStatus.OverCurrentTrigger
                    Case PMUTriggerStatus.UnderVoltageTrigger
                    Case PMUTriggerStatus.RateTrigger
                    Case PMUTriggerStatus.Undetermined
                End Select
            End Set
        End Property

        Private WriteOnly Property SampleCountWord() As Int16
            Set(ByVal Value As Int16)
                m_sampleCountWord = Value

            End Set
        End Property

        Private WriteOnly Property StatusWord() As Int16
            Set(ByVal Value As Int16)
                m_statusWord = Value

                If (m_statusWord And Bit13) > 0 And (m_statusWord And Bit12) > 0 And (m_statusWord And Bit11) > 0 Then
                    m_triggerStatus = PMUTriggerStatus.FrequencyTrigger
                Else
                    m_triggerStatus = PMUTriggerStatus.None
                End If
            End Set
        End Property

        Public MustOverride ReadOnly Property BinaryLength() As Integer
        Public MustOverride ReadOnly Property BinaryImage() As Byte()

        Protected ReadOnly Property CommonBinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), CommonBinaryLength)

                EndianOrder.SwapCopy(BitConverter.GetBytes(Convert.ToUInt32(m_timeTag.Value)), 0, buffer, 0, 4)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_sampleCountWord), 0, buffer, 4, 2)
                EndianOrder.SwapCopy(BitConverter.GetBytes(m_statusWord), 0, buffer, 6, 2)

                Return buffer
            End Get
        End Property

    End Class

End Namespace