'*******************************************************************************************************
'  PMUCellCollection.vb - PDCstream specific PMU cell collection - this is a PDC block
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.DateTime

Namespace BpaPdcStream

    <CLSCompliant(False), Serializable()> _
    Public Class PMUCellCollection

        Inherits ChannelCellCollectionBase(Of PMUCell)
        Implements IChannelFrame

        ' This collection doubles as a "proxy" frame for PMU cells

        Private m_parent As DataCell
        Private m_flags As ChannelFlags

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            m_parent = info.GetValue("parent", GetType(DataCell))
            m_flags = info.GetValue("flags", GetType(ChannelFlags))

        End Sub

        Public Sub New(ByVal parent As DataCell)

            ' Total possible number of PMU cells in a PDC block is 256
            MyBase.New(Byte.MaxValue, True)

            m_parent = parent

        End Sub

        Public Sub New(ByVal parent As DataCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(Byte.MaxValue, True)

            m_parent = parent

            ' TODO: parse channel flags and PMU cells from binary image

        End Sub

        Public Overrides ReadOnly Property DerivedType() As Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public ReadOnly Property Parent() As DataCell
            Get
                Return m_parent
            End Get
        End Property

        Private ReadOnly Property FundamentalFrameType() As FundamentalFrameType Implements IChannelFrame.FrameType
            Get
                Return Phasors.FundamentalFrameType.DataFrame
            End Get
        End Property

        Public Shadows Sub Add(ByVal value As PMUCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Int32) As PMUCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Property ChannelFlags() As ChannelFlags
            Get
                Return m_flags
            End Get
            Set(ByVal Value As ChannelFlags)
                m_flags = Value
            End Set
        End Property

        ' In order for PMU cells to be channel cells, they expect a parent frame so
        ' we make this parent cell collection act as a frame just providing readonly
        ' data from the real parent frame
        Public ReadOnly Property Cells() As Object Implements IChannelFrame.Cells
            Get
                Return Me
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Property IDCode() As UInt16 Implements IChannelFrame.IDCode
            Get
                Return m_parent.Parent.IDCode
            End Get
            Set(ByVal value As UInt16)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Property Ticks() As Long Implements IChannelFrame.Ticks
            Get
                Return m_parent.Parent.Ticks
            End Get
            Set(ByVal value As Long)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public ReadOnly Property NtpTimeTag() As NtpTimeTag
            Get
                Return New NtpTimeTag(Timestamp)
            End Get
        End Property

        Public Property Published() As Boolean Implements IChannelFrame.Published
            Get
                Return m_parent.Parent.Published
            End Get
            Set(ByVal Value As Boolean)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Overridable ReadOnly Property IsPartial() As Boolean Implements IChannelFrame.IsPartial
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Timestamp() As Date Implements IChannelFrame.Timestamp
            Get
                Return m_parent.Parent.Timestamp
            End Get
        End Property

        Public ReadOnly Property TimeTag() As UnixTimeTag Implements IChannelFrame.TimeTag
            Get
                Return m_parent.Parent.TimeTag
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As UInt16
            Get
                Return 4 + MyBase.BinaryLength
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(BinaryLength)


                Return buffer
            End Get
        End Property

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is IChannelFrame Then
                Return Ticks.CompareTo(DirectCast(obj, IChannelFrame).Ticks)
            Else
                Throw New ArgumentException(DerivedType.Name & " can only be compared with other IChannelFrames...")
            End If

        End Function

        Private ReadOnly Property IFrameThis() As Measurements.IFrame Implements Measurements.IFrame.This
            Get
                Return Me
            End Get
        End Property

        Private ReadOnly Property IFrameMeasurements() As Dictionary(Of Measurements.MeasurementKey, Measurements.IMeasurement) Implements Measurements.IFrame.Measurements
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            info.AddValue("parent", m_parent, GetType(DataCell))
            info.AddValue("flags", m_flags, GetType(ChannelFlags))

        End Sub

    End Class

End Namespace