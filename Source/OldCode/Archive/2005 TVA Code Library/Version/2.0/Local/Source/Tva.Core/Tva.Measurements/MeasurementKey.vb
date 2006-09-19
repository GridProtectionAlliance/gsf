'*******************************************************************************************************
'  Tva.Measurements.MeasurementKey.vb - Defines primary key elements for a measurement
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Text.Common

Namespace Measurements

    ''' <summary>Defines a primary key for a measurement</summary>
    Public Structure MeasurementKey

        Implements IEquatable(Of MeasurementKey)

        Public ID As Integer
        Public Source As String

        Public Sub New(ByVal id As Integer, ByVal source As String)

            Me.ID = id
            Me.Source = source

        End Sub

        Public Overrides Function ToString() As String

            Return Source & ":" & ID

        End Function

        Public Overrides Function GetHashCode() As Integer

            ' 1234567890
            ' 2147483647
            Return Concat(Source, ID.ToString().PadLeft(10, "0"c)).GetHashCode()

        End Function

        Public Overloads Function Equals(ByVal other As MeasurementKey) As Boolean Implements System.IEquatable(Of MeasurementKey).Equals

            Return (GetHashCode() = other.GetHashCode())

        End Function

    End Structure

End Namespace