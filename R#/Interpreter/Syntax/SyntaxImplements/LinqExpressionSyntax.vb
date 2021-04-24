#Region "Microsoft.VisualBasic::d5be3c5ac6d84fbbe6e3bc00a1415388, R#\Interpreter\Syntax\SyntaxImplements\LinqExpressionSyntax.vb"

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

'     Module LinqExpressionSyntax
' 
'         Function: LinqExpression
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module LinqExpressionSyntax

        Friend ReadOnly linqKeywordDelimiters As String() = {"where", "distinct", "select", "order", "group", "let"}

        ''' <summary>
        ''' produce a <see cref="LinqQuery"/>
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LinqExpression(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            'Dim variables As New List(Of String)
            'Dim i As Integer = 0
            'Dim sequence As SyntaxResult = Nothing
            'Dim locals As New List(Of DeclareNewSymbol)
            'Dim trace As New StackFrame

            'For i = 1 To tokens.Count - 1
            '    If tokens(i).isIdentifier Then
            '        variables.Add(tokens(i)(Scan0).text)

            '        If Not trace Is Nothing Then
            '            trace = opts.GetStackTrace(tokens(i)(Scan0))
            '        End If
            '    ElseIf tokens(i).isKeyword("in") Then
            '        sequence = Expression.CreateExpression(tokens(i + 1), opts)
            '        Exit For
            '    End If
            'Next

            'If sequence Is Nothing Then
            '    Return New SyntaxResult("Missing sequence provider!", opts.debug)
            'ElseIf sequence.isException Then
            '    Return sequence
            'Else
            '    i += 2
            '    locals = New DeclareNewSymbol(
            '        names:=variables.ToArray,
            '        value:=Nothing,
            '        type:=TypeCodes.generic,
            '        [readonly]:=False,
            '        stackFrame:=trace
            '    )
            'End If

            'tokens = tokens _
            '    .Skip(i) _
            '    .IteratesALL _
            '    .SplitByTopLevelDelimiter(TokenType.keyword)

            'Dim projection As Expression = Nothing
            'Dim output As New List(Of ExpressionSymbols.Closure.FunctionInvoke)
            'Dim program As ClosureExpression = Nothing
            'Dim p As New Pointer(Of Token())(tokens)
            'Dim parser As New LinqSyntaxParser(p, opts)
            'Dim [error] As SyntaxResult = parser.doParseLINQProgram(locals, projection, output, program)
            'Dim stackframe As New StackFrame With {
            '    .File = opts.source.fileName,
            '    .Line = tokens.First()(Scan0).span.line,
            '    .Method = New Method With {
            '        .Method = "linq_closure",
            '        .[Module] = "linq_closure",
            '        .[Namespace] = "SMRUCC/R#"
            '    }
            '}

            'If Not [error] Is Nothing AndAlso [error].isException Then
            '    Return [error]
            'Else
            '    Return New LinqExpression(
            '        locals:=locals,
            '        sequence:=sequence.expression,
            '        program:=program,
            '        projection:=projection,
            '        output:=output,
            '        stackframe:=stackframe
            '    )
            'End If
        End Function
    End Module
End Namespace
