#Region "Microsoft.VisualBasic::5f880a78922dca450ecd65b64b7a4ba5, LINQ\LINQ\Script\SyntaxImplements.vb"

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

'     Module SyntaxImplements
' 
'         Function: CreateAggregateQuery, CreateProjectionQuery, GetParameters, GetProjection, GetSequence
'                   GetVector, IsClosure, IsNumeric, JoinOperators, ParseExpression
'                   ParseKeywordExpression, ParseToken, PopulateExpressions, PopulateQueryExpression
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Public Module SyntaxImplements

        ''' <summary>
        ''' target token is a number literal value? 
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <Extension>
        Private Function IsNumeric(t As Token) As Boolean
            Return t.name = TokenType.integerLiteral OrElse t.name = TokenType.numberLiteral
        End Function

        <Extension>
        Private Function JoinOperators(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token)
            Dim list As New List(Of Token)(tokenList)
            Dim t As Token

            For i As Integer = 0 To list.Count - 1
                If i >= list.Count Then
                    Exit For
                Else
                    t = list(i)
                End If

                If t = (TokenType.operator, "-") AndAlso list(i + 1).isNumeric Then
                    t = New Token(list(i + 1).name, -Val(list(i + 1).text))
                    list.RemoveAt(i + 1)
                    list.RemoveAt(i)
                    list.Insert(i, t)
                ElseIf t = (TokenType.operator, ">") OrElse t = (TokenType.operator, "<") Then
                    If list(i + 1) = (TokenType.operator, "=") OrElse list(i + 1) = (TokenType.operator, ">") Then
                        t = New Token(TokenType.operator, t.text & list(i + 1).text)
                        list.RemoveAt(i + 1)
                        list.RemoveAt(i)
                        list.Insert(i, t)
                    End If
                End If
            Next

            Return list
        End Function

        ReadOnly sortOrders As Index(Of String) = {"descending", "ascending"}

        ''' <summary>
        ''' the main entry of parse linq expression
        ''' </summary>
        ''' <param name="tokenList"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PopulateQueryExpression(tokenList As IEnumerable(Of Token)) As Expression
            Dim blocks As List(Of Token()) = tokenList _
                .JoinOperators _
                .SplitByTopLevelStack _
                .ToList

            For i As Integer = 1 To blocks.Count - 1
                If i >= blocks.Count Then
                    Exit For
                End If

                If blocks(i).Length = 1 AndAlso blocks(i)(Scan0).name = TokenType.keyword Then
                    If blocks(i)(Scan0).text.ToLower Like sortOrders Then
                        blocks(i - 1) = blocks(i - 1) _
                            .JoinIterates(blocks(i)) _
                            .ToArray
                        blocks.RemoveAt(i)
                    End If
                End If
            Next

            If blocks(Scan0).First.isKeywordFrom Then
                Return blocks(Scan0).CreateProjectionQuery(blocks.Skip(1).ToArray)
            ElseIf blocks(Scan0).First.isKeywordAggregate Then
                Return blocks(Scan0).CreateAggregateQuery(blocks.Skip(1).ToArray)
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        <Extension>
        Private Function CreateProjectionQuery(symbol As Token(), blocks As Token()()) As ProjectionExpression
            Dim symbolExpr As SymbolDeclare = symbol.ParseExpression
            Dim i As Integer = 0
            Dim seq As Expression = blocks.GetSequence(offset:=i)
            Dim exec As Expression() = blocks.Skip(i).PopulateExpressions.ToArray
            Dim proj As Expression = exec.Where(Function(t) TypeOf t Is OutputProjection).FirstOrDefault
            Dim opt As New Options(exec.Where(Function(t) TypeOf t Is PipelineKeyword))
            Dim execProgram As Expression() = exec _
                .Where(Function(t)
                           Return (Not TypeOf t Is PipelineKeyword) AndAlso (Not TypeOf t Is OutputProjection)
                       End Function) _
                .ToArray
            Dim Linq As New ProjectionExpression(symbolExpr, seq, execProgram, proj, opt)

            Return Linq
        End Function

        <Extension>
        Private Iterator Function PopulateExpressions(blocks As IEnumerable(Of Token())) As IEnumerable(Of Expression)
            For Each blockLine As Token() In blocks
                Yield ParseExpression(blockLine)
            Next
        End Function

        <Extension>
        Private Function GetSequence(blocks As Token()(), ByRef offset As Integer) As Expression
            If Not blocks(Scan0).First.isKeyword("in") Then
                Throw New SyntaxErrorException
            ElseIf blocks(Scan0).Length = 1 Then
                offset = 2
                Return blocks(1).ParseExpression
            Else
                offset = 1
                Return blocks(0).ParseExpression
            End If
        End Function

        <Extension>
        Private Function CreateAggregateQuery(symbol As Token(), blocks As Token()()) As AggregateExpression
            Dim symbolExpr As SymbolDeclare = symbol.ParseExpression
            ' Dim seq As Expression

            Throw New NotImplementedException
        End Function

        <Extension>
        Friend Function ParseToken(t As Token) As Expression
            If t.name = TokenType.identifier Then
                Return New SymbolReference(t.text)
            ElseIf t.name = TokenType.booleanLiteral OrElse
                t.name = TokenType.integerLiteral OrElse
                t.name = TokenType.numberLiteral OrElse
                t.name = TokenType.stringLiteral Then

                Return New Literal(t)
            Else
                Throw New NotImplementedException
            End If
        End Function

        <Extension>
        Private Function GetProjection(tokenList As IEnumerable(Of Token)) As Expression
            Dim values As Expression() = tokenList.GetParameters.ToArray
            Dim fields As New List(Of NamedValue(Of Expression))

            For Each item As Expression In values
                If TypeOf item Is BinaryExpression Then
                    With DirectCast(item, BinaryExpression)
                        fields.Add(New NamedValue(Of Expression)(item.ToString, item))
                    End With
                ElseIf TypeOf item Is ValueAssign Then
                    With DirectCast(item, ValueAssign)
                        fields.Add(New NamedValue(Of Expression)(DirectCast(.left, SymbolReference).symbolName, .right))
                    End With
                ElseIf TypeOf item Is MemberReference Then
                    With DirectCast(item, MemberReference)
                        fields.Add(New NamedValue(Of Expression)(.memberName, item))
                    End With
                Else
                    fields.Add(New NamedValue(Of Expression)(item.ToString, item))
                End If
            Next

            Return New OutputProjection(fields)
        End Function

        <Extension>
        Private Function ParseKeywordExpression(tokenList As Token()) As Expression
            If tokenList(Scan0).isKeywordFrom OrElse tokenList(Scan0).isKeywordAggregate OrElse tokenList(Scan0).isKeyword("let") Then
                ' declare new symbol
                Dim name As String = tokenList(1).text
                Dim type As String = "any"

                If tokenList.Length > 2 Then
                    type = tokenList(3).text
                End If

                Return New SymbolDeclare With {.symbolName = name, .typeName = type}
            ElseIf tokenList(Scan0).isKeyword("where") Then
                Return New WhereFilter(ParseExpression(tokenList.Skip(1).ToArray))
            ElseIf tokenList(Scan0).isKeyword("in") Then
                Return ParseExpression(tokenList.Skip(1).ToArray)
            ElseIf tokenList(Scan0).isKeyword("select") Then
                Return tokenList.Skip(1).GetProjection
            ElseIf tokenList(Scan0).isKeyword("order") Then
                Dim sortKey = tokenList.Skip(2).ToArray
                Dim desc As Boolean

                If sortKey.Last.name = TokenType.keyword AndAlso sortKey.Last.text.ToLower Like sortOrders Then
                    desc = sortKey.Last.text.TextEquals("descending")
                    sortKey = sortKey.Take(sortKey.Length - 1).ToArray
                End If

                Return New OrderBy(ParseExpression(sortKey), desc)
            ElseIf tokenList(Scan0).isKeyword("take") Then
                Return New TakeItems(ParseExpression(tokenList.Skip(1).ToArray))
            ElseIf tokenList(Scan0).isKeyword("skip") Then
                Return New SkipItems(ParseExpression(tokenList.Skip(1).ToArray))
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        <Extension>
        Private Function IsClosure(tokenList As Token()) As Boolean
            Return tokenList(Scan0).name = TokenType.open AndAlso tokenList.Last.name = TokenType.close
        End Function

        <Extension>
        Public Function ParseExpression(tokenList As Token()) As Expression
            If tokenList.Length = 1 Then
                Return tokenList(Scan0).ParseToken
            ElseIf tokenList(Scan0).name = TokenType.keyword Then
                Return tokenList.ParseKeywordExpression
            End If

            Dim blocks = tokenList.SplitByTopLevelStack.ToArray

            If blocks.Length = 1 Then
                tokenList = blocks(Scan0)

                If tokenList.First = (TokenType.open, "[") OrElse tokenList.First = (TokenType.open, "{") Then
                    Return tokenList.Skip(1).Take(tokenList.Length - 2).GetVector
                ElseIf tokenList.First = (TokenType.open, "(") Then
                    tokenList = tokenList _
                        .Skip(1) _
                        .Take(tokenList.Length - 2) _
                        .ToArray
                End If
            ElseIf blocks.Length = 2 Then
                Dim name As Expression = ParseExpression(blocks(Scan0))

                If TypeOf name Is SymbolReference AndAlso blocks(1).IsClosure Then
                    Dim params As Expression() = blocks(1) _
                        .Skip(1) _
                        .Take(blocks(1).Length - 2) _
                        .GetParameters

                    Return New FunctionInvoke(name, Nothing, params)
                End If
            End If

            Return tokenList.ParseBinary
        End Function

        <Extension>
        Private Iterator Function GetParameters(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Expression)
            Dim blocks As Token()() = tokenList _
                .SplitParameters _
                .Select(Function(b)
                            If b(Scan0).name = TokenType.comma Then
                                Return b.Skip(1).ToArray
                            Else
                                Return b
                            End If
                        End Function) _
                .ToArray

            For Each block As Token() In blocks
                Yield ParseExpression(block)
            Next
        End Function

        <Extension>
        Private Function GetVector(tokenList As IEnumerable(Of Token)) As Expression
            Dim elements As Expression() = tokenList.GetParameters.ToArray
            Dim vec As New VectorLiteral(elements)

            Return vec
        End Function
    End Module
End Namespace
