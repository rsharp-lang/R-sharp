﻿#Region "Microsoft.VisualBasic::1f93f89ad51dd814c12000b84a01c7bb, R#\Runtime\Interop\RsharpApi\RMethodInfo.vb"

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

    '   Total Lines: 528
    '    Code Lines: 342 (64.77%)
    ' Comment Lines: 122 (23.11%)
    '    - Xml Docs: 82.79%
    ' 
    '   Blank Lines: 64 (12.12%)
    '     File Size: 22.34 KB


    '     Class RMethodInfo
    ' 
    '         Properties: [namespace], invisible, name, parameters, returns
    '                     stackFrame
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: createNormalArguments, CreateParameterArrayFromListArgument, GetArgumentOrdinal, getArguments, GetNetCoreCLRDeclaration
    '                   GetPackageInfo, GetPrintContent, GetRApiReturns, getReturns, GetUnionTypes
    '                   getValue, (+2 Overloads) Invoke, missingParameter, parseParameters, setArgumentIndex
    '                   ToString
    ' 
    '         Sub: setRuntimeTraceback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Interop

    ''' <summary>
    ''' Use for R# package method, a wrapper for the .NET clr function <see cref="MethodInfo"/>.
    ''' </summary>
    Public Class RMethodInfo : Inherits RsharpDataObject
        Implements RFunction, RPrint, INamespaceReferenceSymbol, IRuntimeTrace

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        ''' <summary>
        ''' the return type of current api method
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property returns As IRType
        ''' <summary>
        ''' A list of parameters of current .NET api that imported
        ''' from the external dll assembly file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property parameters As RMethodArgument()

        ''' <summary>
        ''' Do not print the value of this function on console
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property invisible As Boolean

        ''' <summary>
        ''' module namespace string that parsed from <see cref="PackageAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [namespace] As String Implements INamespaceReferenceSymbol.namespace
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetPackageInfo.namespace
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        ''' <summary>
        ''' <see cref="MethodInfo"/>
        ''' </summary>
        ReadOnly api As MethodInvoke
        ReadOnly argNames As Index(Of String)

        ''' <summary>
        ''' the location of the ``...`` list parameter
        ''' </summary>
        Friend ReadOnly listObjectMargin As ListObjectArgumentMargin = ListObjectArgumentMargin.none

        ''' <summary>
        ''' wrap the .net clr lambda expression
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="closure">
        ''' Runtime generated .NET method
        ''' </param>
        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.api = New MethodInvoke With {
                .method = closure.Method,
                .target = closure.Target
            }
            Me.returns = RApiReturnAttribute.MeasureRReturnInfo(closure.Method)
            Me.parameters = parseParameters(closure.Method, is_lambda:=True)
            Me.listObjectMargin = RArgumentList.objectListArgumentMargin(Me)
            Me.elementType = RType.any
            Me.argNames = setArgumentIndex(parameters)

            Call setRuntimeTraceback()
        End Sub

        ''' <summary>
        ''' Static method
        ''' </summary>
        ''' <param name="api"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(api As NamedValue(Of MethodInfo))
            Call Me.New(api.Name, api.Value, Nothing)
        End Sub

        ''' <summary>
        ''' construct a R# runtime function from a static clr function
        ''' </summary>
        ''' <param name="func"></param>
        Sub New(func As MethodInfo)
            Call Me.New(func.Name, func, Nothing)
        End Sub

        ''' <summary>
        ''' Construct a object class binding instance member function
        ''' </summary>
        ''' <param name="name">The symbol name of the given function</param>
        ''' <param name="closure"><see cref="MethodInfo"/> from parsing .NET dll module file.</param>
        ''' <param name="target">
        ''' for object instance method used only, nothing means static method
        ''' </param>
        Sub New(name$, closure As MethodInfo, target As Object)
            Me.name = name
            Me.api = New MethodInvoke With {.method = closure, .target = target}
            Me.returns = RApiReturnAttribute.MeasureRReturnInfo(closure)
            Me.parameters = parseParameters(closure, is_lambda:=False)
            Me.invisible = RSuppressPrintAttribute.IsPrintInvisible(closure)
            Me.listObjectMargin = RArgumentList.objectListArgumentMargin(Me)
            Me.elementType = RType.any
            Me.argNames = setArgumentIndex(parameters)

            Call setRuntimeTraceback()
        End Sub

        Private Shared Function setArgumentIndex(args As IEnumerable(Of RMethodArgument)) As Index(Of String)
            Return args.Select(Function(a) a.name).Indexing
        End Function

        Private Sub setRuntimeTraceback()
            Dim asm As Assembly = GetNetCoreCLRDeclaration.DeclaringType.Assembly
            Dim clr_func_stackFrame As New StackFrame With {
                .File = $"[{asm.ToString}]",
                .Line = "&Hx0" & api.method.GetHashCode.ToHexString,
                .Method = New Method With {
                    .Method = name,
                    .[Module] = "R#_clr_interop::",
                    .[Namespace] = GetPackageInfo.namespace
                }
            }

            _stackFrame = clr_func_stackFrame
        End Sub

        Public Iterator Function getArguments() As IEnumerable(Of NamedValue(Of Expression)) Implements RFunction.getArguments
            For Each arg As RMethodArgument In parameters
                Yield New NamedValue(Of Expression) With {
                    .Name = arg.name,
                    .Value = If(arg.isOptional, New RuntimeValueLiteral(arg.default), Nothing)
                }
            Next
        End Function

        Public Function GetArgumentOrdinal(name As String) As Integer
            Return argNames(name)
        End Function

        ''' <summary>
        ''' Gets the <see cref="MethodInfo"/> represented by the delegate.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function GetNetCoreCLRDeclaration() As MethodInfo
            Return api.method
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function will returns nothing if the attribute is not found
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function GetRApiReturns() As RApiReturnAttribute
            Return GetNetCoreCLRDeclaration.GetCustomAttribute(Of RApiReturnAttribute)()
        End Function

        Public Iterator Function GetUnionTypes() As IEnumerable(Of Type)
            Dim method As MethodInfo = GetNetCoreCLRDeclaration()

            If method.ReturnType Is Nothing OrElse method.ReturnType Is GetType(Void) Then
                Return
            End If

            If Not method.ReturnType Is GetType(Object) Then
                Yield method.ReturnType
            Else
                Dim attr = method.GetCustomAttribute(Of RApiReturnAttribute)()

                If Not attr Is Nothing Then
                    For Each type As Type In attr.returnTypes
                        Yield type
                    Next
                Else
                    Yield GetType(Object)
                End If
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPackageInfo() As Package
            Return GetNetCoreCLRDeclaration.DeclaringType.ParsePackage(strict:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Return markdown
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function parseParameters(method As MethodInfo, is_lambda As Boolean) As RMethodArgument()
            Return method _
                .GetParameters _
                .Select(Function(a) RMethodArgument.ParseArgument(a, is_lambda)) _
                .ToArray
        End Function

        ''' <summary>
        ''' direct invoke
        ''' </summary>
        ''' <param name="parameters"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function Invoke(parameters As Object(), env As Environment) As Object Implements RFunction.Invoke
            Dim result As Object

            For Each arg In parameters
                If Not arg Is Nothing AndAlso arg.GetType Is GetType(Message) Then
                    Return arg
                End If
            Next

            Dim showExpression As Boolean = env.globalEnvironment.debugMode AndAlso
                (env.globalEnvironment.debugLevel = DebugLevels.All OrElse env.globalEnvironment.debugLevel = DebugLevels.Stack)

            If showExpression Then
                Call VBDebugger.WriteLine($"[exec] {GetPackageInfo.namespace}::{name}", ConsoleColor.Cyan)
            End If

            If env.globalEnvironment.Rscript.debug Then
                result = api.Invoke(parameters)
            Else
                Try
                    result = api.Invoke(parameters)
                Catch ex As Exception
                    If Not ex.InnerException Is Nothing Then
                        ex = ex.InnerException
                    End If

                    Return Internal.debug.stop(ex, env)
                End Try
            End If

            If showExpression Then
                Call VBDebugger.WriteLine($"[finished] {GetPackageInfo.namespace}::{name}", ConsoleColor.Gray)
            End If

            If invisible Then
                Return New invisible With {
                    .value = result
                }
            Else
                Return result
            End If
        End Function

        Public Function CreateParameterArrayFromListArgument(envir As Environment, list As Dictionary(Of String, Object)) As Object()
            Dim parameters As New Dictionary(Of String, InvokeParameter)

            For Each p In list
                parameters(p.Key) = New InvokeParameter With {
                    .value = New RuntimeValueLiteral(p.Value)
                }
            Next

            Return createNormalArguments(envir, arguments:=parameters).ToArray
        End Function

        ''' <summary>
        ''' Invoke a R-sharp wrap .NET clr function
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="params">no orders, the parameters will be aligned in this clr function invoke process</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 1. do parameter alignment in this function
        ''' 2. and then invoke clr function
        ''' </remarks>
        Public Function Invoke(envir As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim parameters As New List(Of Object)

            Using env As New Environment(envir, stackFrame, isInherits:=True)
                ' do parameter alignment at first
                If listObjectMargin <> ListObjectArgumentMargin.none Then
                    For Each value As Object In RArgumentList.CreateObjectListArguments(Me, env, params)
                        If Program.isException(value) Then
                            Return value
                        Else
                            parameters.Add(value)
                        End If
                    Next
                Else
                    Dim callParams = InvokeParameter.CreateArguments(env, params, hasObjectList:=False)

                    'If callParams Like GetType(Message) Then
                    '    Return callParams.TryCast(Of Message)
                    'End If

                    For Each value As Object In createNormalArguments(env, callParams)
                        If Program.isException(value) Then
                            Return value
                        Else
                            Call parameters.Add(value)
                        End If
                    Next
                End If

                ' and finally invoke the .net clr function
                Return Invoke(parameters.ToArray, env)
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments">
        ''' required of replace dot(.) to underline(_)?
        ''' </param>
        ''' <returns></returns>
        Private Iterator Function createNormalArguments(envir As Environment, arguments As Dictionary(Of String, InvokeParameter)) As IEnumerable(Of Object)
            Dim arg As RMethodArgument
            Dim keys As String() = arguments.Keys.ToArray
            Dim nameKey As String
            Dim apiTrace As String = name

            'For Each value As Object In arguments.Values
            '    If Not value Is Nothing AndAlso value.GetType Is GetType(Message) Then
            '        Yield value
            '        Return
            '    End If
            'Next

            For i As Integer = 0 To Me.parameters.Length - 1
                arg = Me.parameters(i)

                If arguments.ContainsKey(arg.name) Then
                    If arg.islazyeval Then
                        Yield arguments(arg.name).value
                    Else
                        Yield getValue(arg, arguments(arg.name).Evaluate(envir), apiTrace, envir, False)
                    End If
                ElseIf arguments.ContainsKey(arg.name.Replace("_", ".")) Then
                    If arg.islazyeval Then
                        Yield arguments(arg.name.Replace("_", ".")).value
                    Else
                        Yield getValue(arg, arguments(arg.name.Replace("_", ".")).Evaluate(envir), apiTrace, envir, False)
                    End If
                ElseIf i >= arguments.Count Then
                    ' default value
                    If arg.type.raw Is GetType(Environment) Then
                        Yield envir
                    ElseIf arg.type.raw Is GetType(GlobalEnvironment) Then
                        Yield envir.globalEnvironment
                    ElseIf Not arg.isOptional Then
                        Yield missingParameter(arg, envir, name)
                    ElseIf TypeOf arg.default Is Expression Then
                        Yield DirectCast(arg.default, Expression).Evaluate(envir)
                    Else
                        Yield arg.default
                    End If
                Else
                    nameKey = $"${i}"

                    If arguments.ContainsKey(nameKey) Then
                        If arg.islazyeval Then
                            Yield arguments(nameKey).value
                        ElseIf arguments(nameKey).isFormula AndAlso Not arg.acceptFormula Then
                            Dim formula As FormulaExpression = arguments(nameKey).value
                            Dim var As Expression = formula.var

                            If Not TypeOf var Is SymbolReference Then
                                Yield Internal.debug.stop({
                                    $"other kind of the response variable is not supported when create variable name from a formula expression.",
                                    $"response_var: {var.ToString}"
                                }, envir)
                                Return
                            End If

                            ' 20221130
                            ' syntax sugar for do something like:
                            ' t.test(data.frame(...), y, t ~ a + b + c + x);
                            Call arguments.Add(DirectCast(var, SymbolReference).symbol, arguments(nameKey))

                            GoTo opt
                        Else
                            Yield getValue(arg, arguments(nameKey).Evaluate(envir), apiTrace, envir, False)
                        End If
                    Else
opt:                    If arg.isOptional Then
                            If TypeOf arg.default Is Expression Then
                                Yield DirectCast(arg.default, Expression).Evaluate(envir)
                            ElseIf arg.type.raw Is GetType(Environment) Then
                                Yield envir
                            ElseIf arg.type.raw Is GetType(GlobalEnvironment) Then
                                Yield envir.globalEnvironment
                            Else
                                Yield arg.default
                            End If
                        Else
                            Yield missingParameter(arg, envir, name)
                        End If
                    End If
                End If
            Next
        End Function

        Friend Shared Function missingParameter(arg As RMethodArgument, envir As Environment, funcName$) As Object
            Dim messages$() = {
                $"missing parameter value for '{arg.name}'!",
                $"parameter: {arg.name}",
                $"type: {arg.type.raw.FullName}",
                $"function: {funcName}",
                $"environment: {envir.ToString}"
            }
            Dim debug As Boolean = envir.globalEnvironment.debugMode

            Return Internal.debug.stop(messages, envir, suppress:=Not debug)
        End Function

        ''' <summary>
        ''' Get type converted object value for match the parameter type. 
        ''' </summary>
        ''' <param name="arg"></param>
        ''' <param name="value"></param>
        ''' <param name="trace"></param>
        ''' <param name="envir"></param>
        ''' <param name="trygetListParam">
        ''' Fix bugs for list arguments when the parameter input have no symbol name
        ''' In such situation, then type is mismatch due to the reason of invalid 
        ''' offset bugs
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' returns an error message when type cast error
        ''' </remarks>
        Friend Shared Function getValue(arg As RMethodArgument,
                                        value As Object,
                                        trace$,
                                        ByRef envir As Environment,
                                        trygetListParam As Boolean) As Object

            If Program.isException(value, envir) Then
                Return value
            ElseIf arg.type Like GetType(Environment) Then
                If value IsNot Nothing AndAlso value.GetType.IsInheritsFrom(GetType(Environment)) Then
                    Return value
                Else
                    Return envir
                End If
            ElseIf value Is Nothing Then
                Return Nothing
            ElseIf value.GetType Is arg.type.raw Then
                Return value
            End If

            If arg.type.isArray Then
                value = CObj(REnv.asVector(value, arg.type.GetRawElementType, env:=envir))

                If arg.type.raw Is GetType(Array) Then
                    Return value
                End If
            ElseIf arg.type.isCollection Then
                ' ienumerable
                value = value
            ElseIf Not arg.isRequireRawVector Then
                value = Runtime.getFirst(value)
            ElseIf arg.isRequireRawVector AndAlso Not arg.rawVectorFlag.vector Is Nothing Then
                Return Runtime.asVector(value, arg.rawVectorFlag.vector, envir)
            End If

            If arg.type Like GetType(Object) OrElse Program.isException(value) Then
                Return value
            End If

            Try
                If TypeOf value Is String AndAlso DataFramework.IsNumericType(arg.type.raw) Then
                    Dim valStr As String = DirectCast(value, String)

                    If valStr = "NULL" OrElse valStr = "null" OrElse valStr = "Null" Then
                        Return CTypeDynamic(0, arg.type.raw)
                    Else
                        Return CTypeDynamic(Double.Parse(valStr), arg.type.raw)
                    End If
                End If

                Return RCType.CTypeDynamic(value, arg.type.raw, env:=envir)
            Catch ex As Exception When trygetListParam
                Return GetType(Void)
            Catch ex As Exception
                Return Internal.debug.stop(New InvalidCastException("api: " & trace, ex), envir)
            End Try
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"Dim {name} As {api.ToString}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getReturns(env As Environment) As IRType Implements RFunction.getReturns
            Return returns
        End Function
    End Class
End Namespace
