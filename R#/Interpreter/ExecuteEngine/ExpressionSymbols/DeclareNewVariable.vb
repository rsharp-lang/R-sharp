Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class DeclareNewVariable : Inherits Expression

        ''' <summary>
        ''' 对于tuple类型，会存在多个变量
        ''' </summary>
        Dim names As String()
        Dim value As Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(code As List(Of Token()))
            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            Call getNames(code(1))

            If code.Count = 2 Then
                type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    Call code.Skip(5).DoCall(AddressOf getInitializeValue)
                End If
            Else
                type = TypeCodes.generic

                If code.Count > 2 AndAlso code(2).isOperator("=", "<-") Then
                    Call code.Skip(3).DoCall(AddressOf getInitializeValue)
                End If
            End If
        End Sub

        Private Sub getNames(code As Token())
            If code.Length > 1 Then
                names = code.Skip(1) _
                    .Take(code.Length - 2) _
                    .Where(Function(token) Not token.name = TokenType.comma) _
                    .Select(Function(symbol)
                                If symbol.name <> TokenType.identifier Then
                                    Throw New SyntaxErrorException
                                Else
                                    Return symbol.text
                                End If
                            End Function) _
                    .ToArray
            Else
                names = {code(Scan0).text}
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub getInitializeValue(code As IEnumerable(Of Token()))
            value = Expression.CreateExpression(code.AsList)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object

            If Me.value Is Nothing Then
                value = Nothing
            Else
                value = Me.value.Evaluate(envir)
            End If

            If names.Length = 1 Then
                Return envir.Push(names(Scan0), value, type)
            Else
                ' tuple
                Throw New NotImplementedException
            End If
        End Function

        Public Overrides Function ToString() As String
            If names.Length > 1 Then
                Return $"Dim [{names.JoinBy(", ")}] As {type.Description} = {value.ToString}"
            Else
                Return $"Dim {names(Scan0)} As {type.Description} = {value.ToString}"
            End If
        End Function
    End Class
End Namespace