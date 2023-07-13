#Region "Microsoft.VisualBasic::a44e7120ebf8f75d9afaf3d0602c16fe, G:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxImplements/DeclareLambdaFunctionSyntax.vb"

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

    '   Total Lines: 114
    '    Code Lines: 95
    ' Comment Lines: 7
    '   Blank Lines: 12
    '     File Size: 5.09 KB


    '     Module DeclareLambdaFunctionSyntax
    ' 
    '         Function: (+3 Overloads) DeclareLambdaFunction, GetSymbols
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module DeclareLambdaFunctionSyntax

        Private Function GetSymbols(exp As Expression) As [Variant](Of String(), Exception)
            If TypeOf exp Is SymbolReference Then
                Return {DirectCast(exp, SymbolReference).symbol}
            ElseIf TypeOf exp Is VectorLiteral Then
                Return DirectCast(exp, VectorLiteral) _
                    .Select(Function(a) ValueAssignExpression.GetSymbol(a)) _
                    .ToArray
            Else
                Return New InvalidExpressionException($"Can not extract symbols from expression: {exp.GetType.FullName}")
            End If
        End Function

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        Public Function DeclareLambdaFunction(args As Expression, expression As Expression, lineNum%, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim argSymbols As [Variant](Of String(), Exception) = GetSymbols(args)

            If Not argSymbols Like GetType(String()) Then
                Return New SyntaxResult(SyntaxError.CreateError(opts, argSymbols.TryCast(Of Exception)))
            End If

            Dim name$ = $"[lambda: {args} -> {expression}]"
            Dim stackframe As New StackFrame With {
                .File = opts.source.fileName,
                .Line = lineNum,
                .Method = New Method With {
                    .Method = name,
                    .[Module] = "n/a",
                    .[Namespace] = "SMRUCC/R#"
                }
            }
            Dim parameter As New DeclareNewSymbol(
                names:=argSymbols.TryCast(Of String()),
                value:=Nothing,
                type:=TypeCodes.generic,
                [readonly]:=False,
                stackFrame:=stackframe
            )
            Dim lambda As New DeclareLambdaFunction(
                name:=name,
                parameter:=parameter,
                closure:=expression,
                stackframe:=stackframe
            )

            Return New SyntaxResult(lambda)
        End Function

        Public Function DeclareLambdaFunction(name As String, body As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult

        End Function

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        ''' <param name="tokens"></param>
        Public Function DeclareLambdaFunction(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            With tokens.ToArray
                Dim name = .IteratesALL _
                           .Select(Function(t) t.text) _
                           .JoinBy(" ") _
                           .DoCall(Function(exp)
                                       Return "[lambda: " & exp & "]"
                                   End Function)
                Dim parameter As SyntaxResult = SyntaxImplements.DeclareNewSymbol(tokens(Scan0), opts)
                Dim closure As SyntaxResult = .Skip(2) _
                                              .IteratesALL _
                                              .DoCall(Function(code)
                                                          Return opts.ParseExpression(code, opts)
                                                      End Function)

                If parameter.isException Then
                    Return parameter
                ElseIf closure.isException Then
                    Return closure
                Else
                    Dim line = .First()(Scan0).span.line
                    Dim stackframe As New StackFrame With {
                        .File = opts.source.fileName,
                        .Line = line,
                        .Method = New Method With {
                            .Method = name,
                            .[Module] = "n/a",
                            .[Namespace] = "SMRUCC/R#"
                        }
                    }
                    Dim lambda As New DeclareLambdaFunction(
                        name:=name,
                        parameter:=parameter.expression,
                        closure:=closure.expression,
                        stackframe:=stackframe
                    )

                    Return New SyntaxResult(lambda)
                End If
            End With
        End Function
    End Module
End Namespace
