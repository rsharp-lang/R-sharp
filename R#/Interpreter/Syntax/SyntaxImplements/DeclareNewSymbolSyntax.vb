#Region "Microsoft.VisualBasic::5776f980eb39e5c774886a7f78ab8344, R#\Interpreter\Syntax\SyntaxImplements\DeclareNewSymbolSyntax.vb"

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

    '     Module DeclareNewSymbolSyntax
    ' 
    '         Function: (+4 Overloads) DeclareNewSymbol, getNames, ModeOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareNewSymbolSyntax

        ReadOnly isKeyword As (TokenType, String) = (TokenType.keyword, "is")
        ReadOnly isSymbol As (TokenType, String) = (TokenType.identifier, "is")

        Public Function ModeOf(keyword$, target As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            If target.Any(Function(a) a = isKeyword OrElse a = isSymbol) Then
                If keyword <> "typeof" Then
                    Return SyntaxResult.CreateError(
                        err:=New NotImplementedException("type check is only implement on typeof keyword."),
                        opts:=opts.SetCurrentRange(target)
                    )
                End If

                Dim blockParts = target.Split(Function(a) a = isKeyword OrElse a = isSymbol).ToArray
                Dim obj As SyntaxResult = Expression.CreateExpression(blockParts(0), opts)

                If obj.isException Then
                    Return obj
                Else
                    Dim checkType As SyntaxResult = Expression.CreateExpression(blockParts(1), opts)

                    If checkType.isException Then
                        Return checkType
                    End If

                    Return New TypeOfCheck(obj.expression, checkType.expression)
                End If
            Else
                Dim objTarget As SyntaxResult = Expression.CreateExpression(target, opts)

                If objTarget.isException Then
                    Return objTarget
                Else
                    Return New ModeOf(keyword, objTarget.expression)
                End If
            End If
        End Function

        ''' <summary>
        ''' const x as type = xxx
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="[readonly]"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DeclareNewSymbol(code As List(Of Token()), [readonly] As Boolean, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim valSyntaxtemp As SyntaxResult = Nothing

            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            Dim symbolNames = getNames(code(1))
            Dim type As TypeCodes
            Dim value As Expression = Nothing
            Dim trace As StackFrame = opts.GetStackTrace(code(1)(Scan0))

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(code.IteratesALL.ToArray)
                )
            End If

            If code = 2 Then
                type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(5).AsList.ParseExpression(opts)
                End If
            Else
                type = TypeCodes.generic

                If code > 2 AndAlso code(2).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(3).AsList.ParseExpression(opts)
                End If
            End If

            If (Not valSyntaxtemp Is Nothing) AndAlso valSyntaxtemp.isException Then
                Return valSyntaxtemp
            Else
                value = valSyntaxtemp?.expression
            End If

            Dim symbol As New DeclareNewSymbol(
                names:=symbolNames,
                value:=value,
                type:=type,
                [readonly]:=[readonly],
                stackFrame:=trace
            )

            Return New SyntaxResult(symbol)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DeclareNewSymbol(code As List(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .DeclareNewSymbol(False, opts)
        End Function

        ''' <summary>
        ''' declare a parameter symbol
        ''' </summary>
        ''' <param name="singleToken"></param>
        ''' <returns></returns>
        Public Function DeclareNewSymbol(singleToken As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim symbolNames = getNames(singleToken)
            Dim type As TypeCodes
            Dim trace As StackFrame = opts.GetStackTrace(singleToken(Scan0))

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(singleToken)
                )
            End If

            If singleToken.Length > 1 AndAlso symbolNames.TryCast(Of String()).Length = 1 Then
                type = singleToken(2).text.GetRTypeCode
            Else
                type = TypeCodes.generic
            End If

            Return New DeclareNewSymbol(
                names:=symbolNames,
                value:=Nothing,
                type:=type,
                [readonly]:=False,
                stackFrame:=trace
            )
        End Function

        ''' <summary>
        ''' declare a new parameter with optional default value
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="value"></param>
        ''' <param name="opts"></param>
        ''' <param name="funcParameter"></param>
        ''' <returns></returns>
        Public Function DeclareNewSymbol(symbol As Token(), value As Token(), opts As SyntaxBuilderOptions, funcParameter As Boolean) As SyntaxResult
            Dim valSyntaxTemp As SyntaxResult = Expression.CreateExpression(value, opts)

            If valSyntaxTemp.isException Then
                Return valSyntaxTemp
            End If

            Dim symbolNames = getNames(symbol)

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(symbol)
                )
            End If

            Dim type As TypeCodes
            Dim trace As StackFrame = opts.GetStackTrace(symbol(Scan0))

            If funcParameter Then
                type = TypeCodes.generic
            Else
                type = valSyntaxTemp.expression.type
            End If

            Return New DeclareNewSymbol(
                names:=symbolNames,
                value:=valSyntaxTemp.expression,
                type:=type,
                [readonly]:=False,
                stackFrame:=trace
            )
        End Function

        ''' <summary>
        ''' get tuple names or a single symbol name
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        Friend Function getNames(code As Token()) As [Variant](Of String(), SyntaxErrorException)
            If code.Length > 1 Then
                If code(Scan0) = (TokenType.open, "[") AndAlso code.Last = (TokenType.close, "]") Then
                    ' [a,b,c]
                    ' tuple symbol names
                    Dim symbols = code.Skip(1) _
                        .Take(code.Length - 2) _
                        .Where(Function(token) Not token.name = TokenType.comma) _
                        .ToArray
                    Dim names As New List(Of String)

                    For Each symbol In symbols
                        ' allowes using keyword as symbol 
                        If symbol.name <> TokenType.identifier AndAlso symbol.name <> TokenType.keyword Then
                            Return New SyntaxErrorException(code.Select(Function(a) a.text).JoinBy(" "))
                        Else
                            names.Add(symbol.text)
                        End If
                    Next

                    Return names.ToArray
                ElseIf code(1) = (TokenType.keyword, "as") Then
                    ' a as type
                    Return {code(Scan0).text}
                Else
                    Return New SyntaxErrorException(code.Select(Function(a) a.text).JoinBy(" "))
                End If
            Else
                ' single symbol
                Return {code(Scan0).text}
            End If
        End Function
    End Module
End Namespace
