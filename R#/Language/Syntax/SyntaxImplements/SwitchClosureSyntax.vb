#Region "Microsoft.VisualBasic::634b9ff803b059f0e05b34bb831afc16, R#\Language\Syntax\SyntaxImplements\SwitchClosureSyntax.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 80
    '    Code Lines: 68
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 3.79 KB


    '     Module SwitchClosureSyntax
    ' 
    '         Function: GetSwitchs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

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
                Else
                    Call opts.SetCurrentRange(input(2))
                End If

                If Not TypeOf [default].expression Is DeclareLambdaFunction Then
                    Return SyntaxResult.CreateError(New Exception("invalid expression type for the default value of the switch expression!"), opts)
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

            For Each opt As Token() In From line In options Where line.Length > 1
                Dim optVal = ExpressionBuilder.ParseExpression(New List(Of Token()) From {opt}, opts)

                If optVal.isException Then
                    Return optVal
                ElseIf Not TypeOf optVal.expression Is ValueAssignExpression Then
                    Return SyntaxResult.CreateError(
                        err:=New Exception("invalid expression for switch options!"),
                        opts:=opts.SetCurrentRange(opt)
                    )
                Else
                    With DirectCast(optVal.expression, ValueAssignExpression)
                        Dim keyStr As [Variant](Of String(), Exception)

                        If TypeOf .targetSymbols(Scan0) Is Literal Then
                            keyStr = {DirectCast(.targetSymbols(Scan0), Literal).ValueStr}
                        Else
                            keyStr = FormulaExpression.GetSymbols(.targetSymbols(Scan0))
                        End If

                        If keyStr Like GetType(Exception) Then
                            Return SyntaxResult.CreateError(keyStr, opts.SetCurrentRange(opt))
                        Else
                            Call switch.Add(keyStr.TryCast(Of String())()(0), .value)
                        End If
                    End With
                End If
            Next

            Return switch
        End Function
    End Module
End Namespace
