'*******************************************************************************************************
'  Collections.vb - Collection definitions for PC37_118 classes
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/31/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor.PC37_118

    Public Class PhasorValues

        Inherits CollectionBase

        Friend Sub New()
        End Sub

        Public Sub Add(ByVal value As PhasorValue)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As PhasorValue
            Get
                Return DirectCast(List.Item(index), PhasorValue)
            End Get
        End Property

    End Class

    Public Class DigitalValues

        Inherits CollectionBase

        Friend Sub New()
        End Sub

        Public Sub Add(ByVal value As Int16)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As Int16
            Get
                Return DirectCast(List.Item(index), Int16)
            End Get
        End Property

    End Class

    Public Class PhasorDefinitions

        Inherits CollectionBase

        Friend Sub New()
        End Sub

        Public Sub Add(ByVal value As PhasorDefinition)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As PhasorDefinition
            Get
                Return DirectCast(List.Item(index), PhasorDefinition)
            End Get
        End Property

    End Class

    Public Class DigitalDefinitions

        Inherits CollectionBase

        Friend Sub New()
        End Sub

        Public Sub Add(ByVal value As DigitalDefinition)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As DigitalDefinition
            Get
                Return DirectCast(List.Item(index), DigitalDefinition)
            End Get
        End Property

    End Class

End Namespace