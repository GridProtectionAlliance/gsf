<Serializable()> _
Public Class PDCDescriptorUnit

    Inherits EventArgs

    Public AnalogNames As String()
    Public DigitalNames As String()

End Class

<Serializable()> _
Public Class PDCDataUnit

    Inherits EventArgs

    Public AnalogData As Double(,)
    Public DigitalData As UInt16(,)

End Class

<Serializable()> _
Public Class PDCErrorUnit

    Inherits EventArgs

    Public Number As Short
    Public SCode As Integer
    Public Source As String
    Public Description As String

End Class