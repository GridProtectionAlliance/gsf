'***********************************************************************
'  PhasorFrameBase.vb - Phasor frame base class
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

Namespace EE.Phasor

    Public Interface IPhasorFrame

        Property TimeTag() As NtpTimeTag

        Property Milliseconds() As Double

        ReadOnly Property Timestamp() As DateTime

        ReadOnly Property This() As IPhasorFrame

        Property SynchronizationIsValid() As Boolean

        Property DataIsValid() As Boolean

        ReadOnly Property Name() As String

        Property DataLength() As Int16

        Property DataImage() As Byte()

        Property BinaryLength() As Int16

        ReadOnly Property BinaryImage() As Byte()

    End Interface

    ' This class represents the common definition of all phasor message frames that can be sent or received from a PMU.
    Public MustInherit Class PhasorFrameBase

        Implements IPhasorFrame

        Protected m_timeTag As NtpTimeTag
        Protected m_sampleCount As Int16
        Protected m_status As Int16

        Protected Sub New()

            m_timeTag = New NtpTimeTag(DateTime.Now)

        End Sub

        Protected Sub Clone(ByVal source As PhasorFrameBase)

            With source
                m_timeTag = .m_timeTag
                m_sampleCount = .m_sampleCount
                m_status = .m_status
            End With

        End Sub

        Public Property TimeTag() As NtpTimeTag Implements IPhasorFrame.TimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public Property Milliseconds() As Double
            Get

            End Get
            Set(ByVal Value As Double)

            End Set
        End Property

        Public ReadOnly Property This() As PhasorFrameBase
            Get
                Return Me
            End Get
        End Property

        Public MustOverride Property SynchronizationIsValid() As Boolean

        Public MustOverride Property DataIsValid() As Boolean

        Protected Overridable Sub AppendCRC(ByVal buffer As Byte(), ByVal startIndex As Integer)

            EndianOrder.SwapCopyBytes(CRC16(-1, buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Public Overridable ReadOnly Property Name() As String
            Get
                Return "Phasor.FrameBase"
            End Get
        End Property

        Public MustOverride Property DataLength() As Int16

        Public MustOverride Property DataImage() As Byte()

        Public MustOverride Property BinaryLength() As Int16

        Public MustOverride ReadOnly Property BinaryImage() As Byte()

    End Class

End Namespace