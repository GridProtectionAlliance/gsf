'***********************************************************************
'  DataFrameBase.vb - Data frame base class
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

    ' This class represents the protocol independent common implementation of all data frames that can be sent or received from a PMU.
    Public MustInherit Class DataFrameBase

        Implements IDataFrame

        Protected m_timeTag As NtpTimeTag
        Protected m_sampleCount As Int16    ' TODO: Validate cross protocol compatibility of sample count and status...
        Protected m_status As Int16

        Protected Sub New()

            m_timeTag = New NtpTimeTag(DateTime.Now)

        End Sub

        Protected Overridable Sub Clone(ByVal source As DataFrameBase)

            With source
                m_timeTag = .m_timeTag
                m_sampleCount = .m_sampleCount
                m_status = .m_status
            End With

        End Sub

        Public Overridable Property TimeTag() As NtpTimeTag Implements IDataFrame.TimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timeTag = Value
            End Set
        End Property

        Public MustOverride Property Milliseconds() As Double Implements IDataFrame.Milliseconds

        Public Overridable ReadOnly Property Timestamp() As DateTime Implements IDataFrame.Timestamp
            Get
                Return TimeTag.ToDateTime.AddMilliseconds(Milliseconds)
            End Get
        End Property

        Public Overridable ReadOnly Property This() As IDataFrame Implements IDataFrame.This
            Get
                Return Me
            End Get
        End Property

        Public MustOverride Property SynchronizationIsValid() As Boolean Implements IDataFrame.SynchronizationIsValid

        Public MustOverride Property DataIsValid() As Boolean Implements IDataFrame.DataIsValid

        Protected Overridable Sub AppendCRC(ByVal buffer As Byte(), ByVal startIndex As Integer)

            EndianOrder.SwapCopyBytes(CRC16(-1, buffer, 0, startIndex), buffer, startIndex)

        End Sub

        Public Overridable ReadOnly Property Name() As String Implements IDataFrame.Name
            Get
                Return "TVA.EE.Phasor.DataFrameBase"
            End Get
        End Property

        Public MustOverride Property DataLength() As Int16 Implements IDataFrame.DataLength

        Public MustOverride Property DataImage() As Byte() Implements IDataFrame.DataImage

        Public MustOverride Property BinaryLength() As Int16 Implements IDataFrame.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IDataFrame.BinaryImage

    End Class

End Namespace