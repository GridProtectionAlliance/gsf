'*******************************************************************************************************
'  ChannelCellParsingStateBase.vb - Channel frame cell parsing state base class
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
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the common implementation of the protocol independent parsing state of any kind of data cell.
Public MustInherit Class ChannelCellParsingStateBase

    Inherits ChannelParsingStateBase
    Implements IChannelCellParsingState

    Private m_phasorCount As Int32
    Private m_analogCount As Int32
    Private m_digitalCount As Int32

    Public Property PhasorCount() As Int32 Implements IChannelCellParsingState.PhasorCount
        Get
            Return m_phasorCount
        End Get
        Set(ByVal value As Int32)
            m_phasorCount = Value
        End Set
    End Property

    Public Property AnalogCount() As Int32 Implements IChannelCellParsingState.AnalogCount
        Get
            Return m_analogCount
        End Get
        Set(ByVal value As Int32)
            m_analogCount = Value
        End Set
    End Property

    Public Property DigitalCount() As Int32 Implements IChannelCellParsingState.DigitalCount
        Get
            Return m_digitalCount
        End Get
        Set(ByVal value As Int32)
            m_digitalCount = value
        End Set
    End Property

End Class

