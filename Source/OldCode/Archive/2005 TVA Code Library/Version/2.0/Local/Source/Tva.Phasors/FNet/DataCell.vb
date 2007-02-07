'*******************************************************************************************************
'  DataCell.vb - IEEE 1344 PMU Data Cell
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
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.Phasors.FNet.Common

Namespace FNet

    ' This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
    <CLSCompliant(False), Serializable()> _
    Public Class DataCell

        Inherits DataCellBase

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal parent As IDataFrame, ByVal configurationCell As IConfigurationCell)

            MyBase.New(parent, False, configurationCell, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues)

            Dim x As Int32

            With configurationCell
                ' Initialize phasor values and frequency value with an empty value
                For x = 0 To .PhasorDefinitions.Count - 1
                    PhasorValues.Add(New PhasorValue(Me, .PhasorDefinitions(x), 0, 0))
                Next

                ' Initialize frequency and df/dt
                FrequencyValue = New FrequencyValue(Me, .FrequencyDefinition, 0, 0)

                ' Initialize any digital values
                For x = 0 To .DigitalDefinitions.Count - 1
                    DigitalValues.Add(New DigitalValue(Me, .DigitalDefinitions(x), 0))
                Next
            End With

        End Sub

        Public Sub New(ByVal dataCell As IDataCell)

            MyBase.New(dataCell)

        End Sub

        Public Sub New(ByVal parent As IDataFrame, ByVal state As DataFrameParsingState, ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
                New DataCellParsingState(state.ConfigurationFrame.Cells(index), _
                    AddressOf FNet.PhasorValue.CreateNewPhasorValue, _
                    AddressOf FNet.FrequencyValue.CreateNewFrequencyValue, _
                    Nothing, _
                    AddressOf FNet.DigitalValue.CreateNewDigitalValue), _
                binaryImage, startIndex)

        End Sub

        Friend Shared Function CreateNewDataCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IDataCell), ByVal index As Int32, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IDataCell

            Return New DataCell(parent, state, index, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        ' TODO: May want to shadow all parents in final derived classes - also go through code and make sure all MustInherit class properties are overridable
        Public Shadows ReadOnly Property Parent() As DataFrame
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overrides Property SynchronizationIsValid() As Boolean
            Get
                Return (StatusFlags And Bit15) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    StatusFlags = StatusFlags And Not Bit15
                Else
                    StatusFlags = StatusFlags Or Bit15
                End If
            End Set
        End Property

        Public Overrides Property DataIsValid() As Boolean
            Get
                Return (StatusFlags And Bit14) = 0
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    StatusFlags = StatusFlags And Not Bit14
                Else
                    StatusFlags = StatusFlags Or Bit14
                End If
            End Set
        End Property

        Public Property TriggerStatus() As TriggerStatus
            Get
                Return StatusFlags And TriggerMask
            End Get
            Set(ByVal value As TriggerStatus)
                StatusFlags = (StatusFlags And Not TriggerMask) Or value
            End Set
        End Property

    End Class

End Namespace