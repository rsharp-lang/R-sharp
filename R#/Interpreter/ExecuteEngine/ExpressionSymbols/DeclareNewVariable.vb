#Region "Microsoft.VisualBasic::767f07da6bc1125f5127e37d6806113c, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DeclareNewVariable.vb"

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

    '     Class DeclareNewVariable
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Evaluate, getNames, PushNames, ToString
    ' 
    '         Sub: getInitializeValue, PushTuple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class DeclareNewVariable : Inherits Expression

        ''' <summary>
        ''' 对于tuple类型，会存在多个变量
        ''' </summary>
        Friend names As String()
        Friend value As Expression
        Friend hasInitializeExpression As Boolean = False

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(code As List(Of Token()))
            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            names = getNames(code(1))

            If code.Count = 2 Then
                type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    Call code.Skip(5).DoCall(AddressOf getInitializeValue)
                End If
            Else
                type = TypeCodes.generic

                If code.Count > 2 AndAlso code(2).isOperator("=", "<-") Then
                    Call code.Skip(3).DoCall(AddressOf getInitializeValue)
                End If
            End If
        End Sub

        Sub New(code As List(Of Token))
            Call Me.New(code:=code.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True))
        End Sub

        Sub New(singleToken As Token())
            names = getNames(singleToken)
            type = TypeCodes.generic
            hasInitializeExpression = False
            value = Nothing
        End Sub

        Sub New()
        End Sub

        Friend Shared Function getNames(code As Token()) As String()
            If code.Length > 1 Then
                Return code.Skip(1) _
                    .Take(code.Length - 2) _
                    .Where(Function(token) Not token.name = TokenType.comma) _
                    .Select(Function(symbol)
                                If symbol.name <> TokenType.identifier Then
                                    Throw New SyntaxErrorException
                                Else
                                    Return symbol.text
                                End If
                            End Function) _
                    .ToArray
            Else
                Return {code(Scan0).text}
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub getInitializeValue(code As IEnumerable(Of Token()))
            hasInitializeExpression = True
            value = Expression.ParseExpression(code.AsList)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object

            If Me.value Is Nothing Then
                value = Nothing
            Else
                value = Me.value.Evaluate(envir)
            End If

            Call PushNames(names, value, type, envir)

            Return value
        End Function

        Friend Shared Function PushNames(names$(), value As Object, type As TypeCodes, envir As Environment) As Environment
            If names.Length = 1 Then
                Call envir.Push(names(Scan0), value, type)
            Else
                ' tuple
                Call PushTuple(names, value, type, envir)
            End If

            Return envir
        End Function

        Private Shared Sub PushTuple(names$(), value As Object, type As TypeCodes, envir As Environment)
            If value.GetType.IsInheritsFrom(GetType(Array)) Then
                Dim vector As Array = value

                If vector.Length = 1 Then
                    ' all set with one value
                    For Each name As String In names
                        Call envir.Push(name, value)
                    Next
                ElseIf vector.Length = names.Length Then
                    ' declare one by one
                    For i As Integer = 0 To vector.Length - 1
                        Call envir.Push(names(i), vector.GetValue(i))
                    Next
                Else
                    Throw New SyntaxErrorException
                End If
            Else
                ' all set with one value
                For Each name As String In names
                    Call envir.Push(name, value)
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            If names.Length > 1 Then
                Return $"Dim [{names.JoinBy(", ")}] As {type.Description} = {Scripting.ToString(value, "NULL")}"
            Else
                Return $"Dim {names(Scan0)} As {type.Description} = {Scripting.ToString(value, "NULL")}"
            End If
        End Function
    End Class
End Namespace
