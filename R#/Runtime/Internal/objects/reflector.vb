Imports System.Text
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal

    Module reflector

        Public Function GetStructure(x As Object, env As GlobalEnvironment) As String
            If x Is Nothing Then
                Return "<NULL>"
            End If

            Dim type As Type = x.GetType
            Dim code As TypeCodes = type.GetRTypeCode

            If type.ImplementInterface(GetType(IDictionary)) Then
                Dim list As IDictionary = x
                Dim value As Object
                Dim sb As New StringBuilder

                Call sb.AppendLine("List of " & list.Count)

                For Each slotKey As Object In list.Keys
                    value = list(slotKey)
                    sb.AppendLine($" $ {slotKey}: {GetStructure(value, env)}")
                Next

                Return sb.ToString
            ElseIf Runtime.IsPrimitive(code, includeComplexList:=False) Then
                Return strVector(Runtime.asVector(Of Object)(x), type, env)
            Else
                Return classPrinter.printClass(x)
            End If
        End Function

        Private Function strVector(a As Array, type As Type, env As GlobalEnvironment) As String
            Dim typeCode$

            If type Like BinaryExpression.integers Then
                typeCode = "int"
            ElseIf type Like BinaryExpression.characters Then
                typeCode = "chr"
            ElseIf type Like BinaryExpression.floats Then
                typeCode = "num"
            Else
                typeCode = "logical"
            End If

            If a.Length = 1 Then
                Return $"{typeCode} {printer.ValueToString(a.GetValue(Scan0), env)}"
            Else
                Return $"{typeCode} [1:{a.Length}] {printer.getStrings(a, env).Take(6).JoinBy(vbTab)}"
            End If
        End Function
    End Module
End Namespace