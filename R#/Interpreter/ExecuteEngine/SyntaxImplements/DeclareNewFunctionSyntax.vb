Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module DeclareNewFunctionSyntax

        Public Function DeclareNewFunction(code As List(Of Token())) As SyntaxResult
            Dim [declare] As Token() = code(4)
            Dim parts As List(Of Token()) = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart As Token() = parts(Scan0).Skip(1).ToArray
            Dim bodyPart As SyntaxResult = parts(2).Skip(1) _
                .ToArray _
                .DoCall(AddressOf SyntaxImplements.ClosureExpression)

            If bodyPart.isException Then
                Return bodyPart
            End If

            Dim funcName As String = code(1)(Scan0).text
            Dim params As New List(Of DeclareNewVariable)
            Dim err As SyntaxResult = getParameters(paramPart, params)

            If err.isException Then
                Return err
            Else
                Return New DeclareNewFunction With {
                    .funcName = funcName,
                    .body = bodyPart.expression,
                    .envir = Nothing,
                    .params = params.ToArray
                }
            End If
        End Function

        Private Function getParameters(tokens As Token(), ByRef params As List(Of DeclareNewVariable)) As SyntaxResult
            Dim parts As Token()() = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            For Each syntaxTemp As SyntaxResult In parts _
                .Select(Function(t)
                            Dim [let] As New List(Of Token) From {
                                New Token With {.name = TokenType.keyword, .text = "let"}
                            }
                            Return SyntaxImplements.DeclareNewVariable([let] + t)
                        End Function)

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    params.Add(syntaxTemp.expression)
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace