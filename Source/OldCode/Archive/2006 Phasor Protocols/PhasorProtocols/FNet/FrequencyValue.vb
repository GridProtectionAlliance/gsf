'*******************************************************************************************************
'  FrequencyValue.vb - FNet Frequency value
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
'  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

Namespace FNet

    <CLSCompliant(False), Serializable()> _
    Public Class FrequencyValue

        Inherits FrequencyValueBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)

            MyBase.New(parent, frequencyDefinition, frequency, dfdt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

            MyBase.New(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequencyValue As IFrequencyValue)

            MyBase.New(parent, frequencyDefinition, frequencyValue)

        End Sub

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As DataCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Shadows Property Definition() As FrequencyDefinition
            Get
                Return MyBase.Definition
            End Get
            Set(ByVal value As FrequencyDefinition)
                MyBase.Definition = value
            End Set
        End Property

    End Class

End Namespace