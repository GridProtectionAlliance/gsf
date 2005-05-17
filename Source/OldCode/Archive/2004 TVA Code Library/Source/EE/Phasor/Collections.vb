'***********************************************************************
'  Collections.vb - Collection definitions for phasor classes
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

    Public MustInherit Class ChannelCollection

        Inherits CollectionBase

        Protected Sub _Add(ByVal value As IChannel)

            List.Add(value)

        End Sub

        Protected ReadOnly Property _Item(ByVal index As Integer) As IChannel
            Get
                Return DirectCast(List.Item(index), IChannel)
            End Get
        End Property

    End Class

    Public MustInherit Class ChanneValueCollection

        Inherits ChannelCollection

        Private m_fixedCount As Integer
        Private m_floatCount As Integer

        Protected Overloads Sub _Add(ByVal value As IChannelValue)

            If value.DataFormat = DataFormat.FixedInteger Then
                m_fixedCount += 1
            Else
                m_floatCount += 1
            End If

            MyBase._Add(value)

        End Sub

        Public ReadOnly Property BinaryLength() As Integer
            Get
                If List.Count > 0 Then
                    If m_fixedCount = 0 Or m_floatCount = 0 Then
                        ' Data types in list are consistent, an easy calculation will derive total binary length
                        Return _Item(0).BinaryLength * List.Count
                    Else
                        ' List has items of different data types, will have to traverse list to calculate total binary length
                        Dim length As Integer

                        For x As Integer = 0 To List.Count - 1
                            length += _Item(0).BinaryLength
                        Next

                        Return length
                    End If
                Else
                    Return 0
                End If
            End Get
        End Property

        Protected Overrides Sub OnClearComplete()

            m_fixedCount = 0
            m_floatCount = 0

        End Sub

    End Class

    Public MustInherit Class ChannelDefinitionCollection

        Inherits ChannelCollection

        Public Sub Sort()

            Array.Sort(List)

        End Sub

    End Class

    Public Class PhasorDataCellCollection

        Inherits ChannelCollection

        Public Sub Add(ByVal value As IPhasorDataCell)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorDataCell
            Get
                Return DirectCast(MyBase._Item(index), IPhasorDataCell)
            End Get
        End Property

    End Class

    Public Class PhasorDataFrameCollection

        Inherits ChannelCollection

        Public Sub Add(ByVal value As IPhasorDataFrame)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorDataFrame
            Get
                Return DirectCast(MyBase._Item(index), IPhasorDataFrame)
            End Get
        End Property

    End Class

    Public Class AnalogValueCollection

        Inherits ChanneValueCollection

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

        Inherits ChanneValueCollection

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

        Inherits ChanneValueCollection

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

        Inherits ChanneValueCollection

        Public Sub Add(ByVal value As IPhasorValue)

            MyBase._Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IPhasorValue
            Get
                Return DirectCast(MyBase._Item(index), IPhasorValue)
            End Get
        End Property

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