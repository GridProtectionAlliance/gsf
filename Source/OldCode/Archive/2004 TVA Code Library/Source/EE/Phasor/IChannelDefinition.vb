'***********************************************************************
'  IChannelDefinition.vb - Channel data definition interface
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    ' This interface represents a protocol independent definition of any kind of data.
    Public Interface IChannelDefinition

        Inherits IComparable

        ReadOnly Property InheritedType() As Type

        ReadOnly Property This() As IChannelDefinition

        Property Index() As Integer

        ' Offset provided for end-implementation convenience, virtual base classes should not apply offset to any values
        Property Offset() As Double

        Property ScalingFactor() As Integer

        ReadOnly Property MaximumScalingFactor() As Integer

        Property Label() As String

        ReadOnly Property LabelImage() As Byte()

        ReadOnly Property MaximumLabelLength() As Integer

        ReadOnly Property BinaryLength() As Integer

        ReadOnly Property BinaryImage() As Byte()

    End Interface


End Namespace
