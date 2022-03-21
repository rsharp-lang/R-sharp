#Region "Microsoft.VisualBasic::35a46795548cc2dcc58dc52e354e682e, R-sharp\R#\Interpreter\Syntax\SyntaxImplements\VectorLiteral.vb"

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

    '   Total Lines: 225
    '    Code Lines: 181
    ' Comment Lines: 13
    '   Blank Lines: 31
    '     File Size: 10.12 KB


    '     Module VectorLiteralSyntax
    ' 
    '         Function: LiteralSyntax, ParseAnnotation, ParseAnnotations, (+2 Overloads) SequenceLiteral, TypeCodeOf
    '                   VectorLiteral
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module VectorLiteralSyntax

        Public Function LiteralSyntax(token As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Select Case token.name
                Case TokenType.booleanLiteral
                    Return New Literal With {.m_type = TypeCodes.boolean, .value = token.text.ParseBoolean}
                Case TokenType.integerLiteral
                    Return New Literal With {.m_type = TypeCodes.integer, .value = CLng(token.text.ParseInteger)}
                Case TokenType.numberLiteral
                    Return New Literal With {.m_type = TypeCodes.double, .value = token.text.ParseDouble}
                Case TokenType.stringLiteral, TokenType.cliShellInvoke
                    Return New Literal With {.m_type = TypeCodes.string, .value = token.text}
                Case TokenType.missingLiteral
                    Dim type As TypeCodes = TypeCodes.NA
                    Dim value As Object

                    Select Case token.text
                        Case "NULL", "None" : value = Nothing
                        Case "NA" : value = GetType(Void)
                        Case "Inf" : value = Double.PositiveInfinity
                        Case Else
                            Return SyntaxResult.CreateError(
                                err:=New SyntaxErrorException($"Unknown literal token: {token.ToString}"),
                                opts:=opts.SetCurrentRange({token})
                            )
                    End Select

                    Return New Literal With {
                        .m_type = type,
                        .value = value
                    }
                Case Else
                    Return SyntaxResult.CreateError(
                        err:=New InvalidExpressionException(token.ToString),
                        opts:=opts.SetCurrentRange({token})
                    )
            End Select
        End Function

        <Extension>
        Public Iterator Function ParseAnnotations(blocks As Token()) As IEnumerable(Of NamedValue(Of String))
            For Each block As Token() In blocks.Split(4)
                Yield block.Skip(1).Take(2).ToArray.ParseAnnotation
            Next
        End Function

        <Extension>
        Public Function ParseAnnotation(block As Token()) As NamedValue(Of String)
            Dim name As String = block(Scan0).text.Substring(1)
            Dim value As String = block(1).text

            Return New NamedValue(Of String)(name, value)
        End Function

        Public Function VectorLiteral(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            If blocks Is Nothing AndAlso tokens.Last.name = TokenType.cliShellInvoke Then
                Dim cli = CommandLineSyntax.CommandLine(tokens.Last, opts)

                If cli.isException Then
                    Return cli
                End If

                blocks = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 3) _
                    .SplitByTopLevelDelimiter(TokenType.comma)

                Dim annotation As NamedValue(Of String) = blocks(Scan0).ParseAnnotation
                DirectCast(cli.expression, ExternalCommandLine).SetAttribute(annotation)
                Return cli
            ElseIf blocks.Count = 1 AndAlso blocks(Scan0).Length = 2 Then
                Dim block As Token() = blocks(Scan0)

                ' is user annotation
                If block(Scan0).name = TokenType.annotation AndAlso block(1).isLiteral Then
                    Dim annotation As NamedValue(Of String) = block.ParseAnnotation
                    Call opts.annotations.Add(annotation)
                    Return New CodeComment($"{annotation.Name}:={annotation.Value}")
                End If
            End If

            For Each block As Token() In blocks
                ' is a comma symbol
                If block.Length = 1 AndAlso block(Scan0).name = TokenType.comma Then
                    Continue For
                End If

                syntaxTemp = block.DoCall(Function(code) opts.ParseExpression(code, opts))

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    values.Add(syntaxTemp.expression)
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Return New SyntaxResult(New VectorLiteral(values, values.DoCall(AddressOf TypeCodeOf)))
        End Function

        ''' <summary>
        ''' get type code value of the vector literal 
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function TypeCodeOf(values As IEnumerable(Of Expression)) As TypeCodes
            With values.ToArray
                ' fix for System.InvalidOperationException: Nullable object must have a value.
                '
                If .Length = 0 Then
                    Return TypeCodes.generic
                ElseIf .Length = 1 Then
                    Return DirectCast(.GetValue(Scan0), Expression).type
                Else
                    ' generic > string > double > float > long > integer > byte > boolean
                    Static typeCodeWeights As Index(Of TypeCodes) = {
                        TypeCodes.boolean,
                        TypeCodes.integer,
                        TypeCodes.double,
                        TypeCodes.string,
                        TypeCodes.generic
                    }

                    ' get unique types
                    Dim types As TypeCodes() = .Select(Function(exp)
                                                           Dim t = exp.type

                                                           If t = TypeCodes.NA OrElse t = TypeCodes.ref Then
                                                               t = TypeCodes.generic
                                                           End If

                                                           Return t
                                                       End Function) _
                                               .Distinct _
                                               .ToArray
                    If types.Length = 1 Then
                        Return types(Scan0)
                    Else
                        Dim maxType As TypeCodes = TypeCodes.boolean
                        Dim maxWeight As Integer

                        For Each code As TypeCodes In types
                            If typeCodeWeights.IndexOf(code) > maxWeight Then
                                maxType = code
                                maxWeight = typeCodeWeights(maxType)
                            End If
                        Next

                        Return maxType
                    End If
                End If
            End With
        End Function

        Public Function SequenceLiteral(from As Token, [to] As Token, steps As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Return SequenceLiteral({from}, {[to]}, {steps}, opts)
        End Function

        Public Function SequenceLiteral(from As Token(), [to] As Token(), steps As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim fromSyntax = opts.ParseExpression(from, opts)
            Dim toSyntax = opts.ParseExpression([to], opts)
            Dim sourceMap As StackFrame = opts.GetStackTrace(from(Scan0), "sequence")

            If fromSyntax.isException Then
                Return fromSyntax
            ElseIf toSyntax.isException Then
                Return toSyntax
            End If

            If steps.IsNullOrEmpty Then
                Return New SequenceLiteral(fromSyntax.expression, toSyntax.expression, New Literal(1), sourceMap)
            ElseIf steps.isLiteral Then
                Dim stepLiteral As SyntaxResult = SyntaxImplements.LiteralSyntax(steps(Scan0), opts)

                If stepLiteral.isException Then
                    Return stepLiteral
                End If

                Return New SequenceLiteral(
                    from:=fromSyntax.expression,
                    [to]:=toSyntax.expression,
                    steps:=stepLiteral.expression,
                    stackFrame:=sourceMap
                )
            Else
                Dim stepsSyntax As SyntaxResult = opts.ParseExpression(steps, opts)

                If stepsSyntax.isException Then
                    Return stepsSyntax
                Else
                    Return New SequenceLiteral(
                        from:=fromSyntax.expression,
                        [to]:=toSyntax.expression,
                        steps:=stepsSyntax.expression,
                        stackFrame:=sourceMap
                    )
                End If
            End If
        End Function
    End Module
End Namespace
