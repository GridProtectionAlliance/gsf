'*******************************************************************************************************
'  ConfigurationCellCollection.vb - Configuration cell collection class
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
'  01/14/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

''' <summary>This class represents the protocol independent collection of the common implementation of a set of configuration related data settings that can be sent or received from a PMU.</summary>
<CLSCompliant(False), Serializable()> _
Public Class ConfigurationCellCollection

    Inherits ChannelCellCollectionBase(Of IConfigurationCell)

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal maximumCount As Int32, ByVal constantCellLength As Boolean)

        MyBase.New(maximumCount, constantCellLength)

    End Sub

    Public Overrides ReadOnly Property DerivedType() As Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public Overridable Function TryGetByIDLabel(ByVal label As String, ByRef configurationCell As IConfigurationCell) As Boolean

        For Each configurationCell In Me
            If String.Compare(configurationCell.IDLabel, label, True) = 0 Then
                Return True
            End If
        Next

        configurationCell = Nothing
        Return False

    End Function

    Public Overridable Function IndexOfIDLabel(ByVal label As String) As Integer

        For index As Integer = 0 To Count - 1
            If String.Compare(Me(index).IDLabel, label, True) = 0 Then
                Return index
            End If
        Next

        Return -1

    End Function

End Class