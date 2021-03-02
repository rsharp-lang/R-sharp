Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Bzip2

Public Class Reader

    Public Function OpenFile(file As Stream) As Reader
        Dim bz2 As New BZip2InputStream(inputStream:=file, headerless:=True)

    End Function
End Class
