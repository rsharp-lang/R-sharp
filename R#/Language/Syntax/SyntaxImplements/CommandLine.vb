#Region "Microsoft.VisualBasic::a44d77457d110cd652fb9665e95c2c2d, R-sharp\R#\Interpreter\Syntax\SyntaxImplements\CommandLine.vb"

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

    '   Total Lines: 111
    '    Code Lines: 82
    ' Comment Lines: 12
    '   Blank Lines: 17
    '     File Size: 4.30 KB


    '     Module CommandLineSyntax
    ' 
    '         Function: CommandLine, CommandLineArgument, IsArgumentGetter, isInterpolation
    ' 
    '     Enum ArgumentGetters
    ' 
    '         GetArgument, GetArgumentWithDefault, GetArgumentWithRequired
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module CommandLineSyntax

        Const InterpolatePattern$ = "[$]\{.+?\}"

        Public Function CommandLine(shell As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            If Not shell.text _
                .Match(InterpolatePattern, RegexOptions.Singleline) _
                .StringEmpty Then

                ' 如果是字符串插值，在Windows平台上会需要注意转义问题
                ' 因为windows平台上文件夹符号为\
                ' 可能会对$产生转义，从而导致字符串插值失败
                Dim temp As SyntaxResult = SyntaxImplements.StringInterpolation(shell, opts)

                If temp.isException Then
                    Return temp
                Else
                    Return New ExternalCommandLine(temp.expression)
                End If
            Else
                Dim literalSyntax As SyntaxResult = SyntaxImplements.LiteralSyntax(shell, opts)

                If literalSyntax.isException Then
                    Return literalSyntax
                Else
                    Return New ExternalCommandLine(literalSyntax.expression)
                End If
            End If
        End Function

        <Extension>
        Friend Function isInterpolation(text As String) As Boolean
            Return Not text.Match(InterpolatePattern, RegexOptions.Singleline).StringEmpty
        End Function

        Public Function CommandLineArgument(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim name As SyntaxResult

            If tokens.First = TokenType.iif Then
                name = tokens _
                    .Skip(1) _
                    .DoCall(Function(code)
                                Return opts.ParseExpression(code, opts)
                            End Function)
            Else
                name = opts.ParseExpression(tokens, opts)
            End If

            If name.isException Then
                Return name
            Else
                Return New ArgumentValue(name.expression)
            End If
        End Function

        Public Function IsArgumentGetter(assert As Expression) As ArgumentGetters
            If TypeOf assert Is ArgumentValue Then
                Return ArgumentGetters.GetArgument
            ElseIf TypeOf assert Is BinaryOrExpression Then
                Dim binor As BinaryOrExpression = DirectCast(assert, BinaryOrExpression)

                If Not TypeOf binor.left Is ArgumentValue Then
                    Return ArgumentGetters.FALSE
                ElseIf TypeOf binor.right Is FunctionInvoke Then
                    Dim funcName As Expression = DirectCast(binor.right, FunctionInvoke).funcName

                    If TypeOf funcName Is Literal AndAlso DirectCast(funcName, Literal) = "stop" Then
                        Return ArgumentGetters.GetArgumentWithRequired
                    Else
                        Return ArgumentGetters.GetArgumentWithDefault
                    End If
                Else
                    Return ArgumentGetters.GetArgumentWithDefault
                End If
            Else
                Return ArgumentGetters.FALSE
            End If
        End Function
    End Module

    Public Enum ArgumentGetters As Integer
        [FALSE] = -1

        ''' <summary>
        ''' ?"--arg"
        ''' </summary>
        GetArgument
        ''' <summary>
        ''' ?"--arg" || default
        ''' </summary>
        GetArgumentWithDefault
        ''' <summary>
        ''' ?"--arg" || stop(xxx)
        ''' </summary>
        GetArgumentWithRequired
    End Enum

End Namespace
