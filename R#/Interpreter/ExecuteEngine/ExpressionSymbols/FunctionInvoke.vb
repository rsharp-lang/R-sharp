#Region "Microsoft.VisualBasic::159475166b99a3a437b853578795a84a, R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb"

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

'     Class FunctionInvoke
' 
'         Properties: [namespace], funcName, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        ''' <summary>
        ''' The source location of current function invoke calls
        ''' </summary>
        Dim span As CodeSpan

        Public ReadOnly Property funcName As Expression

        ''' <summary>
        ''' The namespace reference
        ''' </summary>
        ''' <returns></returns>
        Public Property [namespace] As String

        Friend ReadOnly parameters As List(Of Expression)

        Sub New(tokens As Token())
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            funcName = New Literal(tokens(Scan0).text)
            span = tokens(Scan0).span
            parameters = params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            Return Expression.CreateExpression(param)
                        End Function) _
                .ToList
        End Sub

        ''' <summary>
        ''' Use for create pipeline calls from identifier target
        ''' </summary>
        ''' <param name="funcName"></param>
        ''' <param name="parameters"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(funcName$, ParamArray parameters As Expression())
            Call Me.New(New Literal(funcName), parameters)
        End Sub

        ''' <summary>
        ''' Use for create pipeline calls from identifier target
        ''' </summary>
        ''' <param name="funcVar"></param>
        ''' <param name="parameters"></param>
        Sub New(funcVar As Expression, ParamArray parameters As Expression())
            Me.funcName = funcVar
            Me.parameters = parameters.ToList
        End Sub

        Public Overrides Function ToString() As String
            If [namespace].StringEmpty Then
                Return $"Call {funcName}({parameters.JoinBy(", ")})"
            Else
                Return $"Call {[namespace]}::{funcName}({parameters.JoinBy(", ")})"
            End If
        End Function

        ''' <summary>
        ''' These function create from script text in runtime
        ''' </summary>
        ReadOnly runtimeFuncs As Index(Of Type) = {
            GetType(DeclareNewFunction),
            GetType(DeclareLambdaFunction)
        }

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 当前环境中的函数符号的优先度要高于
            ' 系统环境下的函数符号
            Dim funcVar As RFunction

            If TypeOf funcName Is Literal Then
                Dim symbol = envir.FindSymbol(DirectCast(funcName, Literal).ToString)?.value

                If symbol Is Nothing Then
                    funcVar = Nothing
                    'ElseIf symbol.GetType Is GetType(Internal.envir) Then
                    '    funcVar = DirectCast(symbol, Internal.envir).declare
                Else
                    funcVar = symbol
                End If
            Else
                funcVar = funcName.Evaluate(envir)
            End If

            If funcVar Is Nothing AndAlso TypeOf funcName Is Literal Then
                ' 可能是一个系统的内置函数
                Return invokeRInternal(DirectCast(funcName, Literal).ToString, envir)
            ElseIf funcVar.GetType Like runtimeFuncs Then
                ' invoke method create from R# script
                Return DirectCast(funcVar, RFunction).Invoke(envir, InvokeParameter.Create(parameters))
            Else
                ' invoke .NET method
                Return DirectCast(funcVar, RMethodInfo).Invoke(envir, InvokeParameter.Create(parameters))
            End If
        End Function

        Private Function invokeRInternal(funcName$, envir As Environment) As Object
            If funcName = "list" Then
                Return Runtime.Internal.Rlist(envir, parameters)
            ElseIf funcName = "options" Then
                Return Runtime.Internal.options(Runtime.Internal.Rlist(envir, parameters), envir)
            ElseIf funcName = "data.frame" Then
                Return Runtime.Internal.Rdataframe(envir, parameters)
            Else
                Return Runtime.Internal.invokeInternals(envir, funcName, envir.Evaluate(parameters))
            End If
        End Function
    End Class
End Namespace
