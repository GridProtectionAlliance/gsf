'*******************************************************************************************************
'  ChannelCellParsingStateBase.vb - Channel frame cell parsing state base class
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
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent parsing state of any kind of data cell.
    Public MustInherit Class ChannelCellParsingStateBase

        Inherits ChannelParsingStateBase
        Implements IChannelCellParsingState

        Private m_parent As IChannelFrame

        Protected Sub New(ByVal parent As IChannelFrame)

            MyBase.New()
            m_parent = parent

        End Sub

        Public ReadOnly Property Parent() As IChannelFrame Implements IChannelCellParsingState.Parent
            Get
                Return m_parent
            End Get
        End Property

    End Class

End Namespace
