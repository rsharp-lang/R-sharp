﻿#Region "Microsoft.VisualBasic::35b24fa7cf5d8be62cf0094d1d88a408, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\DeclareNewFunction.vb"

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

    '   Total Lines: 430
    '    Code Lines: 240 (55.81%)
    ' Comment Lines: 140 (32.56%)
    '    - Xml Docs: 52.86%
    ' 
    '   Blank Lines: 50 (11.63%)
    '     File Size: 19.22 KB


    '     Class DeclareNewFunction
    ' 
    '         Properties: [Namespace], body, expressionName, funcName, parameters
    '                     stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, getArguments, getReturns, GetSymbolName, InitializeEnvironment
    '                   (+2 Overloads) Invoke, MissingParameters, ToString
    ' 
    '         Sub: SetSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' ## Function Definition
    ''' 
    ''' An R function object. These functions provide the base mechanisms 
    ''' for defining new functions in the R language.
    ''' 
    ''' The names in an argument list can be back-quoted non-standard names 
    ''' (see ‘backquote’).
    ''' If value is missing, NULL is returned. If it is a single expression,
    ''' the value of the evaluated expression is returned. (The expression 
    ''' is evaluated as soon as return is called, in the evaluation frame 
    ''' of the function and before any on.exit expression is evaluated.)
    ''' If the end of a function is reached without calling return, the value
    ''' of the last evaluated expression is returned.
    ''' 
    ''' (普通的函数定义模型: 
    ''' 
    ''' 普通的函数与lambda函数<see cref="DeclareLambdaFunction"/>在结构上是一致的，
    ''' 但是有一个区别就是lambda函数<see cref="DeclareLambdaFunction"/>是没有<see cref="Environment"/>的，
    ''' 所以lambda函数会更加的轻量化，不容易产生内存溢出的问题)
    ''' </summary>
    ''' <remarks>
    ''' This type of function is not the only type in R: they are called 
    ''' closures (a name with origins in LISP) to distinguish them from 
    ''' primitive functions.
    ''' A closure has three components, its formals (its argument list), 
    ''' its body (expr in the ‘Usage’ section) and its environment which 
    ''' provides the enclosure of the evaluation frame when the closure is 
    ''' used.
    ''' There is an optional further component if the closure has been 
    ''' byte-compiled. This is not normally user-visible, but it indicated 
    ''' when functions are printed.
    ''' 
    ''' > Becker, R. A., Chambers, J. M. and Wilks, A. R. (1988) The New S Language.
    ''' Wadsworth &amp; Brooks/Cole.
    ''' 
    ''' A subclass of <see cref="SymbolExpression"/>
    ''' </remarks>
    Public Class DeclareNewFunction : Inherits SymbolExpression
        Implements RFunction
        Implements IRuntimeTrace
        Implements INamespaceReferenceSymbol

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.FunctionDeclare
            End Get
        End Property

        ''' <summary>
        ''' 这个是当前的这个函数的程序包来源名称，在运行时创建的函数的命令空间为``SMRUCC/R#_runtime``
        ''' </summary>
        ''' <returns></returns>
        Public Property [Namespace] As String = SyntaxBuilderOptions.R_runtime Implements INamespaceReferenceSymbol.namespace

        Public ReadOnly Property funcName As String Implements RFunction.name
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame
        Public ReadOnly Property parameters As DeclareNewSymbol()
        Public ReadOnly Property body As ClosureExpression

        ''' <summary>
        ''' The environment of current function closure, this environment
        ''' context is comes from the environment where this function 
        ''' symbol is created.
        ''' 
        ''' this environment context holder make the closure it works for 
        ''' holding the executation context
        ''' </summary>
        Friend envir As Environment
        ''' <summary>
        ''' does the parameter set of this function contains a ``...`` 
        ''' object list parameter?
        ''' </summary>
        Friend hasObjectList As Boolean

        Sub New(funcName$, parameters As DeclareNewSymbol(), body As ClosureExpression, stackframe As StackFrame)
            Me.funcName = funcName
            Me.parameters = parameters
            Me.body = body
            Me.body.program.Rscript = Rscript.FromText(funcName)
            Me.stackFrame = stackframe
            Me.hasObjectList = parameters.Any(Function(p) p.m_names(0) = "...")
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetSymbolName() As String
            Return funcName
        End Function

        ''' <summary>
        ''' renames current function object its symbol name
        ''' </summary>
        ''' <param name="newName"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub SetSymbol(newName As String)
            _funcName = newName
        End Sub

        Public Iterator Function getArguments() As IEnumerable(Of NamedValue(Of Expression)) Implements RFunction.getArguments
            For Each arg As DeclareNewSymbol In Me.parameters
                For Each name As String In arg.names
                    Yield New NamedValue(Of Expression) With {
                        .Name = name,
                        .Value = arg.value
                    }
                Next
            Next
        End Function

        Friend Shared Function MissingParameters(var As DeclareNewSymbol, funcName$, envir As Environment) As Object
            Dim names As String() = var.names.ToArray
            Dim message As String() = {
                $"argument ""{names.GetJson}"" is missing, with no default",
                $"function: {funcName}",
                $"parameterName: {names.GetJson}",
                $"type: {var.type.Description}"
            }

            Return Internal.debug.stop(message, envir)
        End Function

        ''' <summary>
        ''' set parameters
        ''' </summary>
        ''' <param name="caller_context">
        ''' The environment context from the caller stack
        ''' </param>
        ''' <param name="params"></param>
        ''' <param name="runDispose">
        ''' A byref parameter to indicated that the resulted <see cref="Environment"/> should 
        ''' be release after this function has been successfully executated? 
        ''' </param>
        ''' <returns></returns>
        Private Function InitializeEnvironment(caller_context As Environment,
                                               params As InvokeParameter(),
                                               ByRef runDispose As Boolean) As [Variant](Of Message, Environment)
            Dim var As DeclareNewSymbol
            Dim value As Object
            Dim arguments As Dictionary(Of String, InvokeParameter)
            Dim envir As Environment = Me.envir

            If envir Is Nothing Then
                runDispose = True
                envir = New Environment(caller_context, $"R_invoke${Me.funcName}", isInherits:=False)
            Else
                runDispose = True
                envir = New ClosureEnvironment(caller_context, ClosureEnvironment.renameFrame(Me.stackFrame, $"R_invoke${Me.funcName}"), envir)
            End If

            Dim argumentKeys As String()
            Dim dotdotdot As Dictionary(Of String, Object) = Nothing
            Dim hasDotDotDot As Boolean = False
            Dim requireKey As String

            If Me.parameters.Any(Function(par) par.names.Any(Function(si) si = "...")) Then
                hasDotDotDot = True
                dotdotdot = New Dictionary(Of String, Object)
            End If

            ' function parameter should be evaluate 
            ' from the parent environment.
            'With InvokeParameter.CreateArguments(parent, params, hasObjectList:=True)
            '    If .GetUnderlyingType Is GetType(Message) Then
            '        Return .TryCast(Of Message)
            '    Else
            '        arguments = .TryCast(Of Dictionary(Of String, Object))
            '        argumentKeys = arguments.Keys.ToArray
            '    End If
            'End With

            arguments = InvokeParameter.CreateArguments(caller_context, params, hasObjectList)
            argumentKeys = arguments.Keys.ToArray

            ' initialize environment, walk through all parameters of current function
            ' and then create the parameter values
            For i As Integer = 0 To Me.parameters.Length - 1
                var = Me.parameters(i)
                requireKey = var.names(Scan0)

                If requireKey = "..." Then
                    value = dotdotdot
                ElseIf arguments.ContainsKey(requireKey) Then
                    value = arguments(requireKey).Evaluate(caller_context)

                    If hasDotDotDot Then
                        dotdotdot(requireKey) = value
                    End If
                ElseIf i >= params.Length Then
                    ' missing, use default value
                    ' the optional default value not affects the ... parameter
                    If var.hasInitializeExpression Then
                        value = var.value.Evaluate(caller_context)
                    Else
                        Return DirectCast(MissingParameters(var, funcName, caller_context), Message)
                    End If
                Else
                    Dim key = "$" & i

                    If arguments.ContainsKey(key) Then
                        value = arguments(key).Evaluate(caller_context)
                    ElseIf var.hasInitializeExpression Then
                        value = var.value.Evaluate(caller_context)
                    ElseIf TypeOf params(i).value Is ValueAssignExpression Then
                        ' symbol :> func
                        ' will cause parameter name as symbol name
                        ' produce key not found error
                        ' try to fix such bug
                        ' value = arguments(argumentKeys(i))
                        Return Internal.debug.stop({$"argument '{var.names.First}' is required, but missing!", $"name: {var.names.First}"}, caller_context)
                    Else
                        key = argumentKeys(i)
                        value = arguments(key).Evaluate(caller_context)
                    End If
                End If

                ' removes global symbol to makes
                ' local symbols overrides of the
                ' global symbols
                'For Each name As String In var.names
                '    Call envir.Delete(name, seekParent:=False)
                'Next

                ' 20191120 对于函数对象而言，由于拥有自己的环境，在构建闭包之后
                ' 多次调用函数会重复利用之前的环境参数
                ' 所以在这里只需要判断一下更新值或者插入新的变量
                'If var.names.Any(AddressOf envir.symbols.ContainsKey) Then
                '    ' 只检查自己的环境中的变量
                '    ' 因为函数参数是只属于自己的环境之中的符号
                '    Dim names As Literal() = var.names _
                '        .Select(Function(name) New Literal(name)) _
                '        .ToArray

                '    Call ValueAssignExpression.doValueAssign(envir, names, True, value)
                'Else
                Dim err As Message = Nothing

                If Program.isException(value) Then
                    Return DirectCast(value, Message)
                End If

                Try
                    ' add parameter symbol into the environment context
                    ' of the target function invoke context
                    '
                    ' 20230616 the force type cast of the function invoke parameter
                    ' has some bug, example like, a parameter has no type constraint
                    ' a = "xxxxx" will lead cast any parameter invoke value force cast
                    ' to string type, this type cast will cause bugs
                    '
                    ' so we disable the type cast here, if the parameter has no type
                    ' constrain expression token
                    '
                    ' use the typecast from the symbol type constrain at here
                    ' not use the symbol type at here, due to the reason of symbol
                    ' type is affected by the default value expression its value
                    ' type
                    Dim typecast As TypeCodes = var.m_type

                    Call DeclareNewSymbol.PushNames(var.names, value, typecast, False, envir, err:=err)
                Catch ex As Exception
                    Return Internal.debug.stop(New Exception($"unknown create parameter symbol error: {var.m_names.FirstOrDefault}(value = {any.ToString(value)})!", ex), envir)
                End Try

                If Not err Is Nothing Then
                    Return err
                End If
                ' End If
            Next

            ' push the other parameters
            If hasDotDotDot Then
                For Each key As String In arguments.Keys.Where(Function(si) Not si.IsPattern("[$]\d+"))
                    If Not dotdotdot.ContainsKey(key) Then
                        value = arguments(key).Evaluate(caller_context)

                        If Program.isException(value) Then
                            Return DirectCast(value, Message)
                        Else
                            dotdotdot.Add(key, value)
                        End If
                    End If
                Next
            End If

            Return envir
        End Function

        Public Function Invoke(parent As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim runDispose As Boolean = False
            Dim env = InitializeEnvironment(parent, params, runDispose)
            Dim envir As Environment

            If env Like GetType(Message) Then
                Return env.TryCast(Of Message)
            Else
                ' unsure for the symbol conflicts
                ' envir = New Environment(env.TryCast(Of Environment), stackFrame, isInherits:=False)
                envir = env.TryCast(Of Environment)
            End If

            If runDispose Then
                Using envir
                    Return body.Evaluate(envir)
                End Using
            Else
                Return body.Evaluate(envir)
            End If
        End Function

        ''' <summary>
        ''' direct invoke of this R# function
        ''' </summary>
        ''' <param name="arguments"></param>
        ''' <param name="caller"></param>
        ''' <returns></returns>
        Public Function Invoke(arguments() As Object, caller As Environment) As Object Implements RFunction.Invoke
            Dim envir As Environment = Me.envir
            Dim argVal As Object
            Dim runDispose As Boolean = False

            If envir Is Nothing Then
                envir = caller
            Else
                runDispose = True
                ' envir = New Environment(parent, stackFrame, isInherits:=False) & envir
                envir = New ClosureEnvironment(
                    caller:=caller,
                    frame:=ClosureEnvironment.renameFrame(Me.stackFrame, $"R_invoke${Me.funcName}"),
                    closure_context:=envir
                )
            End If

            For i As Integer = 0 To parameters.Length - 1
                Dim names As Literal() = parameters(i).names _
                    .Select(Function(name) New Literal(name)) _
                    .ToArray

                For Each name As String In parameters(i).names
                    Call envir.Push(name, Nothing, [readonly]:=False)
                Next

                If i >= arguments.Length Then
                    If parameters(i).hasInitializeExpression Then
                        argVal = parameters(i).value.Evaluate(envir)

                        If Program.isException(argVal) Then
                            Return argVal
                        End If

                        Call ValueAssignExpression.doValueAssign(envir, names, True, argVal)
                    Else
                        Return MissingParameters(parameters(i), funcName, envir)
                    End If
                Else
                    Call ValueAssignExpression.doValueAssign(envir, names, True, arguments(i))
                End If
            Next

            If runDispose Then
                Using envir
                    Return body.Evaluate(envir)
                End Using
            Else
                Return body.Evaluate(envir)
            End If
        End Function

        ''' <summary>
        ''' just create a new function object in the environment context
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim symbol As Symbol = envir.funcSymbols.TryGetValue(funcName)

            If symbol Is Nothing Then
                envir.funcSymbols(funcName) = New Symbol(Me, TypeCodes.closure, is_readonly:=False) With {
                    .name = funcName
                }
            Else
                symbol.setValue(Me, envir)
            End If

            ' initialize of the internal closure environment
            ' where this function is created
            Me.envir = New Environment(envir, stackFrame, isInherits:=True)

            Return Me
        End Function

        ''' <summary>
        ''' Inspect of the function declaration content of current function closure object.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Dim par_indent As String = ",
                "

            Return $"
declare function '${funcName}'({parameters.Select(AddressOf DeclareNewSymbol.getParameterView).JoinBy(par_indent)}) {{
   # function_internal
{body.Indent}
}}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getReturns(env As Environment) As IRType Implements RFunction.getReturns
            Return RType.GetRSharpType(GetType(Object))
        End Function
    End Class
End Namespace
