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

Imports Tva.Measurements

' This interface represents a protocol independent representation of any kind of data value.
<CLSCompliant(False)> _
Public Interface IChannelValue(Of T As IChannelDefinition)

    Inherits IChannel

    ReadOnly Property Parent() As IDataCell

    Property Definition() As T

    ReadOnly Property DataFormat() As DataFormat

    ''' <summary>Composite measurements of channel value</summary>
    ''' <remarks>
    ''' Because derived classes may consist of more than one measured value,
    ''' we use this property to abstractly expose each value
    ''' </remarks>
    Default Property CompositeValue(ByVal index As Integer) As Single

    ReadOnly Property CompositeValueCount() As Integer

    ReadOnly Property IsEmpty() As Boolean

    ReadOnly Property Measurements() As IMeasurement()

End Interface

