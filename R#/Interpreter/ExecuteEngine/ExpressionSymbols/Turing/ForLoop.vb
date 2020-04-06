#Region "Microsoft.VisualBasic::dcd2eabc54199043f8bbf88ee66c51a5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb"

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
    '         Properties: body, sequence, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, exec, execParallel, getSequence, RunLoop
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    ''' <summary>
    ''' 在R语言之中，只有for each循环
    ''' </summary>
    Public Class ForLoop : Inherits Expression
        Implements IRuntimeTrace

        ''' <summary>
        ''' 单个变量或者tuple的时候为多个变量
        ''' </summary>
        Dim variables$()

        Public ReadOnly Property sequence As Expression

        ''' <summary>
        ''' 为了兼容tuple的赋值，在这里这个函数体就没有参数了
        ''' </summary>
        Public ReadOnly Property body As DeclareNewFunction

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

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(variables$(), sequence As Expression, body As DeclareNewFunction, parallel As Boolean, stackframe As StackFrame)
            Me.variables = variables
            Me.sequence = sequence
            Me.body = body
            Me.parallel = parallel
            Me.stackFrame = stackframe
        End Sub

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
                ElseIf TypeOf item Is ReturnValue Then
                    Return item
                Else
                    result += Runtime.single(item)
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
            Dim result As IEnumerable(Of Object) = getSequence(envir) _
                .AsParallel _
                .Select(Function(value, i)
                            Dim stackframe As New StackFrame(Me.stackFrame)
                            stackframe.Method.Method = $"parallel_for_loop_[{i}]"
                            Return RunLoop(value, stackframe, envir)
                        End Function)

            Return result
        End Function

        Private Function RunLoop(value As Object, stackframe As StackFrame, env As Environment) As Object
            Using closure As Environment = DeclareNewSymbol.PushNames(
                    names:=variables,
                    value:=value,
                    type:=TypeCodes.generic,
                    envir:=New Environment(env, stackframe, isInherits:=False),
                    [readonly]:=False
                )

                Return body.Invoke(closure, {})
            End Using
        End Function

        Private Function getSequence(env As Environment) As IEnumerable(Of Object)
            Dim rawSeq As Object = sequence.Evaluate(env)
            Dim data As IEnumerable(Of Object) = [Rset].getObjectSet(rawSeq)

            Return data
        End Function

        Private Iterator Function exec(envir As Environment) As IEnumerable(Of Object)
            Dim i As i32 = 1
            Dim stackframe As New StackFrame(Me.stackFrame)

            For Each value As Object In getSequence(envir)
                stackframe.Method.Method = $"for_loop_[{++i}]"
                value = RunLoop(value, stackframe, envir)

                Yield value
            Next
        End Function
    End Class
End Namespace
