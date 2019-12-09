#Region "Microsoft.VisualBasic::ea769360f6532c26e4ced272dcf3b4cc, R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb"

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
'         Constructor: (+3 Overloads) Sub New
'         Function: Evaluate, invokePackageInternal, invokeRInternal, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RPackage = SMRUCC.Rsharp.Runtime.Package.Package

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Invoke a function symbol
    ''' 
    ''' ```
    ''' call xxx(...)
    ''' ```
    ''' </summary>
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

        ''' <summary>
        ''' The parameters expression that passing to the target invoked function.
        ''' </summary>
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
                .AsList
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

            If Not [namespace].StringEmpty Then
                Return envir.DoCall(AddressOf invokePackageInternal)
            End If

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
                Dim funcStr = DirectCast(funcName, Literal).ToString
                ' 可能是一个系统的内置函数
                Return invokeRInternal(funcStr, envir)
            ElseIf funcVar.GetType Like runtimeFuncs Then
                ' invoke method create from R# script
                Return DirectCast(funcVar, RFunction).Invoke(envir, InvokeParameter.Create(parameters))
            Else
                Dim args = InvokeParameter.Create(parameters)
                Dim interop As RMethodInfo = DirectCast(funcVar, RMethodInfo)

                ' invoke .NET package method
                Return interop.Invoke(envir, args)
            End If
        End Function

        Private Function invokePackageInternal(envir As Environment) As Object
            ' find package and then load method
            Dim pkg As RPackage = envir.globalEnvironment.packages.FindPackage([namespace], Nothing)
            Dim funcName As String = DirectCast(Me.funcName, Literal).ToString

            If pkg Is Nothing Then
                Return Message.SymbolNotFound(envir, [namespace], TypeCodes.ref)
            Else
                Dim api As RMethodInfo = pkg.GetFunction(funcName)

                If api Is Nothing Then
                    Return Message.SymbolNotFound(envir, $"{[namespace]}::{funcName}", TypeCodes.closure)
                Else
                    Return api.Invoke(envir, InvokeParameter.Create(parameters))
                End If
            End If
        End Function

        Private Function invokeRInternal(funcName$, envir As Environment) As Object
            If funcName = "list" Then
                Return Runtime.Internal.Rlist(envir, parameters)
            ElseIf funcName = "options" Then
                If parameters.DoCall(AddressOf isOptionNames) Then
                    Dim names As String() = Runtime.asVector(Of String)(parameters(Scan0).Evaluate(envir))
                    Dim values As list = base.options(names, envir)

                    Return values
                Else
                    Return base.options(Runtime.Internal.Rlist(envir, parameters), envir)
                End If
            ElseIf funcName = "data.frame" Then
                Return Runtime.Internal.Rdataframe(envir, parameters)
            Else
                Dim argVals As InvokeParameter() = InvokeParameter.Create(parameters)
                Dim result As Object = Internal.invokeInternals(envir, funcName, argVals)

                Return result
            End If
        End Function

        Private Shared Function isOptionNames(parameters As List(Of Expression)) As Boolean
            Dim first As Expression = parameters.ElementAtOrDefault(Scan0)

            If first Is Nothing OrElse Not parameters = 1 Then
                Return False
            End If

            Return TypeOf first Is VectorLiteral OrElse
                   TypeOf first Is SymbolReference OrElse
                   TypeOf first Is SymbolIndexer
        End Function
    End Class
End Namespace
