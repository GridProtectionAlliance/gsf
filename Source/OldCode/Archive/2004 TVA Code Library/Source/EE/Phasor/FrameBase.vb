'***********************************************************************
'  FrameBase.vb - Basic phasor frame
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

    ' This class represents the common definition of all phasor message frames that can be sent or received from a PMU.
    Public MustInherit Class FrameBase

        Protected m_timeTag As NtpTimeTag
        Protected m_sampleCount As Int16
        Protected m_status As Int16

        Protected Sub New()

            m_timeTag = New NtpTimeTag(DateTime.Now)

        End Sub
        Protected Sub Clone(ByVal source As FrameBase)

            With source
                m_timeTag = .m_timeTag
                m_sampleCount = .m_sampleCount
                m_status = .m_status
            End With

        End Sub

        Public Property TimeTag() As NtpTimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public ReadOnly Property This() As FrameBase
            Get
                Return Me
            End Get
        End Property

        Public MustOverride Property SynchronizationIsValid() As Boolean

        Public MustOverride Property DataIsValid() As Boolean

        Protected Overridable Sub AppendCRC(ByVal buffer As Byte(), ByVal startIndex As Integer)

            EndianOrder.SwapCopyBytes(CRC16(-1, buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Protected Overridable ReadOnly Property Name() As String
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