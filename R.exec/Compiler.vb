Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

''' <summary>
''' R# executable file format and script compiler
''' </summary>
Public Module Compiler

    Public Iterator Function Build(script As String) As IEnumerable(Of variable)
        Dim program As Program = Program.BuildProgram(script)
        Dim buffer As variable()
        Dim i32ptr As Integer = Scan0

        For Each line As RExpression In program
            Select Case line.GetType
                Case GetType(DeclareNewVariable)
                    buffer = DirectCast(line, DeclareNewVariable).DeclareNewVariable
            End Select
        Next
    End Function

    <Extension>
    Private Function DeclareNewVariable(declares As DeclareNewVariable) As variable()
        Dim [let] As New variable
        ' Dim names = declares.
    End Function
End Module
