#Region "Microsoft.VisualBasic::6b472e26e22b69fd273a06ce19cbd8a6, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb"

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

    '     Class ForLoop
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, exec
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 在R语言之中，只有for each循环
    ''' </summary>
    Public Class ForLoop : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' 单个变量或者tuple的时候为多个变量
        ''' </summary>
        Dim variables$()
        Dim sequence As Expression
        ''' <summary>
        ''' 为了兼容tuple的赋值，在这里这个函数体就没有参数了
        ''' </summary>
        Dim body As DeclareNewFunction

        Sub New(tokens As IEnumerable(Of Token))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)
            Dim [loop] = blocks(Scan0).Skip(1).SplitByTopLevelDelimiter(TokenType.keyword)
            Dim vars = [loop](Scan0)

            If vars.Length = 1 Then
                variables = {vars(Scan0).text}
            Else
                variables = vars _
                    .Skip(1) _
                    .Take(vars.Length - 2) _
                    .Where(Function(x) Not x.name = TokenType.comma) _
                    .Select(Function(x) x.text) _
                    .ToArray
            End If

            Me.sequence = Expression.CreateExpression([loop](2))
            Me.body = New DeclareNewFunction With {
                .body = blocks(2) _
                    .Skip(1) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree),
                .funcName = "forloop_internal",
                .params = {}
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As New List(Of Object)

            For Each item As Object In envir.DoCall(AddressOf exec)
                If Program.isException(item) Then
                    Return item
                Else
                    result += item
                End If
            Next

            Return result.ToArray
        End Function

        Private Iterator Function exec(envir As Environment) As IEnumerable(Of Object)
            Dim isTuple As Boolean = variables.Length > 1
            Dim i As i32 = 1

            For Each value As Object In Runtime.asVector(Of Object)(sequence.Evaluate(envir))
                Using closure = DeclareNewVariable.PushNames(
                    names:=variables,
                    value:=value,
                    type:=TypeCodes.generic,
                    envir:=New Environment(envir, $"for__[{++i}]")
                )

                    Yield body.Invoke(closure, {})
                End Using
            Next
        End Function
    End Class
End Namespace
