'*******************************************************************************************************
'  IChannelValue.vb - Channel data value interface
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
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.Measurements

''' <summary>This interface represents a protocol independent representation of any kind of data value.</summary>
<CLSCompliant(False)> _
Public Interface IChannelValue(Of T As IChannelDefinition)

    Inherits IChannel, ISerializable

    ReadOnly Property Parent() As IDataCell

    Property Definition() As T

    ReadOnly Property DataFormat() As DataFormat

    ReadOnly Property Label() As String

    ''' <summary>Composite measurements of channel value</summary>
    ''' <remarks>
    ''' Because derived value classes may consist of more than one measured value,
    ''' we use the composite value properties to abstractly expose each value
    ''' </remarks>
    Default Property CompositeValue(ByVal index As Integer) As Single

    ''' <summary>Total number of composite measurements exposed by the channel value</summary>
    ''' <remarks>
    ''' Because derived value classes may consist of more than one measured value,
    ''' we use the composite value properties to abstractly expose each value
    ''' </remarks>
    ReadOnly Property CompositeValueCount() As Integer

    ReadOnly Property IsEmpty() As Boolean

    ReadOnly Property Measurements() As IMeasurement()

End Interface

