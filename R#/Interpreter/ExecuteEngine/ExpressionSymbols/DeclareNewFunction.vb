#Region "Microsoft.VisualBasic::24362c3b7aa73fb2f8fc41712baeaf53, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DeclareNewFunction.vb"

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

        Public Function Invoke(parent As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Using envir As New Environment(parent, funcName)
                Dim var As DeclareNewVariable
                Dim value As Object
                Dim arguments As Dictionary(Of String, Object) = InvokeParameter.CreateArguments(envir, params)

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
                            Throw New MissingFieldException(var.names.GetJson)
                        End If
                    Else
                        value = arguments("$" & i)
                    End If

                    Call DeclareNewVariable.PushNames(var.names, value, var.type, envir)
                Next

                Return body.Evaluate(envir)
            End Using
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return envir.Push(funcName, Me, TypeCodes.closure)
        End Function

        Public Overrides Function ToString() As String
            Return $"declare function '${funcName}'"
        End Function
    End Class
End Namespace
