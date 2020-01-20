Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareLambdaFunctionSyntax

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        ''' <param name="tokens"></param>
        Public Function DeclareLambdaFunction(tokens As List(Of Token())) As SyntaxResult
            With tokens.ToArray
                Dim name = .IteratesALL _
                           .Select(Function(t) t.text) _
                           .JoinBy(" ") _
                           .DoCall(Function(exp)
                                       Return "[lambda: " & exp & "]"
                                   End Function)
                Dim parameter As SyntaxResult = SyntaxImplements.DeclareNewVariable(tokens(Scan0))
                Dim closure As SyntaxResult = .Skip(2) _
                                              .IteratesALL _
                                              .DoCall(AddressOf Expression.CreateExpression)

                If parameter.isException Then
                    Return parameter
                ElseIf closure.isException Then
                    Return closure
                Else
                    Return New SyntaxResult(New DeclareLambdaFunction(name, parameter.expression, closure.expression))
                End If
            End With
        End Function
    End Module
End Namespace