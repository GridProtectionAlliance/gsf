' 03/28/2007

Imports System.Drawing
Imports System.ComponentModel

Namespace Packets

    <ToolboxBitmap(GetType(DwPacketParser))> _
    Public Class DwPacketParser

        Public Overrides Function GetID(ByVal binaryImage() As Byte) As Short

            Return BitConverter.ToInt16(binaryImage, 0)

        End Function

    End Class

End Namespace