'***********************************************************************
'  IDigitalDefinition.vb - Digital definition interface
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

    ' This class represents the protocol independent interface of a digital value definition.
    Public Interface IDigitalDefinition

        Inherits IComparable

        Property Index() As Integer

        ReadOnly Property MaximumLabelLength() As Integer

        Property Label() As String

        ReadOnly Property LabelImage() As Byte()

        ReadOnly Property BinaryLength() As Integer

        ReadOnly Property BinaryImage() As Byte()

    End Interface

End Namespace