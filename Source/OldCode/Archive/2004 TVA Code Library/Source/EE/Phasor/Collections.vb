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

        Public Sub Add(ByVal value As IChannel)

            List.Add(value)

        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As IChannel
            Get
                Return List.Item(index)
            End Get
        End Property

        Public Overridable ReadOnly Property BinaryLength() As Integer
            Get
                If List.Count > 0 Then
                    Return Item(0).BinaryLength * List.Count
                Else
                    Return 0
                End If
            End Get
        End Property

    End Class

    Public MustInherit Class ChannelValueCollection

        Inherits ChannelCollection

        Private m_fixedCount As Integer
        Private m_floatCount As Integer

        Public Shadows Sub Add(ByVal value As IChannelValue)

            ' In typical usage, all channel values will be of the same data type - but we can't anticipate all
            ' possible uses of collection, so we track totals of each data type so we can quickly ascertain if
            ' all the items in the collection are of the same data type
            If value.DataFormat = DataFormat.FixedInteger Then
                m_fixedCount += 1
            Else
                m_floatCount += 1
            End If

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                If List.Count > 0 Then
                    If m_fixedCount = 0 Or m_floatCount = 0 Then
                        ' Data types in list are consistent, a simple calculation will derive total binary length
                        Return Item(0).BinaryLength * List.Count
                    Else
                        ' List has items of different data types, will have to traverse list to calculate total binary length
                        Dim length As Integer

                        For x As Integer = 0 To List.Count - 1
                            length += Item(0).BinaryLength
                        Next

                        Return length
                    End If
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Dim empty As Boolean = True

                For x As Integer = 0 To List.Count - 1
                    If Not Item(x).IsEmpty Then
                        empty = False
                        Exit For
                    End If
                Next

                Return empty
            End Get
        End Property

        Protected Overrides Sub OnClearComplete()

            m_fixedCount = 0
            m_floatCount = 0

        End Sub

    End Class

    Public MustInherit Class ChannelDefinitionCollection

        Inherits ChannelCollection

        Public Shadows Sub Add(ByVal value As IChannelDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public MustInherit Class ChannelCellCollection

        Inherits ChannelCollection

        Public Shadows Sub Add(ByVal value As IChannelCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public MustInherit Class ChannelFrameCollection

        Inherits ChannelCollection

        Public Shadows Sub Add(ByVal value As IChannelFrame)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelFrame
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class ConfigurationCellCollection

        Inherits ChannelCellCollection

        Public Shadows Sub Add(ByVal value As IConfigurationCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IConfigurationCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class DataCellCollection

        Inherits ChannelCellCollection

        Public Shadows Sub Add(ByVal value As IDataCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IDataCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class ConfigurationFrameCollection

        Inherits ChannelFrameCollection

        Public Shadows Sub Add(ByVal value As IConfigurationFrame)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IConfigurationFrame
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class DataFrameCollection

        Inherits ChannelFrameCollection

        Public Shadows Sub Add(ByVal value As IDataFrame)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IDataFrame
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class AnalogValueCollection

        Inherits ChannelValueCollection

        Public Shadows Sub Add(ByVal value As IAnalogValue)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IAnalogValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class DigitalValueCollection

        Inherits ChannelValueCollection

        Public Shadows Sub Add(ByVal value As IDigitalValue)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IDigitalValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class FrequencyValueCollection

        Inherits ChannelValueCollection

        Public Shadows Sub Add(ByVal value As IFrequencyValue)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IFrequencyValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class PhasorValueCollection

        Inherits ChannelValueCollection

        Public Shadows Sub Add(ByVal value As IPhasorValue)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IPhasorValue
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class AnalogDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Shadows Sub Add(ByVal value As IAnalogDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IAnalogDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class DigitalDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Shadows Sub Add(ByVal value As IDigitalDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IDigitalDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class FrequencyDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Shadows Sub Add(ByVal value As IFrequencyDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IFrequencyDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

    Public Class PhasorDefinitionCollection

        Inherits ChannelDefinitionCollection

        Public Shadows Sub Add(ByVal value As IPhasorDefinition)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IPhasorDefinition
            Get
                Return MyBase.Item(index)
            End Get
        End Property

    End Class

End Namespace