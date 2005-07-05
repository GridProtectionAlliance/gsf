'*******************************************************************************************************
'  IChannelDefinition.vb - Channel data definition interface
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
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor

    ' This interface represents a protocol independent definition of any kind of data.
    Public Interface IChannelDefinition

        Inherits IChannel, IComparable

        ReadOnly Property Parent() As IConfigurationCell

        Property DataFormat() As DataFormat

        Property Index() As Integer

        Property Offset() As Double

        Property ScalingFactor() As Integer

        ReadOnly Property MaximumScalingFactor() As Integer

        Property Label() As String

        ReadOnly Property LabelImage() As Byte()

        ReadOnly Property MaximumLabelLength() As Integer

    End Interface

End Namespace
