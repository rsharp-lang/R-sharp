Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module Utils

    <Extension>
    Public Sub RemoveRange(stat As StackStates, buffer As List(Of SyntaxToken))
        Call buffer.RemoveRange(stat.Range.Min, stat.Range.Length)
    End Sub
End Module
