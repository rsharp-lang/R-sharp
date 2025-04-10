#Region "Microsoft.VisualBasic::e56680189dff08a1c970df216fe00009, studio\Rsharp_IL\npy\Expressions\PipelineFunction.vb"

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

'   Total Lines: 90
'    Code Lines: 70 (77.78%)
' Comment Lines: 7 (7.78%)
'    - Xml Docs: 42.86%
' 
'   Blank Lines: 13 (14.44%)
'     File Size: 3.41 KB


' Class PipelineFunction
' 
'     Properties: callFunc, expressionName, pipeNextInvoke, type
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: doPipNextInvoke, Evaluate, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' obj.func(xxx)
''' </summary>
Public Class PipelineFunction : Inherits Expression

    Public Overrides ReadOnly Property type As TypeCodes
        Get
            Return TypeCodes.generic
        End Get
    End Property

    Public Overrides ReadOnly Property expressionName As ExpressionTypes
        Get
            Return ExpressionTypes.FunctionCall
        End Get
    End Property

    Public ReadOnly Property callFunc As FunctionInvoke

    Public ReadOnly Property pipeNextInvoke As Boolean
        Get
            Return TypeOf callFunc.funcName Is Literal AndAlso DirectCast(callFunc.funcName, Literal).ValueStr.Contains("."c)
        End Get
    End Property

    Sub New(calls As FunctionInvoke)
        callFunc = calls
    End Sub

    Public Overrides Function Evaluate(envir As Environment) As Object
        Dim result As Object
        Dim checked As Boolean = False
        Dim callFunc As Object = Me.callFunc.CheckInvoke(envir, checked)
        Dim target As Object

        If TypeOf callFunc Is Message Then
            ' method not found?
            If pipeNextInvoke Then
                result = doPipNextInvoke(DirectCast(Me.callFunc.funcName, Literal).ValueStr, envir)
                Return FunctionInvoke.HandleResult(result, envir)
            Else
                Return callFunc
            End If
        ElseIf TypeOf callFunc Is String AndAlso Not RInternal.invoke.isRInternal(callFunc) Then
            result = doPipNextInvoke(callFunc, envir)
            Return FunctionInvoke.HandleResult(result, envir)
        Else
            target = callFunc
        End If

        Using env As New Environment(envir, Me.callFunc.stackFrame, isInherits:=True)
            If TypeOf target Is Regex Then
                ' regexp match
                Dim parameters = Me.callFunc.parameters
                Dim drop_opt As Object = InvokeParameter.GetArgumentValue(parameters, "drop", 1, [default]:=False, envir)

                If Program.isException(drop_opt) Then
                    Return drop_opt
                End If

                result = Regexp.Matches(
                    r:=target,
                    text:=parameters(Scan0),
                    drop:=CLRVector.asLogical(drop_opt).ElementAtOrDefault(Scan0, [default]:=False),
                    env:=env
                )
            ElseIf TypeOf target Is String Then
                ' 可能是一个系统的内置函数
                result = Me.callFunc.invokeRInternal(DirectCast(target, String), envir)
            Else
                ' is runtime function
                result = Me.callFunc.doInvokeFuncVar(target, env)
            End If

            Return FunctionInvoke.HandleResult(result, envir)
        End Using
    End Function

    Private Function doPipNextInvoke(nameStr As String, env As Environment) As Object
        Dim objMethod = nameStr.GetTagValue(".")
        Dim obj As String = objMethod.Name
        Dim method As String = objMethod.Value
        Dim params = New Expression() {New SymbolReference(obj)}.JoinIterates(callFunc.parameters).ToArray
        Dim newInvoke As New FunctionInvoke(method, callFunc.stackFrame, params)

        Return newInvoke.Evaluate(env)
    End Function

    Public Overrides Function ToString() As String
        Return callFunc.ToString
    End Function
End Class
