'***********************************************************************
'  IPhasorDefinition.vb - Phasor definition interface
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

    Public Interface IPhasorDefinition

        Inherits IComparable

        Property Index() As Integer

        Property [Type]() As PhasorType

        Property VoltageReference() As IPhasorDefinition

        Property CalFactor() As Double

        Property Label() As String

        ReadOnly Property LabelImage() As Byte()

        ReadOnly Property MaximumCalFactor() As Int32

        ReadOnly Property MaximumLabelLength() As Integer

        ReadOnly Property BinaryLength() As Integer

        ReadOnly Property BinaryImage() As Byte()

    End Interface

End Namespace