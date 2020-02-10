#Region "Microsoft.VisualBasic::80c567895c2c15084d361d5e7574f130, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\DeclareLambdaFunction.vb"

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
    '         Properties: name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetPrintContent, Invoke, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public ReadOnly Property name As String Implements RFunction.name

        Dim parameter As DeclareNewVariable
        Dim closure As Expression

        Sub New(name$, parameter As DeclareNewVariable, closure As Expression)
            Me.name = name
            Me.parameter = parameter
            Me.closure = closure
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
            Using envir As New Environment(parent, name)
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
                    Return DeclareNewVariable _
                        .PushNames(names:=parameter.names,
                                   value:=arguments(Scan0).Evaluate(envir),
                                   type:=TypeCodes.generic,
                                   envir:=envir
                        ) _
                        .DoCall(AddressOf closure.Evaluate)
                End If
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Return $"``{name}``"
        End Function
    End Class
End Namespace
