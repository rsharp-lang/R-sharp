Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module SwitchClosureSyntax

        Public Function GetSwitchs(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim input = code(Scan0).Skip(2).Take(code(Scan0).Length - 2).SplitByTopLevelDelimiter(TokenType.comma)
            Dim keyVal = ExpressionBuilder.ParseExpression(New List(Of Token()) From {input(Scan0)}, opts)
            Dim stackframe As StackFrame = opts.GetStackTrace(tokens(Scan0), "switch")
            Dim [default] As SyntaxResult

            If keyVal.isException Then
                Return keyVal
            ElseIf input = 3 Then
                [default] = ExpressionBuilder.ParseExpression(New List(Of Token()) From {input(2)}, opts)

                If [default].isException Then
                    Return [default]
                End If

                If Not TypeOf [default].expression Is DeclareLambdaFunction Then
                    Return New SyntaxResult(New Exception("invalid expression type for the default value of the switch expression!"), opts.debug)
                Else
                    With DirectCast([default].expression, DeclareLambdaFunction)
                        [default] = New SyntaxResult(.closure)
                    End With
                End If
            Else
                [default] = Nothing
            End If

            Dim switch As New SwitchExpression(keyVal.expression, [default]?.expression, stackframe)
            Dim options = code(2) _
                .Skip(1) _
                .Take(code(2).Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)

            For Each opt In From line In options Where line.Length > 1
                Dim optVal = ExpressionBuilder.ParseExpression(New List(Of Token()) From {opt}, opts)

                If optVal.isException Then
                    Return optVal
                ElseIf Not TypeOf optVal.expression Is ValueAssignExpression Then
                    Return New SyntaxResult(New Exception("invalid expression for switch options!"), opts.debug)
                Else
                    With DirectCast(optVal.expression, ValueAssignExpression)
                        Dim keyStr = FormulaExpression.GetSymbols(.targetSymbols(Scan0))

                        If keyStr Like GetType(Exception) Then
                            Return New SyntaxResult(keyStr, opts.debug)
                        Else
                            switch.Add(keyStr.TryCast(Of String())()(0), .value)
                        End If
                    End With
                End If
            Next

            Return switch
        End Function
    End Module
End Namespace