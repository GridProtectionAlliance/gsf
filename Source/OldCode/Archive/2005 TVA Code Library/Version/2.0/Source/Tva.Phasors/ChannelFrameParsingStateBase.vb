'*******************************************************************************************************
'  ChannelFrameParsingStateBase.vb - Channel data frame parsing state base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
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

' This class represents the protocol independent common implementation the parsing state used by any frame of data that can be sent or received from a PMU.
<CLSCompliant(False)> _
Public MustInherit Class ChannelFrameParsingStateBase(Of T As IChannelCell)

    Inherits ChannelParsingStateBase
    Implements IChannelFrameParsingState(Of T)

    Private m_cells As IChannelCellCollection(Of T)
    Private m_cellCount As Integer
    Private m_parsedBinaryLength As UInt16
    Private m_createNewCellFunction As IChannelFrameParsingState(Of T).CreateNewCellFunctionSignature

    Public Sub New(ByVal cells As IChannelCellCollection(Of T), ByVal parsedBinaryLength As UInt16, ByVal createNewCellFunction As IChannelFrameParsingState(Of T).CreateNewCellFunctionSignature)

        m_cells = cells
        m_parsedBinaryLength = parsedBinaryLength
        m_createNewCellFunction = createNewCellFunction

    End Sub

    Public ReadOnly Property CreateNewCellFunction() As IChannelFrameParsingState(Of T).CreateNewCellFunctionSignature Implements IChannelFrameParsingState(Of T).CreateNewCellFunction
        Get
            Return m_createNewCellFunction
        End Get
    End Property

    Public ReadOnly Property Cells() As IChannelCellCollection(Of T) Implements IChannelFrameParsingState(Of T).Cells
        Get
            Return m_cells
        End Get
    End Property

    Public Property CellCount() As Integer Implements IChannelFrameParsingState(Of T).CellCount
        Get
            Return m_cellCount
        End Get
        Set(ByVal value As Integer)
            m_cellCount = value
        End Set
    End Property

    Public Property ParsedBinaryLength() As UInt16 Implements IChannelFrameParsingState(Of T).ParsedBinaryLength
        Get
            Return m_parsedBinaryLength
        End Get
        Set(ByVal value As UInt16)
            m_parsedBinaryLength = value
        End Set
    End Property

End Class
