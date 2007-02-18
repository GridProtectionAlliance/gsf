' 02/18/2007

Imports System.Drawing
Imports System.ComponentModel

<ToolboxBitmap(GetType(ArchiveFile))> _
Public Class ArchiveFile

    <Browsable(False)> _
    Public ReadOnly Property FAT() As ArchiveFileAllocationTable
        Get

        End Get
    End Property

End Class
