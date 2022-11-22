#Region "Microsoft.VisualBasic::94121ff0ae39c8db84b5168c7149fb70, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\FunctionInvoke.vb"

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

'   Total Lines: 434
'    Code Lines: 266
' Comment Lines: 119
'   Blank Lines: 49
'     File Size: 17.42 KB


'     Class FunctionInvoke
' 
'         Properties: [namespace], expressionName, funcName, parameters, stackFrame
'                     type
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: allIsValueAssign, CheckInvoke, doInvokeFuncVar, EnumerateInvokedParameters, Evaluate
'                   (+2 Overloads) GetFunctionVar, getFuncVar, HandleResult, invokeRInternal, runOptions
'                   ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' Invoke a function symbol
    ''' 
    ''' ```
    ''' call xxx(...)
    ''' ```
    ''' </summary>
    Public Class FunctionInvoke : Inherits Expression
        Implements IRuntimeTrace

        Public ReadOnly Property funcName As Expression
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.FunctionCall
            End Get
        End Property

        ''' <summary>
        ''' The namespace reference
        ''' </summary>
        ''' <returns></returns>
        Public Property [namespace] As String

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        ''' <summary>
        ''' The parameters expression that passing to the target invoked function.
        ''' </summary>
        Public Property parameters As Expression()

        ''' <summary>
        ''' Use for create pipeline calls from identifier target
        ''' </summary>
        ''' <param name="funcName"></param>
        ''' <param name="parameters"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(funcName$, stackFrame As StackFrame, ParamArray parameters As Expression())
            Call Me.New(New Literal(funcName), stackFrame, parameters)
        End Sub

        Sub New(copy As FunctionInvoke)
            Me.funcName = copy.funcName
            Me.stackFrame = copy.stackFrame
            Me.namespace = copy.namespace
            Me.parameters = copy.parameters.ToArray
        End Sub

        ''' <summary>
        ''' Use for create pipeline calls from identifier target
        ''' </summary>
        ''' <param name="funcVar"></param>
        ''' <param name="parameters"></param>
        Sub New(funcVar As Expression, stackFrame As StackFrame, ParamArray parameters As Expression())
            Me.funcName = funcVar
            Me.parameters = parameters.ToArray
            Me.stackFrame = New StackFrame With {
                .File = stackFrame.File,
                .Line = stackFrame.Line,
                .Method = New Method With {
                    .[Module] = stackFrame.Method.Module,
                    .[Namespace] = stackFrame.Method.Namespace,
                    .Method = $"{stackFrame.Method.Method}({ Mid(parameters.JoinBy(", ").TrimNewLine, 1, 32)}...)"
                }
            }
        End Sub

        Public Iterator Function EnumerateInvokedParameters() As IEnumerable(Of Expression)
            For Each a In parameters
                Yield a
            Next
        End Function

        Public Overrides Function ToString() As String
            If [namespace].StringEmpty OrElse [namespace] = "n/a" Then
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

        ''' <summary>
        ''' call R#/python/julia function
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As Object
            Dim checked As Boolean = False
            Dim callFunc As Object = CheckInvoke(envir, checked)
            Dim target As Object

            If TypeOf callFunc Is Message Then
                Return callFunc
            ElseIf TypeOf callFunc Is String Then
                target = Nothing
            Else
                target = callFunc
            End If

            Using env As New Environment(envir, stackFrame, isInherits:=True)
                If TypeOf target Is Regex Then
                    ' regexp match
                    result = Regexp.Matches(target, parameters(Scan0), env)
                ElseIf target Is Nothing AndAlso TypeOf funcName Is Literal Then
                    ' 可能是一个系统的内置函数
                    result = invokeRInternal(DirectCast(funcName, Literal).ValueStr, env)
                Else
                    result = doInvokeFuncVar(target, env)
                End If

                Return HandleResult(result, envir)
            End Using
        End Function

        Public Function CheckInvoke(envir As Environment, ByRef passed As Boolean) As Object
            Dim target As Object = getFuncVar(funcName, [namespace], envir)

            passed = False

            If target Is Nothing Then
                ' is system internal callable method
                If TypeOf funcName Is Literal Then
                    ' string/r internal function
                    Return DirectCast(funcName, Literal).ValueStr
                Else
                    ' message
                    Return Message.InCompatibleType(GetType(String), funcName.GetType, envir)
                End If
            ElseIf target.GetType Is GetType(Message) Then
                ' message
                Return target
            ElseIf Not target.GetType.ImplementInterface(Of RFunction) Then
                If Not TypeOf target Is Regex Then
                    ' message
                    Return Internal.debug.stop({
                        $"the given symbol is not callable!",
                        $"target: {funcName.ToString}",
                        $"schema: {target.GetType.FullName}"
                    }, envir)
                Else
                    passed = True
                    ' regex
                    Return target
                End If
            Else
                passed = True
                ' rfunction
                Return target
            End If
        End Function

        ''' <summary>
        ''' check the function invoke returns values for:
        '''
        ''' 1. error message
        ''' 2. invisible
        ''' 3. returns expression, break expression
        ''' 
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function HandleResult(result As Object, env As Environment) As Object
            If result Is Nothing Then
                Return Nothing
            ElseIf Program.isException(result) Then
                Return result
            ElseIf TypeOf result Is RReturn Then
                Dim returns As RReturn = DirectCast(result, RReturn)
                Dim messages = env.globalEnvironment.messages

                If returns.HasValue Then
                    messages.AddRange(returns.messages)
                    Return returns.Value
                ElseIf returns.isError Then
                    returns.messages _
                        .Where(Function(m) m.level <> MSG_TYPES.ERR) _
                        .DoCall(AddressOf messages.AddRange)

                    Return returns.messages.Where(Function(m) m.level = MSG_TYPES.ERR)
                Else
                    ' 2019-12-15
                    ' isError的时候也会导致hasValue为false
                    ' 所以null的情况不可以和warning的情况合并在一起处理
                    messages.AddRange(returns.messages)
                    Return Nothing
                End If
            ElseIf TypeOf result Is pipeline AndAlso DirectCast(result, pipeline).isError Then
                Return DirectCast(result, pipeline).getError
            Else
                Return result
            End If
        End Function

        ''' <summary>
        ''' This function maybe returns <see cref="RFunction"/>, nothing or exception <see cref="Message"/> 
        ''' if the namespace reference function is not found or unable to load library module.
        ''' </summary>
        ''' <param name="funcName"></param>
        ''' <param name="namespace"></param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' This function returns a R api function or regex object for do string matches
        ''' 
        ''' + 1. <see cref="RFunction"/>
        ''' + 2. <see cref="Regex"/>
        ''' </returns>
        Friend Shared Function getFuncVar(funcName As Expression, namespace$, envir As Environment) As Object
            ' 当前环境中的函数符号的优先度要高于
            ' 系统环境下的函数符号
            Dim funcVar As Object

            If (Not [namespace].StringEmpty) AndAlso (Not [namespace] = "n/a") Then
                If [namespace] <> ".Internal" Then
                    ' 20220512
                    ' 
                    ' ignore ``.Internal`` namespace
                    ' do calls of internal invoke for makes bug fixed
                    ' of the function overrides that comes from the
                    ' external package
                    Return NamespaceFunctionSymbolReference.getPackageApiImpl(
                        env:=envir,
                        [namespace]:=[namespace],
                        funcNameSymbol:=funcName
                    )
                Else
                    Dim funcStr As Object = NamespaceFunctionSymbolReference.getFuncNameSymbolText(funcName, envir)

                    If TypeOf funcStr Is Message Then
                        Return funcStr
                    End If

                    ' .Internal::function
                    ' means should found the function from the internal
                    ' base environment directly
                    Dim method As RMethodInfo = Internal.invoke.getFunction(funcStr)

                    If method Is Nothing Then
                        Return Message.SymbolNotFound(envir, funcStr, TypeCodes.closure)
                    Else
                        Return method
                    End If
                End If
            End If

            If TypeOf funcName Is Literal Then
                Dim symbolName As String = DirectCast(funcName, Literal).ValueStr
                Dim symbol As Symbol = envir.FindFunction(symbolName)

                If symbol Is Nothing Then
                    symbol = envir.FindSymbol(symbolName)

                    If Not symbol Is Nothing Then
                        ' is regular expression:  $"xxx"(string)
                        ' or is invokable: xxx(yyy)
                        If symbol.typeof Is GetType(Regex) OrElse symbol.typeof.ImplementInterface(Of RFunction) Then
                            funcVar = symbol.value
                        Else
                            funcVar = Nothing
                        End If
                    Else
                        funcVar = Nothing
                    End If
                Else
                    funcVar = symbol.value
                End If
            Else
                funcVar = funcName.Evaluate(envir)
            End If

            Return funcVar
        End Function

        ''' <summary>
        ''' get internal function symbol object or runtime function 
        ''' from the given runtime <see cref="Environment"/>.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="funcName">
        ''' 这个参数不会处理ns::func的引用情况，请在调用这个函数之前完成解析
        ''' </param>
        ''' <returns>
        ''' an error message will be return if the 
        ''' target function symbol is not found in 
        ''' the given runtime <paramref name="env"/>.
        ''' </returns>
        Public Shared Function GetFunctionVar(funcName As Expression, env As Environment, Optional [namespace] As String = Nothing) As Object
            Dim target As Object = getFuncVar(funcName, [namespace], env)

            If Program.isException(target) Then
                Return target
            End If

            If target Is Nothing AndAlso TypeOf funcName Is Literal Then
                Dim funcStr = DirectCast(funcName, Literal).ValueStr
                ' 可能是一个系统的内置函数
                Dim method As RMethodInfo = Internal.invoke.getFunction(funcStr)

                If method Is Nothing Then
                    Return Message.SymbolNotFound(env, funcStr, TypeCodes.closure)
                Else
                    Return method
                End If
            Else
                Return target
            End If
        End Function

        ''' <summary>
        ''' get internal function symbol object or runtime function 
        ''' from the given runtime <see cref="Environment"/>.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFunctionVar(env As Environment) As Object
            Return GetFunctionVar(funcName, env, [namespace])
        End Function

        ''' <summary>
        ''' invoke runtime function or .NET methodinfo
        ''' </summary>
        ''' <param name="funcVar"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Friend Function doInvokeFuncVar(funcVar As RFunction, envir As Environment) As Object
            If funcVar.GetType Like runtimeFuncs Then
                ' invoke method create from R# script
                ' end of user function invoke
                ' for break the internal function closure loop
                ' due to the reason of in program closure executation
                ' loop, then returns result will be wrapped as return runtime literal value
                ' so we needs to break such wrapper at here
                ' or ctype error will happends
                Dim arguments As InvokeParameter() = InvokeParameter.Create(expressions:=parameters)
                Dim result As Object = DirectCast(funcVar, RFunction).Invoke(envir, arguments)

                If Not result Is Nothing Then
                    If result.GetType Is GetType(ReturnValue) AndAlso DirectCast(result, ReturnValue).IsRuntimeFunctionReturnWrapper Then
                        Return DirectCast(result, ReturnValue).Evaluate(Nothing)
                    Else
                        Return result
                    End If
                Else
                    Return Nothing
                End If
            Else
                Dim args = InvokeParameter.Create(expressions:=parameters)
                Dim interop As RMethodInfo = DirectCast(funcVar, RMethodInfo)

                ' invoke .NET package method
                Return interop.Invoke(envir, args)
            End If
        End Function

        Friend Function invokeRInternal(funcName$, envir As Environment) As Object
            If funcName = "options" AndAlso Not parameters.DoCall(AddressOf allIsValueAssign) Then
                Return runOptions(envir)
            Else
                ' create argument models
                Dim argVals As InvokeParameter() = InvokeParameter.Create(expressions:=parameters)
                ' and then invoke the specific internal R# api
                Dim result As Object = Internal.invoke.invokeInternals(envir, funcName, argVals)

                Return result
            End If
        End Function

        Private Function runOptions(env As Environment) As Object
            Dim names As String()

            If parameters.Count = 1 Then
                Dim firstInput = parameters(Scan0).Evaluate(env)

                If firstInput.GetType Is GetType(list) Then
                    Return base.options(firstInput, env)
                ElseIf Program.isException(firstInput) Then
                    Return firstInput
                Else
                    names = Runtime.asVector(Of String)(firstInput)
                End If
            Else
                Dim vector As Object() = parameters _
                    .Select(Function(exp)
                                Return exp.Evaluate(env)
                            End Function) _
                    .ToArray

                For Each element As Object In vector
                    If Program.isException(element) Then
                        Return element
                    End If
                Next

                names = Runtime.asVector(Of String)(vector)
            End If

            Return base.options(names, env)
        End Function

        <DebuggerStepThrough>
        Private Shared Function allIsValueAssign(parameters As IEnumerable(Of Expression)) As Boolean
            Return parameters.All(Function(e) TypeOf e Is ValueAssignExpression)
        End Function
    End Class
End Namespace
