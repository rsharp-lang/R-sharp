#Region "Microsoft.VisualBasic::30432a0e2cada9bfb6b7f91d47d3e82d, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DeclareNewFunction.vb"

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

'     Class DeclareNewFunction
' 
'         Properties: funcName, type
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: Evaluate, Invoke, ToString
' 
'         Sub: getExecBody, getParameters
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
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 普通的函数定义模型
    ''' 
    ''' 普通的函数与lambda函数<see cref="DeclareLambdaFunction"/>在结构上是一致的，
    ''' 但是有一个区别就是lambda函数<see cref="DeclareLambdaFunction"/>是没有<see cref="Environment"/>的，
    ''' 所以lambda函数会更加的轻量化，不容易产生内存溢出的问题
    ''' </summary>
    Public Class DeclareNewFunction : Inherits Expression
        Implements RFunction

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Property funcName As String Implements RFunction.name

        Friend params As DeclareNewVariable()
        Friend body As ClosureExpression
        ''' <summary>
        ''' The environment of current function closure
        ''' </summary>
        Friend envir As Environment

        Sub New()
        End Sub

        Sub New(code As List(Of Token()))
            Dim [declare] = code(4)
            Dim parts = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart = parts(Scan0).Skip(1).ToArray
            Dim bodyPart = parts(2).Skip(1).ToArray

            funcName = code(1)(Scan0).text

            Call getParameters(paramPart)
            Call getExecBody(bodyPart)
        End Sub

        Private Sub getParameters(tokens As Token())
            Dim parts = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            params = parts _
                .Select(Function(t)
                            Dim [let] As New List(Of Token) From {
                                New Token With {.name = TokenType.keyword, .text = "let"}
                            }
                            Return New DeclareNewVariable([let] + t)
                        End Function) _
                .ToArray
        End Sub

        Private Sub getExecBody(tokens As Token())
            body = New ClosureExpression(tokens)
        End Sub

        Friend Shared Function MissingParameters(var As DeclareNewVariable, funcName$, envir As Environment) As Object
            Dim message As String() = {
                $"argument ""{var.names.GetJson}"" is missing, with no default",
                $"function: {funcName}",
                $"parameterName: {var.names.GetJson}",
                $"type: {var.type.Description}"
            }

            Return Internal.stop(message, envir)
        End Function

        Public Function Invoke(parent As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim var As DeclareNewVariable
            Dim value As Object
            Dim arguments As Dictionary(Of String, Object)
            Dim envir As Environment = Me.envir

            If envir Is Nothing Then
                envir = parent
            Else
                envir = New Environment(parent, Me.funcName)
            End If

            Dim argumentKeys As String()
            Dim key$

            ' function parameter should be evaluate 
            ' from the parent environment.
            arguments = InvokeParameter.CreateArguments(parent, params)
            argumentKeys = arguments.Keys.ToArray

            ' initialize environment
            For i As Integer = 0 To Me.params.Length - 1
                var = Me.params(i)

                If arguments.ContainsKey(var.names(Scan0)) Then
                    value = arguments(var.names(Scan0))
                ElseIf i >= params.Length Then
                    ' missing, use default value
                    If var.hasInitializeExpression Then
                        value = var.value.Evaluate(envir)
                    Else
                        Return MissingParameters(var, funcName, envir)
                    End If
                Else
                    key = "$" & i

                    If arguments.ContainsKey(key) Then
                        value = arguments(key)
                    Else
                        ' symbol :> func
                        ' will cause parameter name as symbol name
                        ' produce key not found error
                        ' try to fix such bug
                        value = arguments(argumentKeys(i))
                    End If
                End If

                ' 20191120 对于函数对象而言，由于拥有自己的环境，在构建闭包之后
                ' 多次调用函数会重复利用之前的环境参数
                ' 所以在这里只需要判断一下更新值或者插入新的变量
                If var.names.Any(AddressOf envir.variables.ContainsKey) Then
                    ' 只检查自己的环境中的变量
                    ' 因为函数参数是只属于自己的环境之中的符号
                    Dim names As Literal() = var.names _
                        .Select(Function(name) New Literal(name)) _
                        .ToArray

                    Call ValueAssign.doValueAssign(envir, names, True, value)
                Else
                    ' 不存在，则插入新的
                    Call DeclareNewVariable.PushNames(var.names, value, var.type, envir)
                End If
            Next

            Return body.Evaluate(envir)
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result = envir.Push(funcName, Me, TypeCodes.closure)
            Me.envir = New Environment(envir, funcName)
            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"declare function '${funcName}'({params.Select(AddressOf DeclareNewVariable.getParameterView).JoinBy(", ")})"
        End Function
    End Class
End Namespace
