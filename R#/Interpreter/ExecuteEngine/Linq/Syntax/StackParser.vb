#Region "Microsoft.VisualBasic::f92ffed09e9590711104f5d59e309ef6, R#\Interpreter\ExecuteEngine\Linq\Syntax\StackParser.vb"

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

    '   Total Lines: 182
    '    Code Lines: 145 (79.67%)
    ' Comment Lines: 10 (5.49%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 27 (14.84%)
    '     File Size: 7.31 KB


    '     Module StackParser
    ' 
    '         Function: DoSplitByTopLevelStack, filter, GetParentPair, isKeyword, isKeywordAggregate
    '                   isKeywordFrom, isKeywordJoin, isKeywordSelect, SplitByTopLevelStack, SplitOperators
    '                   SplitParameters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    ''' <summary>
    ''' Stack parser of the Linq query expression
    ''' </summary>
    Module StackParser

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function isKeywordFrom(t As Token) As Boolean
            Return isKeyword(t, "from")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function isKeywordAggregate(t As Token) As Boolean
            Return isKeyword(t, "aggregate")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function isKeywordJoin(t As Token) As Boolean
            Return isKeyword(t, "join")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function isKeyword(t As Token, text As String) As Boolean
            Return t.name = TokenType.keyword AndAlso t.text.TextEquals(text)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function isKeywordSelect(t As Token) As Boolean
            Return isKeyword(t, "select")
        End Function

        Private Function GetParentPair(t As String) As String
            Select Case t
                Case "]" : Return "["
                Case "}" : Return "{"
                Case ")" : Return "("
                Case Else
                    Throw New InvalidExpressionException
            End Select
        End Function

        <Extension>
        Public Function SplitByTopLevelStack(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token())
            Static ignores As Index(Of String) = {"as", "by", "descending", "ascending"}

            Return tokenList _
                .DoSplitByTopLevelStack(Function(t)
                                            Return t.name = TokenType.keyword AndAlso (Not t.text.ToLower Like ignores)
                                        End Function, True, True, False)
        End Function

        <Extension>
        Public Function SplitParameters(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token())
            Return tokenList _
                .DoSplitByTopLevelStack(Function(t)
                                            Return t.name = TokenType.comma
                                        End Function, False, False, False)
        End Function

        <Extension>
        Public Iterator Function SplitOperators(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token())
            For Each block As Token() In tokenList _
                .DoSplitByTopLevelStack(Function(t)
                                            Return t.name = TokenType.operator
                                        End Function, True, False, True)

                If block(Scan0).name = TokenType.operator Then
                    Yield {block(Scan0)}
                    Yield block.Skip(1).ToArray
                Else
                    Yield block
                End If
            Next
        End Function

        Private Function filter(t As Token) As Boolean
            Return t.name <> TokenType.terminator AndAlso t.name <> TokenType.comment
        End Function

        ''' <summary>
        ''' 根据最顶端的关键词以及括号进行栈片段的分割
        ''' </summary>
        ''' <param name="tokenList"></param>
        ''' <returns></returns>
        <Extension>
        Private Iterator Function DoSplitByTopLevelStack(tokenList As IEnumerable(Of Token),
                                                         delimiter As Func(Of Token, Boolean),
                                                         isParentStack As Boolean,
                                                         pushStack As Boolean,
                                                         popOnClearStack As Boolean) As IEnumerable(Of Token())
            Dim block As New List(Of Token)
            Dim stack As New Stack(Of String)
            Dim i As New Pointer(Of Token)(tokenList.Where(Function(t) filter(t)))

            Do While Not i.EndRead
                Dim item As Token = ++i

                If delimiter(item) AndAlso (item <> (TokenType.operator, "$")) Then
                    If stack.Count > 1 Then
                        block.Add(item)
                    Else
                        If block > 0 Then
                            If popOnClearStack AndAlso stack.Count <> 0 Then
                                ' do nothing
                            Else
                                Yield block.PopAll
                            End If
                        End If

                        block.Add(item)
                    End If

                    If pushStack Then
                        If stack.Count > 0 Then
                            stack.Pop()
                        End If

                        stack.Push(item.text)
                    End If
                ElseIf item.name = TokenType.open Then
                    stack.Push(item.text)

                    If stack.Count > 1 Then
                        block.Add(item)
                    Else
                        If isParentStack AndAlso block > 0 Then
                            Yield block.PopAll
                        End If

                        block.Add(item)
                    End If
                ElseIf item.name = TokenType.close Then
                    Dim parent As String = stack.Pop

                    If parent <> GetParentPair(item.text) Then
                        Throw New SyntaxErrorException
                    End If

                    If stack.Count > 1 OrElse (stack.Count = 1 AndAlso "([{".IndexOf(stack.Peek) > -1) Then
                        block.Add(item)
                    Else
                        block.Add(item)

                        If isParentStack AndAlso block > 0 AndAlso (i.Current Is Nothing OrElse (i.Current.name <> TokenType.comma AndAlso i.Current.name <> TokenType.operator)) Then
                            Yield block.PopAll
                        End If
                    End If

                    If stack.Count = 1 AndAlso "([{".IndexOf(stack.Peek) = -1 Then
                        ' stack.Pop()
                        Console.Write("")
                    End If
                Else
                    block.Add(item)
                End If
            Loop

            If stack.Count = 1 AndAlso "([{".IndexOf(stack.Peek) = -1 Then
                If block > 0 Then
                    Yield block.PopAll
                End If
            ElseIf stack.Count > 0 Then
                Throw New SyntaxErrorException
            ElseIf block > 0 Then
                Yield block.PopAll
            End If
        End Function
    End Module
End Namespace
