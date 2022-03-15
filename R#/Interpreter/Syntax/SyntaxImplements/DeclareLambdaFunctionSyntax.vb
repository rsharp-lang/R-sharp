#Region "Microsoft.VisualBasic::7e169b31db1d24fa394aa3dfe7ed6bd3, R-sharp\R#\Interpreter\Syntax\SyntaxImplements\DeclareLambdaFunctionSyntax.vb"

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


     Code Statistics:

        Total Lines:   90
        Code Lines:    76
        Comment Lines: 7
        Blank Lines:   7
        File Size:     3.90 KB


    '     Module DeclareLambdaFunctionSyntax
    ' 
    '         Function: (+2 Overloads) DeclareLambdaFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareLambdaFunctionSyntax

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        Public Function DeclareLambdaFunction(args As SymbolReference, expression As Expression, lineNum%, opts As SyntaxBuilderOptions) As SyntaxResult
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
                names:={args.symbol},
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
