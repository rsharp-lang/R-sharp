﻿#Region "Microsoft.VisualBasic::716a324153e79ab2221dc2535258db84, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb"

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

    '   Total Lines: 222
    '    Code Lines: 152 (68.47%)
    ' Comment Lines: 39 (17.57%)
    '    - Xml Docs: 89.74%
    ' 
    '   Blank Lines: 31 (13.96%)
    '     File Size: 8.33 KB


    '     Class ForLoop
    ' 
    '         Properties: body, expressionName, sequence, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, exec, execParallel, GetLoopTagLabel, getSequence
    '                   RunLoop, ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime
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
        Friend ReadOnly variables$()

        Public ReadOnly Property sequence As Expression

        ''' <summary>
        ''' 为了兼容tuple的赋值，在这里这个函数体就没有参数了
        ''' </summary>
        Public ReadOnly Property body As DeclareNewFunction

        ''' <summary>
        ''' ``%dopar%``
        ''' </summary>
        Friend ReadOnly parallel As Boolean = False

        ''' <summary>
        ''' 循环体的返回值类型就是for循环的返回值类型
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return body.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.For
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(variables$(),
                sequence As Expression,
                body As DeclareNewFunction,
                parallel As Boolean,
                stackframe As StackFrame)

            Me.variables = variables
            Me.sequence = sequence
            Me.body = body
            Me.parallel = parallel
            Me.stackFrame = stackframe
        End Sub

        ''' <summary>
        ''' run for loop at here
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As New List(Of Object)
            Dim runLoop As Func(Of Environment, Object)

            If parallel Then
                runLoop = AddressOf execParallel
            Else
                runLoop = AddressOf exec
            End If

            ' R 语言中的for循环其实就是一个for each循环
            For Each item As Object In envir.DoCall(runLoop)
                If TypeOf item Is BreakLoop Then
                    Exit For
                ElseIf Program.isException(item) Then
                    ' is error message
                    Return item
                ElseIf TypeOf item Is ReturnValue Then
                    Return item
                ElseIf Not TypeOf item Is ContinuteFor Then
                    result += REnv.single(item)
                End If
            Next

            Return REnv.TryCastGenericArray(result.ToArray, envir)
        End Function

        Public Overrides Function ToString() As String
            Return $"
for (let {variables.GetJson} in {sequence}) {If(parallel, "%dopar%", "%do%")} {{
   # forloop_internal
{body.body.Indent}
}}"
        End Function

        Private Function execParallel(envir As Environment) As IEnumerable(Of Object)
            Dim result As IEnumerable(Of Object) = getSequence(envir) _
                .AsParallel _
                .Select(Function(value, i)
                            Dim stackframe As New StackFrame(Me.stackFrame)
                            Dim label As String = GetLoopTagLabel(i, value)
                            stackframe.Method.Method = $"parallel_for_loop_[{label}]"
                            Return RunLoop(value, stackframe, envir)
                        End Function)

            Return result
        End Function

        Private Shared Function GetLoopTagLabel(i As Integer, value As Object) As String
            If value Is Nothing Then
                Return $"[{i}] <NULL>"
            ElseIf TypeOf value Is String Then
                Return $"[{i}]: ""{Mid(CStr(value), 1, 64)}{If(CStr(value).Length > 64, "...", "")}"""
            ElseIf DataFramework.IsPrimitive(type:=value.GetType) Then
                Return $"[{i}]: {value}"
            Else
                Return i.ToString
            End If
        End Function

        ''' <summary>
        ''' run a single loop
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="stackframe"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function RunLoop(value As Object, stackframe As StackFrame, env As Environment) As Object
            Dim err As Message = Nothing

            ' make a copy of the given runtime environment
            ' and then push internal loop symbols
            Using closure As Environment = DeclareNewSymbol.PushNames(
                    names:=variables,
                    value:=value,
                    type:=TypeCodes.generic,
                    envir:=New Environment(env, stackframe, isInherits:=False),
                    [readonly]:=False,
                    err:=err
                )

                If Not err Is Nothing Then
                    Return err
                End If

                Return body.body.Evaluate(closure)
            End Using
        End Function

        ''' <summary>
        ''' this function will cast dataframe to rows if
        ''' the variables is a multiple symbol tuple 
        ''' vector. 
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function getSequence(env As Environment) As IEnumerable(Of Object)
            Dim rawSeq As Object = sequence.Evaluate(env)
            Dim data As IEnumerable(Of Object)

            If TypeOf rawSeq Is Message Then
                Return {rawSeq}
            ElseIf TypeOf rawSeq Is dataframe AndAlso variables.Length > 1 Then
                Dim tmp = DirectCast(rawSeq, dataframe).listByRows(Nothing, env)

                If TypeOf tmp Is Message Then
                    Return {tmp}
                Else
                    data = DirectCast(tmp, list).slots _
                        .Values _
                        .ToArray
                End If
            ElseIf TypeOf rawSeq Is tqdmList Then
                Dim tqdm As tqdmList = rawSeq
                Dim pull As Func(Of IEnumerable(Of Object)) =
                    Iterator Function() As IEnumerable(Of Object)
                        For Each key As String In tqdm.getKeys
                            Yield tqdm(key)
                        Next
                    End Function

                data = pull()
            Else
                data = ObjectSet.GetObjectSet(rawSeq, env)
            End If

            Return data
        End Function

        Private Iterator Function exec(envir As Environment) As IEnumerable(Of Object)
            Dim i As i32 = 1
            Dim stackframe As New StackFrame(Me.stackFrame)

            For Each value As Object In getSequence(envir)
                stackframe.Method.Method = $"for_loop_[{GetLoopTagLabel(++i, value)}]"
                value = RunLoop(value, stackframe, envir)

                Yield value
            Next
        End Function
    End Class
End Namespace
