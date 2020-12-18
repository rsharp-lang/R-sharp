#Region "Microsoft.VisualBasic::c878271cf68240959978ad07516e1880, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\ByRefFunctionCall.vb"

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

'     Class ByRefFunctionCall
' 
'         Properties: type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, getTargetFunction, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' ``func(a) &lt;- value``
    ''' </summary>
    Public Class ByRefFunctionCall : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return value.type
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Friend ReadOnly funcRef As Expression
        Friend ReadOnly target As Expression
        Friend ReadOnly value As Expression

        Sub New(invoke As Expression, value As Expression, stackFrame As StackFrame)
            Dim target As FunctionInvoke = invoke

            Me.value = value
            Me.funcRef = target.funcName
            Me.target = target.parameters(Scan0)
            Me.stackFrame = stackFrame
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' only supprts .NET api with attribute 
            ' <RByRefValueAssignAttribute>
            Dim funcVar As Object = getTargetFunction(envir)

            If funcVar Is Nothing Then
                ' symbol not found
                Return REnv.debug.stop($"Function symbol not found base on the evaluation: '{funcRef.ToString}'", envir)
            ElseIf funcVar.GetType Is GetType(Message) Then
                Return funcVar
            End If

            If Not funcVar.GetType Is GetType(RMethodInfo) Then
                Return REnv.debug.stop(New NotSupportedException("Only supports .NET api function with custom attribute <RByRefValueAssignAttribute> tagged!"), envir)
            End If

            Dim api As RMethodInfo = funcVar
            Dim [byref] As RMethodArgument = api.parameters.FirstOrDefault(Function(a) a.isByrefValueParameter)

            If [byref] Is Nothing Then
                Return REnv.debug.stop(New NotSupportedException($"api '{api}' is not supports byref calls!"), envir)
            Else
                Return api.Invoke(envir, InvokeParameter.Create(expressions:={target, value}))
            End If
        End Function

        Private Function getTargetFunction(env As Environment) As Object
            Dim var As Object = FunctionInvoke.getFuncVar(funcRef, Nothing, env)

            If var Is Nothing Then
                var = REnv.invoke.getFunction(DirectCast(funcRef, Literal).value)
            ElseIf var.GetType Is GetType(Message) Then
                Return var
            End If

            Return var
        End Function

        Public Overrides Function ToString() As String
            Return $"<byref> {funcRef}({target}) <- {value}"
        End Function
    End Class
End Namespace
