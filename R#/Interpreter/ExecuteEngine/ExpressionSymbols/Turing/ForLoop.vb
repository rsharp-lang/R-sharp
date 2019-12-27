#Region "Microsoft.VisualBasic::a6fbc9d70db6ddb39d73b5c49dd83ba7, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb"

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
    '         Function: Evaluate, exec, execParallel, ParseLoopBody, RunLoop
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 在R语言之中，只有for each循环
    ''' </summary>
    Public Class ForLoop : Inherits Expression

        ''' <summary>
        ''' 单个变量或者tuple的时候为多个变量
        ''' </summary>
        Dim variables$()
        Dim sequence As Expression
        ''' <summary>
        ''' 为了兼容tuple的赋值，在这里这个函数体就没有参数了
        ''' </summary>
        Dim body As DeclareNewFunction

        ''' <summary>
        ''' ``%dopar%``
        ''' </summary>
        Dim parallel As Boolean = False

        ''' <summary>
        ''' 循环体的返回值类型就是for循环的返回值类型
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return body.type
            End Get
        End Property

        Sub New(tokens As IEnumerable(Of Token))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)
            Dim [loop] = blocks(Scan0).Skip(1).SplitByTopLevelDelimiter(TokenType.keyword)
            Dim vars As Token() = [loop](Scan0)

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

            Me.sequence = Expression.CreateExpression([loop].Skip(2).IteratesALL)
            Me.body = New DeclareNewFunction With {
                .body = ParseLoopBody(blocks(2), isParallel:=parallel),
                .funcName = "forloop_internal",
                .params = {}
            }
        End Sub

        Private Shared Function ParseLoopBody(tokens As Token(), ByRef isParallel As Boolean) As ClosureExpression
            If tokens(Scan0) = (TokenType.open, "{") Then
                Return tokens _
                    .Skip(1) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree)
            ElseIf tokens(Scan0) = (TokenType.operator, "%") AndAlso
                   tokens(1) = (TokenType.identifier, "dopar") AndAlso
                   tokens(2) = (TokenType.operator, "%") Then
                isParallel = True

                Return tokens _
                    .Skip(4) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree)
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As New List(Of Object)
            Dim runLoop As Func(Of Environment, Object)

            If parallel Then
                runLoop = AddressOf execParallel
            Else
                runLoop = AddressOf exec
            End If

            For Each item As Object In envir.DoCall(runLoop)
                If Program.isException(item) Then
                    Return item
                Else
                    result += item
                End If
            Next

            Return result.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"for (let {variables.GetJson} in {sequence}) {If(parallel, "%dopar%", "%do%")} {{
    # forloop_internal
    {body.body}
}}"
        End Function

        Private Function execParallel(envir As Environment) As IEnumerable(Of Object)
            Dim data As Array = Runtime.asVector(Of Object)(sequence.Evaluate(envir))
            Dim result As IEnumerable(Of Object) = data _
                .AsObjectEnumerator _
                .AsParallel _
                .Select(Function(value, i)
                            Return RunLoop(value, i, envir)
                        End Function)

            Return result
        End Function

        Private Function RunLoop(value As Object, loopTag$, env As Environment) As Object
            Using closure As Environment = DeclareNewVariable.PushNames(
                    names:=variables,
                    value:=value,
                    type:=TypeCodes.generic,
                    envir:=New Environment(env, $"for__[{loopTag}]")
                )

                Return body.Invoke(closure, {})
            End Using
        End Function

        Private Iterator Function exec(envir As Environment) As IEnumerable(Of Object)
            Dim data As Array = Runtime.asVector(Of Object)(sequence.Evaluate(envir))
            Dim i As i32 = 1

            For Each value As Object In data
                Yield RunLoop(value, ++i, envir)
            Next
        End Function
    End Class
End Namespace
