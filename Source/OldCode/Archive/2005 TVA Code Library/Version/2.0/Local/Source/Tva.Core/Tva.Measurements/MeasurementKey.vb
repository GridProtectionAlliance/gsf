'*******************************************************************************************************
'  TVA.Measurements.MeasurementKey.vb - Defines primary key elements for a measurement
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

Imports TVA.Common
Imports TVA.Text.Common

Namespace Measurements

    ''' <summary>Defines a primary key for a measurement</summary>
    Public Structure MeasurementKey

        Implements IEquatable(Of MeasurementKey), IComparable(Of MeasurementKey), IComparable

        Public ID As Integer
        Public Source As String

        Public Sub New(ByVal id As Integer, ByVal source As String)

            Me.ID = id
            Me.Source = source

        End Sub

        Public Overrides Function ToString() As String

            Return Source & ":" & ID.ToString()

        End Function

        Public Overrides Function GetHashCode() As Integer

            ' 1234567890
            ' 2147483647
            Return Concat(Source, ID.ToString().PadLeft(10, "0"c)).GetHashCode()

        End Function

        Public Overloads Function Equals(ByVal other As MeasurementKey) As Boolean Implements System.IEquatable(Of MeasurementKey).Equals

            Return (ID = other.ID AndAlso String.Compare(Source, other.Source, True) = 0)

        End Function

        Public Function CompareTo(ByVal other As MeasurementKey) As Integer Implements System.IComparable(Of MeasurementKey).CompareTo

            Dim sourceCompare As Integer = String.Compare(Source, other.Source, True)

            If sourceCompare = 0 Then
                Return IIf(ID < other.ID, -1, IIf(ID > other.ID, 1, 0))
            Else
                Return sourceCompare
            End If

        End Function

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is MeasurementKey Then Return CompareTo(DirectCast(obj, MeasurementKey))
            Throw New ArgumentException("Object is not a MeasurementKey")

        End Function

    End Structure

End Namespace