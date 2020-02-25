#Region "Microsoft.VisualBasic::362f43236d7e228b58e121be4d52402c, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\DeclareLambdaFunction.vb"

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

'     Class DeclareLambdaFunction
' 
'         Properties: name, stackFrame, type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: CreateLambda, Evaluate, GetPrintContent, Invoke, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 只允许简单的表达式出现在这里
    ''' 并且参数也只允许出现一个
    ''' </summary>
    ''' <remarks>
    ''' lambda函数与普通函数相比，lambda函数是没有environment的
    ''' 所以lambda函数会更加的轻量化
    ''' </remarks>
    Public Class DeclareLambdaFunction : Inherits Expression
        Implements RFunction
        Implements RPrint
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Dim parameter As DeclareNewVariable
        Dim closure As Expression

        Public ReadOnly Property parameterNames As String()
            Get
                Return parameter.names
            End Get
        End Property

        Sub New(name$, parameter As DeclareNewVariable, closure As Expression, stackframe As StackFrame)
            Me.name = name
            Me.parameter = parameter
            Me.closure = closure
            Me.stackFrame = stackframe
        End Sub

        ''' <summary>
        ''' 返回的是一个<see cref="RFunction"/>
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me
        End Function

        Public Function Invoke(parent As Environment, arguments() As InvokeParameter) As Object Implements RFunction.Invoke
            Using envir As New Environment(parent, stackFrame, isInherits:=False)
                If parameter.names.Length = 0 Then
                    ' lambda function with no parameter
                    Return closure.Evaluate(envir)
                ElseIf arguments.Length = 0 AndAlso parameter.names.Length > 0 Then
                    ' no value for the required parameter
                    Return DeclareNewFunction.MissingParameters(parameter, name, envir)
                Else
                    ' lambda function only allows one parameter in 
                    ' its declaration
                    '
                    ' example as:
                    ' 
                    ' x -> x + 1;
                    ' [x, y] -> x + y;       # tuple element is still one parameter, so this is legal
                    ' [x, y, z] -> x * y *z; # tuple element is still one parameter, so this is legal
                    '
                    ' function invoke of the lambda function should be in syntax of
                    '
                    ' lambda(x)
                    ' lambda([x,y])
                    ' lambda([x,y,z])
                    '
                    ' Due to the reason of syntax rule of only allows one parameter.
                    '
                    Dim argVal As Object = arguments(Scan0).Evaluate(envir)
                    Dim env As Environment = DeclareNewVariable _
                        .PushNames(names:=parameter.names,
                                   value:=argVal,
                                   type:=TypeCodes.generic,
                                   envir:=envir,
                                   [readonly]:=True
                        )
                    Dim result As Object = closure.Evaluate(env)

                    Return result
                End If
            End Using
        End Function

        Public Function CreateLambda(Of T, Out)(parent As Environment) As Func(Of T, Out)
            Dim envir = New Environment(parent, stackFrame, isInherits:=False)
            Dim v As Symbol

            Call DeclareNewVariable _
                .PushNames(names:=parameter.names,
                           value:=Nothing,
                           type:=GetType(T).GetRTypeCode,
                           envir:=envir,
                           [readonly]:=False
            )

            v = envir.FindSymbol(parameter.names(Scan0), [inherits]:=False)

            Return Function(x As T) As Out
                       Dim result As Object

                       v.SetValue(x, envir)
                       result = closure.Evaluate(envir)

                       ' 20200210 对于lambda函数而言，其是运行时创建的函数
                       ' 返回的值很可能是一个向量
                       If Not result Is Nothing Then
                           Dim type As Type = result.GetType

                           If type.IsArray AndAlso type.GetElementType Is GetType(Out) Then
                               result = DirectCast(result, Array).GetValue(Scan0)
                           End If
                       End If

                       Return result
                   End Function
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Return $"``{name}``"
        End Function
    End Class
End Namespace
