'***********************************************************************
'  DigitalDefinitionBase.vb - Digital value definition base class
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

Imports System.ComponentModel

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a digital value.
    Public MustInherit Class DigitalDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IDigitalDefinition

        Protected Sub New()

            MyBase.New()

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String)

            MyBase.New(index, label, 1.0)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Property ScalingFactor() As Double
            Get
                Return m_scale
            End Get
            Set(ByVal Value As Double)
                Throw New NotImplementedException("Digital values are not scaled")
            End Set
        End Property

    End Class

End Namespace
