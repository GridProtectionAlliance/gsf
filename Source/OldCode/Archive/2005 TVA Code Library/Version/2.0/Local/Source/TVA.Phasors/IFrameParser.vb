'*******************************************************************************************************
'  IFrameParser.vb - Frame parsing interface
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

''' <summary>This interface represents the protocol independent representation of a frame parser.</summary>
Public Interface IFrameParser

    Event ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame)
    Event ReceivedDataFrame(ByVal frame As IDataFrame)
    Event ReceivedHeaderFrame(ByVal frame As IHeaderFrame)
    Event ReceivedCommandFrame(ByVal frame As ICommandFrame)
    Event ReceivedUndeterminedFrame(ByVal frame As IChannelFrame)
    Event ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage As Byte(), ByVal offset As Integer, ByVal length As Integer)
    Event ConfigurationChanged()
    Event DataStreamException(ByVal ex As Exception)

    Sub Start()
    Sub [Stop]()
    ReadOnly Property Enabled() As Boolean
    Property ExecuteParseOnSeparateThread() As Boolean
    ReadOnly Property QueuedBuffers() As Int32
    Property ConfigurationFrame() As IConfigurationFrame
    Sub Write(ByVal buffer As Byte(), ByVal offset As Int32, ByVal count As Int32)
    ReadOnly Property Status() As String
    Property ConnectionParameters() As IConnectionParameters

End Interface
