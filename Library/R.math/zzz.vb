Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SMRUCC.Rsharp.Runtime.Internal

Public Class zzz

    Public Shared Sub onLoad()
        Call ConsolePrinter.AttachConsoleFormatter(Of GeneralMatrix)(AddressOf printMatrix)
    End Sub

    Private Shared Function printMatrix(m As GeneralMatrix) As String
        Dim strMat As String()() = m _
            .RowVectors _
            .Select(Function(r)
                        Return r.Select(Function(d) d.ToString("F4")).ToArray
                    End Function) _
            .ToArray
        Dim text As New StringBuilder
        Dim i As New Uid(0, Uid.AlphabetUCase)
        Dim titles As String() = strMat(Scan0) _
            .Select(Function(null) ++i) _
            .ToArray

        Using writer As New StringWriter(text)
            Call PrintAsTable.PrintTable(strMat, writer, " ", title:=titles, trilinearTable:=True)
            Call writer.Flush()
        End Using

        Return text.ToString
    End Function
End Class
