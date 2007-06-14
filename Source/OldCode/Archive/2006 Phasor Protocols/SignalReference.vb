Imports TVA.Common

Public Structure SignalReference

    Implements IEquatable(Of SignalReference), IComparable(Of SignalReference), IComparable

    Public PmuAcronym As String
    Public PmuCellIndex As Integer
    Public SignalType As SignalType
    Public SignalIndex As Integer

    ' Parse signal reference
    Public Sub New(ByVal signalReference As String)

        Dim elements As String() = signalReference.Trim().Split("-"c)
        Dim signal As String

        PmuAcronym = elements(0).Trim().ToUpper()

        ' If the length of the signal type acronym is greater than 2, then this
        ' is an indexed signal type (e.g., CORD-PA2)
        If elements(1).Length > 2 Then
            signal = elements(1).Substring(0, 2).Trim().ToUpper()
            SignalIndex = Convert.ToInt32(elements(1).Substring(2))
        Else
            signal = elements(1).Trim().ToUpper()
            SignalIndex = 0
        End If

        Me.SignalType = GetSignalType(signal)

    End Sub

    Public Shared Function GetSignalType(ByVal acronym As String) As SignalType

        Select Case acronym
            Case "PA"
                Return SignalType.Angle
            Case "PM"
                Return SignalType.Magnitude
            Case "FQ"
                Return SignalType.Frequency
            Case "DF"
                Return SignalType.dfdt
            Case "SF"
                Return SignalType.Status
            Case "DV"
                Return SignalType.Digital
            Case Else
                Return SignalType.Unknown
        End Select

    End Function

    Public Shared Function GetSignalTypeAcronym(ByVal signal As SignalType) As String

        Select Case signal
            Case SignalType.Angle
                Return "PA"
            Case SignalType.Magnitude
                Return "PM"
            Case SignalType.Frequency
                Return "FQ"
            Case SignalType.dfdt
                Return "DF"
            Case SignalType.Status
                Return "SF"
            Case SignalType.Digital
                Return "DV"
            Case Else
                Return "??"
        End Select

    End Function

    Public Overloads Shared Function ToString(ByVal pmuAcronym As String, ByVal signal As SignalType) As String

        Return ToString(pmuAcronym, signal, 0)

    End Function

    Public Overloads Shared Function ToString(ByVal pmuAcronym As String, ByVal signal As SignalType, ByVal signalIndex As Integer) As String

        If signalIndex > 0 Then
            Return String.Format("{0}-{1}{2}", pmuAcronym, GetSignalTypeAcronym(signal), signalIndex)
        Else
            Return String.Format("{0}-{1}", pmuAcronym, GetSignalTypeAcronym(signal))
        End If

    End Function

    Public Overrides Function ToString() As String

        Return ToString(Me.PmuAcronym, Me.SignalType, Me.SignalIndex)

    End Function

    Public Overrides Function GetHashCode() As Integer

        Return Me.ToString().GetHashCode()

    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean

        If TypeOf obj Is SignalReference Then Return Equals(DirectCast(obj, SignalReference))
        Throw New ArgumentException("Object is not a SignalReference")

    End Function

    Public Overloads Function Equals(ByVal other As SignalReference) As Boolean Implements System.IEquatable(Of SignalReference).Equals

        Return (String.Compare(Me.PmuAcronym, other.PmuAcronym, True) = 0 AndAlso Me.SignalType = other.SignalType AndAlso Me.SignalIndex = other.SignalIndex)

    End Function

    Public Function CompareTo(ByVal other As SignalReference) As Integer Implements System.IComparable(Of SignalReference).CompareTo

        Dim acronymCompare As Integer = String.Compare(Me.PmuAcronym, other.PmuAcronym, True)

        If acronymCompare = 0 Then
            Dim signalTypeCompare As Integer = IIf(Me.SignalType < other.SignalType, -1, IIf(Me.SignalType > other.SignalType, 1, 0))

            If signalTypeCompare = 0 Then
                Return Me.SignalIndex.CompareTo(other.SignalIndex)
            Else
                Return signalTypeCompare
            End If
        Else
            Return acronymCompare
        End If

    End Function

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is SignalReference Then Return CompareTo(DirectCast(obj, SignalReference))
        Throw New ArgumentException("Object is not a SignalReference")

    End Function

End Structure
