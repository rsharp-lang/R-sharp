#Region "Microsoft.VisualBasic::3f2f7c4570a7cc749d2165f5d6de9873, G:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//Models/SyntaxToken.vb"

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

    '   Total Lines: 89
    '    Code Lines: 71
    ' Comment Lines: 4
    '   Blank Lines: 14
    '     File Size: 2.86 KB


    ' Class SyntaxToken
    ' 
    '     Properties: index, value
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: [TryCast], Cast, GetSymbol, (+2 Overloads) IsToken, ToString
    '     Operators: (+2 Overloads) Like
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class SyntaxToken

    ''' <summary>
    ''' <see cref="Token"/> or <see cref="Expression"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property value As Object
    Public Property index As Integer

    Sub New(i As Integer, t As Token)
        index = i
        value = t
    End Sub

    Sub New(i As Integer, exp As Expression)
        index = i
        value = exp
    End Sub

    Public Overrides Function ToString() As String
        Return $"<{index}> {value.ToString}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <DebuggerStepThrough>
    Public Function [TryCast](Of T As Class)() As T
        Return TryCast(value, T)
    End Function

    Public Function IsToken(token As TokenType, text As String()) As Boolean
        If Not TypeOf value Is Token Then
            Return False
        End If
        If DirectCast(value, Token).name <> token Then
            Return False
        End If

        Return text.Any(Function(si) si = DirectCast(value, Token).text)
    End Function

    Public Function IsToken(token As TokenType, Optional text As String = Nothing) As Boolean
        If Not TypeOf value Is Token Then
            Return False
        End If
        If DirectCast(value, Token).name <> token Then
            Return False
        End If

        If text Is Nothing Then
            Return True
        Else
            Return text = DirectCast(value, Token).text
        End If
    End Function

    Public Function GetSymbol() As String
        If TypeOf value Is Token Then
            Return DirectCast(value, Token).text
        Else
            Return ValueAssignExpression.GetSymbol(DirectCast(value, Expression))
        End If
    End Function

    Public Shared Operator Like(t As SyntaxToken, type As Type) As Boolean
        If t Is Nothing OrElse t.value Is Nothing Then
            Return False
        Else
            Return t.value.GetType Is type OrElse t.value.GetType.IsInheritsFrom(type, strict:=False)
        End If
    End Operator

    Public Shared Iterator Function Cast(list As IEnumerable(Of SyntaxToken)) As IEnumerable(Of [Variant](Of Expression, String))
        For Each item In list
            If item Like GetType(Token) Then
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Token).text)
            Else
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Expression))
            End If
        Next
    End Function

End Class
