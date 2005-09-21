'*******************************************************************************************************
'  PMUCellCollection.vb - PDCstream specific PMU cell collection - this is a PDC block
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Shared.DateTime

Namespace EE.Phasor.PDCstream

    Public Class PMUCellCollection

        Inherits ChannelCellCollectionBase
        Implements IChannelFrame    ' This collection doubles as a "proxy" frame for PMU cells

        Private m_parent As DataCell
        Private m_flags As ChannelFlags

        Public Sub New(ByVal parent As DataCell)

            ' Total possible number of PMU cells in a PDC block is 256
            MyBase.New(Byte.MaxValue, True)

            m_parent = parent

        End Sub

        Public Sub New(ByVal parent As DataCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(Byte.MaxValue, True)

            m_parent = parent

            ' TODO: parse channel flags and PMU cells from binary image

        End Sub

        Public Shadows Sub Add(ByVal value As PMUCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As PMUCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property InheritedType() As Type
            Get
                Return Me.GetType()
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

        Public Property Parent() As DataCell
            Get
                Return m_parent
            End Get
            Set(ByVal Value As DataCell)
                m_parent = Value
            End Set
        End Property

        ' In order for PMU cells to be channel cells, they expect a parent frame so
        ' we make this parent cell collection act as a frame just providing readonly
        ' data from the real parent frame
        Public ReadOnly Property Cells() As IChannelCellCollection Implements IChannelFrame.Cells
            Get
                Return Me
            End Get
        End Property

        Public Property DataIsValid() As Boolean Implements IChannelFrame.DataIsValid
            Get
                Return m_parent.Parent.DataIsValid
            End Get
            Set(ByVal Value As Boolean)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Property Milliseconds() As Double Implements IChannelFrame.Milliseconds
            Get
                Return m_parent.Parent.Milliseconds
            End Get
            Set(ByVal Value As Double)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Property NtpTimeTag() As NtpTimeTag Implements IChannelFrame.NtpTimeTag
            Get
                Return m_parent.Parent.NtpTimeTag
            End Get
            Set(ByVal Value As NtpTimeTag)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Property Published() As Boolean Implements IChannelFrame.Published
            Get
                Return m_parent.Parent.Published
            End Get
            Set(ByVal Value As Boolean)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Property SynchronizationIsValid() As Boolean Implements IChannelFrame.SynchronizationIsValid
            Get
                Return m_parent.Parent.SynchronizationIsValid
            End Get
            Set(ByVal Value As Boolean)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public ReadOnly Property Timestamp() As Date Implements IChannelFrame.Timestamp
            Get
                Return m_parent.Parent.Timestamp
            End Get
        End Property

        Public Property TimeTag() As Interop.Unix.TimeTag Implements IChannelFrame.TimeTag
            Get
                Return m_parent.Parent.TimeTag
            End Get
            Set(ByVal Value As Interop.Unix.TimeTag)
                Throw New NotImplementedException("This property is readonly in this implementation")
            End Set
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                Return 4 + MyBase.BinaryLength
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)


                Return buffer
            End Get
        End Property

    End Class

End Namespace