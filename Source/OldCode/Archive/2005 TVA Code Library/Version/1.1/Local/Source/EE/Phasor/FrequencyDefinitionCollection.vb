'*******************************************************************************************************
'  FrequencyDefinitionCollection.vb - Frequency and df/dt value definition collection class
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

    ' This class represents the common implementation collection of the protocol independent definitions of frequency and df/dt values.
    Public Class FrequencyDefinitionCollection

        Inherits ChannelDefinitionCollectionBase

        Public Sub New(ByVal maximumCount As Integer)

            MyBase.New(maximumCount)

        End Sub

        Public Shadows Sub Add(ByVal value As IFrequencyDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IFrequencyDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property InheritedType() As Type
            Get
                Return Me.GetType()
            End Get
        End Property

    End Class

End Namespace