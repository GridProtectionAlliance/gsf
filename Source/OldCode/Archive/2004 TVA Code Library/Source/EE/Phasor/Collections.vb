'***********************************************************************
'  Collections.vb - Collection definitions for phasor classes
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

    Public MustInherit Class ChannelFrameCollection

        Inherits CollectionBase

        Protected Sub _Add(ByVal value As IChannelFrame)

            List.Add(value)

        End Sub

        Protected ReadOnly Property _Item(ByVal index As Integer) As IChannelFrame
            Get
                Return DirectCast(List.Item(index), IChannelFrame)
            End Get
        End Property

    End Class

    Public Class PhasorDataFrameCollection

        Inherits ChannelFrameCollection

        Public Sub Add(ByVal value As IPhasorDataFrame)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorDataFrame
            Get
                Return DirectCast(MyBase._Item(index), IPhasorDataFrame)
            End Get
        End Property

    End Class

    Public MustInherit Class ChannelValueCollection

        Inherits CollectionBase

        Protected Sub _Add(ByVal value As IChannelValue)

            List.Add(value)

        End Sub

        Protected ReadOnly Property _Item(ByVal index As Integer) As IChannelValue
            Get
                Return DirectCast(List.Item(index), IChannelValue)
            End Get
        End Property

    End Class

    Public Class AnalogValueCollection

        Inherits ChannelValueCollection

        Public Sub Add(ByVal value As IAnalogValue)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IAnalogValue
            Get
                Return DirectCast(MyBase._Item(index), IAnalogValue)
            End Get
        End Property

    End Class

    Public Class DigitalValueCollection

        Inherits ChannelValueCollection

        Public Sub Add(ByVal value As IDigitalValue)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IDigitalValue
            Get
                Return DirectCast(MyBase._Item(index), IDigitalValue)
            End Get
        End Property

    End Class

    Public Class FrequencyValueCollection

        Inherits ChannelValueCollection

        Public Sub Add(ByVal value As IFrequencyValue)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IFrequencyValue
            Get
                Return DirectCast(MyBase._Item(index), IFrequencyValue)
            End Get
        End Property

    End Class

    Public Class PhasorValueCollection

        Inherits ChannelValueCollection

        Public Sub Add(ByVal value As IPhasorValue)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorValue
            Get
                Return DirectCast(MyBase._Item(index), IPhasorValue)
            End Get
        End Property

    End Class

    Public MustInherit Class ChannelDefinitionCollection

        Inherits CollectionBase

        Protected Sub _Add(ByVal value As IChannelDefinition)

            List.Add(value)

        End Sub

        Protected ReadOnly Property _Item(ByVal index As Integer) As IChannelDefinition
            Get
                Return DirectCast(List.Item(index), IChannelDefinition)
            End Get
        End Property

        Public Sub Sort()

            Array.Sort(List)

        End Sub

    End Class

    Public Class AnalogDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Sub Add(ByVal value As IAnalogDefinition)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IAnalogDefinition
            Get
                Return DirectCast(MyBase._Item(index), IAnalogDefinition)
            End Get
        End Property

    End Class

    Public Class DigitalDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Sub Add(ByVal value As IDigitalDefinition)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IDigitalDefinition
            Get
                Return DirectCast(MyBase._Item(index), IDigitalDefinition)
            End Get
        End Property

    End Class

    Public Class FrequencyDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Sub Add(ByVal value As IFrequencyDefinition)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IFrequencyDefinition
            Get
                Return DirectCast(MyBase._Item(index), IFrequencyDefinition)
            End Get
        End Property

    End Class

    Public Class PhasorDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Sub Add(ByVal value As IPhasorDefinition)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorDefinition
            Get
                Return DirectCast(MyBase._Item(index), IPhasorDefinition)
            End Get
        End Property

    End Class

End Namespace