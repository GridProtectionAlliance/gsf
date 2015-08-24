'*******************************************************************************************************
'  IChannelDefinition.vb - Channel data definition interface
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
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

''' <summary>This interface represents a protocol independent definition of any kind of data.</summary>
<CLSCompliant(False)> _
Public Interface IChannelDefinition

    Inherits IChannel, ISerializable, IEquatable(Of IChannelDefinition), IComparable(Of IChannelDefinition), IComparable

    ReadOnly Property Parent() As IConfigurationCell

    ReadOnly Property DataFormat() As DataFormat

    Property Index() As Int32

    Property Offset() As Single

    Property ScalingFactor() As Int32

    ReadOnly Property MaximumScalingFactor() As Int32

    Property ConversionFactor() As Single

    ReadOnly Property ScalePerBit() As Single

    Property Label() As String

    ReadOnly Property LabelImage() As Byte()

    ReadOnly Property MaximumLabelLength() As Int32

End Interface