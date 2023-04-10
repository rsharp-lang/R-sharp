#Region "Microsoft.VisualBasic::06490662352a26205060e6cdaf69f79f, E:/GCModeller/src/R-sharp/R#//Runtime/Environment/Environment.vb"

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

    '   Total Lines: 788
    '    Code Lines: 405
    ' Comment Lines: 286
    '   Blank Lines: 97
    '     File Size: 34.26 KB


    '     Class Environment
    ' 
    '         Properties: funcSymbols, globalEnvironment, isGlobal, isLINQContext, last
    '                     messages, parent, stackFrame, stackTrace
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: asRVector, AssignSymbol, EnumerateAllFunctions, EnumerateAllSymbols, Evaluate
    '                   FindFunction, FindFunctionWithNamespaceRestrict, FindSymbol, GetAcceptorArguments, GetEnumerator
    '                   GetSymbolsNames, IEnumerable_GetEnumerator, Push, ToString, WriteLineHandler
    ' 
    '         Sub: AddMessage, Clear, Delete, (+2 Overloads) Dispose, PushEnvironmentContext
    '              redirectError, redirectWarning, setStackInfo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime

    ''' <summary>
    ''' A ``R#`` scripting runtime context.
    ''' (在一个环境对象容器之中，所有的对象都是以变量来表示的)
    ''' </summary>
    Public Class Environment : Implements IEnumerable(Of Symbol)
        Implements IDisposable

        ''' <summary>
        ''' 最顶层的closure环境的parent是空值来的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property parent As Environment
        ''' <summary>
        ''' The name of this current stack closure.(R function name, closure id, etc)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property stackFrame As StackFrame

        ''' <summary>
        ''' <see cref="debug.getEnvironmentStack"/>
        ''' </summary>
        ''' <returns>
        ''' from current stack to top stack
        ''' </returns>
        Public ReadOnly Property stackTrace As StackFrame()
            Get
                Return debug.getEnvironmentStack(Me)
            End Get
        End Property

        Protected ReadOnly symbols As Dictionary(Of Symbol)

        ''' <summary>
        ''' 导入的函数列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property funcSymbols As Dictionary(Of Symbol)
            Get
                Return hiddenFunctions
            End Get
        End Property

        ''' <summary>
        ''' 主要是存储警告消息
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property messages As New List(Of Message)

        ''' <summary>
        ''' the global environment
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property globalEnvironment As GlobalEnvironment
            Get
                Return [global]
            End Get
        End Property

        ''' <summary>
        ''' get value of the special last variable in R# <see cref="Environment"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property last As Object
            Get
                Return FindSymbol(RInterpreter.lastVariableName)?.value
            End Get
        End Property

        Friend ReadOnly ifPromise As New List(Of IfPromise)
        Friend ReadOnly acceptorArguments As New Dictionary(Of String, Object)
        Friend ReadOnly profiler As List(Of ProfileRecord)

        ''' <summary>
        ''' In the constructor function of <see cref="Runtime.GlobalEnvironment"/>, 
        ''' require attach itself to this parent field
        ''' So this parent field should not be readonly
        ''' </summary>
        Protected [global] As GlobalEnvironment
        Protected hiddenFunctions As Dictionary(Of Symbol)

        ''' <summary>
        ''' 当前的环境是否为最顶层的全局环境？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isGlobal As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return parent Is Nothing AndAlso TypeOf Me Is GlobalEnvironment
            End Get
        End Property

        ''' <summary>
        ''' Get/set variable value
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' getter of this property is a wrapper of function <see cref="FindSymbol(String, Boolean)"/>,
        ''' and the setter of this property is a wrapper of visit data table <see cref="symbols"/>.
        ''' </returns>
        ''' <remarks>
        ''' If the current stack does not contains the target variable, then the program will try to find the variable in his parent
        ''' if variable in format like [var], then it means a global or parent environment variable
        ''' </remarks>
        Default Public Overridable Property value(name As String) As Symbol
            Get
                Dim symbol As Symbol = FindSymbol(name)

                If symbol Is Nothing Then
                    Return New Symbol(Message.SymbolNotFound(Me, name, TypeCodes.generic))
                Else
                    Return symbol
                End If
            End Get
            Set(value As Symbol)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    globalEnvironment(name.GetStackValue("[", "]")) = value
                Else
                    symbols(name) = value
                End If
            End Set
        End Property

        Const AlreadyExists$ = "Variable ""{0}"" is already existed, can not declare it again!"
        Const ConstraintInvalid$ = "Value can not match the type constraint!!! ({0} <--> {1})"

        Dim linqContext As Boolean = False

        Public Property isLINQContext As Boolean
            Get
                If linqContext Then
                    Return True
                ElseIf Not parent Is Nothing Then
                    Return parent.isLINQContext
                Else
                    Return linqContext
                End If
            End Get
            Set(value As Boolean)
                linqContext = value
            End Set
        End Property

        ''' <summary>
        ''' run environment initialize:
        ''' 
        ''' 1. <see cref="symbols"/>
        ''' 2. <see cref="hiddenFunctions"/>
        ''' </summary>
        Sub New()
            symbols = New Dictionary(Of Symbol)
            hiddenFunctions = New Dictionary(Of Symbol)
            parent = Nothing
            [global] = Nothing
            stackFrame = globalStackFrame
            profiler = Nothing

            Log4VB.redirectError = AddressOf redirectError
            Log4VB.redirectWarning = AddressOf redirectWarning
        End Sub

        ''' <summary>
        ''' context constructor for parallel task
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="stackName"></param>
        ''' <param name="symbols"></param>
        ''' <param name="funcs"></param>
        Friend Sub New(parent As Environment, stackName As String,
                       symbols As IEnumerable(Of Symbol),
                       funcs As IEnumerable(Of Symbol)
            )

            Call Me.New(parent, stackName)

            Me.symbols = symbols.ToDictionary
            Me.hiddenFunctions = funcs.ToDictionary
        End Sub

        ''' <param name="isInherits">
        ''' If is inherits mode, then all of the modification in sub-environment will affects the <paramref name="parent"/> environment.
        ''' Otherwise, the modification in sub-environment will do nothing to the <paramref name="parent"/> environment.
        ''' </param>
        Sub New(parent As Environment, stackName As String, Optional isInherits As Boolean = False)
            Call Me.New(parent, StackFrame.FromUnknownLocation(stackName), isInherits)
        End Sub

        ''' <summary>
        ''' Create a new runtime environment context
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="stackFrame"></param>
        ''' <param name="isInherits">
        ''' If is inherits mode, then all of the modification in sub-environment will affects the <paramref name="parent"/> environment.
        ''' Otherwise, the modification in sub-environment will do nothing to the <paramref name="parent"/> environment.
        ''' </param>
        Sub New(parent As Environment,
                stackFrame As StackFrame,
                isInherits As Boolean,
                Optional openProfiler As Boolean = False)

            Call Me.New()

            Me.parent = parent
            Me.stackFrame = stackFrame
            Me.global = parent.globalEnvironment
            Me.profiler = parent.profiler

            If openProfiler Then
                Me.profiler = New List(Of ProfileRecord)
            End If

            If isInherits Then
                symbols = parent.symbols
                hiddenFunctions = parent.funcSymbols
            End If

            If parent.global.log4vb_redirect Then
                Log4VB.redirectError = AddressOf redirectError
                Log4VB.redirectWarning = AddressOf redirectWarning
            End If
        End Sub

        Sub New(globalEnv As GlobalEnvironment)
            Call Me.New

            Me.parent = globalEnv
            Me.global = globalEnv
            Me.stackFrame = globalStackFrame
            Me.profiler = globalEnv.profiler

            If globalEnv.log4vb_redirect Then
                Log4VB.redirectError = AddressOf redirectError
                Log4VB.redirectWarning = AddressOf redirectWarning
            End If
        End Sub

        Public Function GetAcceptorArguments() As Dictionary(Of String, Object)
            Dim args = acceptorArguments.ToDictionary
            Dim base = parent?.GetAcceptorArguments

            If Not base.IsNullOrEmpty Then
                For Each arg In base
                    If Not args.ContainsKey(arg.Key) Then
                        Call args.Add(arg.Key, arg.Value)
                    End If
                Next
            End If

            Return args
        End Function

        ''' <summary>
        ''' get all variable symbol names(function symbol is not included)
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Public Function GetSymbolsNames() As IEnumerable(Of String)
            Return symbols.Keys
        End Function

        ''' <summary>
        ''' Try to get value of the specific symbol
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' this function returns nothing by default if the symbol is missing in the environment context
        ''' </returns>
        Public Function GetValue(name As String, Optional [default] As Object = Nothing) As Object
            Dim symbol As Symbol = FindSymbol(name)

            If symbol Is Nothing Then
                Return [default]
            Else
                Return symbol.value
            End If
        End Function

        ''' <summary>
        ''' a shortcut wrapper function pointer for 
        ''' <see cref="base.print(Object, list, Environment)"/> 
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Public Function WriteLineHandler() As Action(Of Object)
            Return Sub(line) Call base.print(line, , Me)
        End Function

        <DebuggerStepThrough>
        Protected Sub redirectError(obj$, msg$, level As MSG_TYPES)
            Call AddMessage({msg, $"{stackFrame.Method.ToString}\[{obj}]"}, level:=MSG_TYPES.ERR)
        End Sub

        Protected Sub redirectWarning(obj$, msg$, level As MSG_TYPES)
            If obj = NameOf(deps.TryHandleNetCore5AssemblyBugs) Then
                Call Internal.debug _
                    .CreateMessageInternal({msg, $"{stackFrame.Method.ToString}\[{obj}]"}, Me, level) _
                    .DoCall(AddressOf globalEnvironment.dotnetCoreWarning.Add)
            Else
                Call AddMessage({msg, $"{stackFrame.Method.ToString}\[{obj}]"})
            End If
        End Sub

        ''' <summary>
        ''' add error/warning message
        ''' </summary>
        ''' <param name="message"></param>
        ''' <param name="level"></param>
        Public Sub AddMessage(message As Object, Optional level As MSG_TYPES = MSG_TYPES.WRN)
            Internal.debug _
                .CreateMessageInternal(message, Me, level) _
                .DoCall(AddressOf messages.Add)

            If globalEnvironment.verboseOption Then
                Call base.print(message,, Me)
            End If
        End Sub

        <DebuggerStepThrough>
        Public Sub setStackInfo(stackframe As StackFrame)
            _stackFrame = stackframe
        End Sub

        Public Sub Clear()
            ' 20210524
            ' 在这里为了实现function() xxx
            ' 闭包
            ' 在这里不将符号列表清空了，否则会出现找不到符号xxx的问题

            'If Not parent Is Nothing Then
            '    ' 20200304 fix bugs for environment inherits mode
            '    If Not symbols Is parent.symbols Then
            '        Call symbols.Clear()
            '    End If
            '    If Not funcSymbols Is parent.funcSymbols Then
            '        Call funcSymbols.Clear()
            '    End If
            'End If

            Call ifPromise.Clear()
            Call messages.Clear()
        End Sub

        ''' <summary>
        ''' 这个函数查找失败的时候只会返回空值
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="inherits">
        ''' andalso look into the parent environment for the target symbol by <paramref name="name"/>?
        ''' </param>
        ''' <returns></returns>
        Public Overridable Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            'If (name.First = "["c AndAlso name.Last = "]"c) Then
            '    Return globalEnvironment.FindSymbol(name.GetStackValue("[", "]"))
            'End If

            If symbols.ContainsKey(name) Then
                Return symbols(name)
            ElseIf funcSymbols.ContainsKey(name) Then
                Return funcSymbols(name)
            ElseIf [inherits] AndAlso Not parent Is Nothing Then
                Return parent.FindSymbol(name)
            ElseIf name.IndexOf("::") > 0 Then
                Return FindFunctionWithNamespaceRestrict(name)
            Else
                Return Nothing
            End If
        End Function

        Public Function FindFunctionWithNamespaceRestrict(name As String) As Symbol
            Dim tokens As String() = Microsoft.VisualBasic.Strings.Split(name, "::")
            Dim pkgName As String = tokens(Scan0)
            Dim symbolName As String = tokens(1)
            Dim attaches As SymbolNamespaceSolver = globalEnvironment.attachedNamespace
            Dim funcSymbol As RFunction = attaches.FindSymbol(pkgName, symbolName)

            If funcSymbol Is Nothing Then
                funcSymbol = attaches.FindPackageSymbol(pkgName, symbolName, Me)
            End If

            If funcSymbol Is Nothing Then
                Return Nothing
            Else
                Return New Symbol(symbolName, funcSymbol)
            End If
        End Function

        ''' <summary>
        ''' 会首先在当前环境中的符号列表查找函数，如果不是函数则会在当前环境的函数列表中查找
        ''' 如果当前环境中不存在，则会在父环境中查找符号
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns>
        ''' returns nothing if symbol not found
        ''' </returns>
        Public Overridable Function FindFunction(name As String, Optional [inherits] As Boolean = True) As Symbol
            If (name.First = "["c AndAlso name.Last = "]"c) Then
                Return globalEnvironment.FindFunction(name.GetStackValue("[", "]"))
            ElseIf name.IndexOf("::") > 0 Then
                Return FindFunctionWithNamespaceRestrict(name)
            End If

            If symbols.ContainsKey(name) AndAlso symbols(name).isCallable Then
                Return symbols(name)
            End If

            If funcSymbols.ContainsKey(name) Then
                Return funcSymbols(name)
            End If

            If [inherits] AndAlso Not parent Is Nothing Then
                Return parent.FindFunction(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' just removes the symbol reference in the runtime environment
        ''' </summary>
        ''' <param name="name">
        ''' target symbol name that will be removes 
        ''' from the current environment context.
        ''' </param>
        ''' <param name="seekParent">
        ''' andalso removes the symbol with the same name in any parent environment context?
        ''' (是否将任意环境路径中的目标符号引用进行删除？)
        ''' </param>
        Public Sub Delete(name As String, Optional seekParent As Boolean = False)
            If FindSymbol(name, [inherits]:=seekParent) Is Nothing Then
                Return
            End If

            If symbols.ContainsKey(name) Then
                Call symbols.Remove(name)
            ElseIf seekParent AndAlso Not parent Is Nothing Then
                Call parent.Delete(name)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(exec As IEnumerable(Of Expression)) As Object()
            Return exec _
                .Select(Function(a) a.Evaluate(Me)) _
                .ToArray
        End Function

        ''' <summary>
        ''' do value assign
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="strict">
        ''' create a new symbol automatically if target symbol <paramref name="name"/> 
        ''' is not exists in current environment context? otherwise this function will 
        ''' populate an error message if this parameter value is config as false by 
        ''' default.
        ''' </param>
        ''' <returns></returns>
        Public Function AssignSymbol(name As String, value As Object, Optional [strict] As Boolean = False) As Object
            If symbols.ContainsKey(name) Then
                Return symbols(name).SetValue(value, Me)
            ElseIf strict Then
                Return Message.SymbolNotFound(Me, name, TypeCodes.generic)
            Else
                Return Push(name, value, [readonly]:=False, TypeCodes.generic)
            End If
        End Function

        ''' <summary>
        ''' Variable declare, just add a new symbol in current environment context.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="mode"></param>
        ''' <returns>返回错误消息或者对象值</returns>
        Public Function Push(name$, value As Object, [readonly] As Boolean, Optional mode As TypeCodes = TypeCodes.generic) As Object
            If symbols.ContainsKey(name) Then
                ' 变量可以被重复申明
                ' 即允许
                ' let x = 1
                ' let x = 2
                ' 这样子的操作
                ' Return Internal.debug.stop({String.Format(AlreadyExists, name)}, Me)

                Call New String() {
                    $"symbol '{name}' is already exists in current environment",
                    $"symbol: {name}"
                }.DoCall(Sub(info)
                             Call AddMessage(info, MSG_TYPES.WRN)
                         End Sub)

                ' 只需要设置值就可以了
                Return symbols(name).SetValue(value, Me)

            ElseIf (Not value Is Nothing) Then
                value = asRVector(mode, value)
            End If

            With New Symbol(mode) With {
                .name = name,
                .stacktrace = Me.stackTrace
            }
                Call .SetValue(value, Me)

                ' 只读开关应该在设置了初始值之后
                ' 再进行设置，否则会无法设置初始值的
                .[readonly] = [readonly]

                If Not .constraintValid Then
                    Return Internal.debug.stop(New Exception(String.Format(ConstraintInvalid, .typeCode, mode)), Me)
                Else
                    Call .DoCall(AddressOf symbols.Add)
                End If

                Return value
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="value">
        ''' 应该是确保这个变量值是非空的
        ''' </param>
        ''' <returns></returns>
        Friend Shared Function asRVector(type As TypeCodes, value As Object) As Object
            If (TypeOf value Is list) OrElse (TypeOf value Is dataframe) Then
                Return value
            End If

            If type = TypeCodes.generic Then
                ' 没有定义as type做类型约束的时候
                ' 会需要通过值来推断
                type = RType.TypeOf(value).mode
            End If

            If vector.isVectorOf(value, type) Then
                Return value
            End If

            Select Case type
                Case TypeCodes.boolean
                    value = vector.asVector(Of Boolean)(CLRVector.asLogical(value))
                Case TypeCodes.double
                    value = vector.asVector(Of Double)(CLRVector.asNumeric(value))
                Case TypeCodes.integer
                    value = vector.asVector(Of Long)(CLRVector.asLong(value))
                Case TypeCodes.string
                    value = vector.asVector(Of String)(CLRVector.asCharacter(value))
            End Select

            Return value
        End Function

        Public Overrides Function ToString() As String
            If isGlobal Then
                Return $"Global({NameOf(Environment)})"
            Else
                Return GetEnvironmentStackTraceString
            End If
        End Function

        ''' <summary>
        ''' get all function symbols based on the environment context lineage to the root environment
        ''' </summary>
        ''' <returns></returns>
        Public Function EnumerateAllFunctions() As IEnumerable(Of Symbol)
            Dim list = funcSymbols.SafeQuery _
                .Select(Function(fun) fun.Value) _
                .ToList

            If Not parent Is Nothing Then
                list.AddRange(parent.EnumerateAllFunctions)
            End If

            Return list.Distinct
        End Function

        ''' <summary>
        ''' get all symbols based on the environment context lineage to the root environment
        ''' </summary>
        ''' <returns>
        ''' not includes the function symbols
        ''' </returns>
        Public Function EnumerateAllSymbols() As IEnumerable(Of Symbol)
            Dim list = symbols.SafeQuery _
                .Select(Function(fun) fun.Value) _
                .ToList

            If Not parent Is Nothing Then
                list.AddRange(parent.EnumerateAllSymbols)
            End If

            Return list.Distinct
        End Function

        ''' <summary>
        ''' get all symbols in current environment context 
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Symbol) Implements IEnumerable(Of Symbol).GetEnumerator
            For Each var As Symbol In symbols.Values
                Yield var
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' add all of the data symbols from <paramref name="parent"/> to <paramref name="join"/>.
        ''' </summary>
        ''' <param name="join"></param>
        ''' <param name="parent"></param>
        Private Shared Sub PushEnvironmentContext(join As Environment, parent As Environment)
            Dim symbols As New List(Of KeyValuePair(Of String, Symbol))
            Dim funcs As New List(Of KeyValuePair(Of String, Symbol))

            ' 20221115 try to make bugs fixed for the parallel object lock
            ' Linq ToArray has bug when deal with the parallel on unix
            ' .NET6
            ' 
            ' Error in <globalEnvironment> -> <globalEnvironment> -> InitializeEnvironment -> "doMSMSalignment" -> "doMSMSalignment" -> doMSMSalignment -> ".protocol_workflow" -> ".protocol_workflow" -> .protocol_workflow -> "protocol" -> "protocol" -> workflow.general_annotation -> "treeSearch" -> "treeSearch" -> treeSearch -> else_false -> parLapply -> parallel_task[deepCloneContext ~ parallel_task_group_11] -> parallel_task[deepCloneContext ~ parallel_task_group_11] -> run_search -> "cluster_hitscores" -> "cluster_hitscores" -> cluster_hitscores -> "extract_clusterscore" -> "extract_clusterscore" -> extract_clusterscore -> for_loop_[1] -> "cluster_score" -> "cluster_score" -> cluster_score -> lapply
            ' 1. ArgumentException: Destination array is not long enough to copy all the items in the collection. Check array index and length.
            ' 2. stackFrames: 
            ' at System.Collections.Generic.SortedSet`1.CopyTo(T[] array, Int32 index, Int32 count)
            ' at System.Collections.Generic.EnumerableHelpers.ToArray[T](IEnumerable`1 source)
            ' at SMRUCC.Rsharp.Runtime.Environment.PushEnvironmentContext(Environment join, Environment parent)
            ' at SMRUCC.Rsharp.Runtime.Environment.op_Concatenate(Environment closure, Environment parent)
            ' at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure.DeclareNewFunction.InitializeEnvironment(Environment parent, InvokeParameter[] params, Boolean& runDispose)
            ' at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure.DeclareNewFunction.Invoke(Environment parent, InvokeParameter[] params)
            ' at SMRUCC.Rsharp.Runtime.Internal.Invokes.applys.lapply(Object X, Object FUN, Object names, Environment envir)

            ' "candidates" <- Call "lapply"(Call "groupBy"(Call "as.list"(&candidates, "byrow" <- True), [r] -> &r["id"]), declare function '$<$anonymous_800704>'(block) {
            ' # function_internal
            ' <in_memory> <$anonymous_800704>: 
            ' Call "list"("forward" <- Call "sapply"(&block, [i] -> &i["forward"]), "reverse" <- Call "sapply"(&block, [i] -> &i["reverse"]), "jaccard" <- Call "sapply"(&block, [i] -> &i["jaccard"]), "rt" <- Call "sapply"(&block, [i] -> &i["rt"]));
            ' })
            ' ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            ' applys.R#_interop::.lapply at REnv.dll:line <unknown>
            ' biodeepMSMS.declare_function.cluster_score at cluster_score.R:line 16
            ' biodeepMSMS.call_function."cluster_score" at extract_clusterscore.R:line 14
            ' biodeepMSMS.call_function."cluster_score" at extract_clusterscore.R:line 14
            ' biodeepMSMS.forloop.for_loop_[1] at extract_clusterscore.R:line 6
            ' biodeepMSMS.declare_function.extract_clusterscore at extract_clusterscore.R:line 2
            ' biodeepMSMS.call_function."extract_clusterscore" at cluster_hitscores.R:line 24
            ' biodeepMSMS.call_function."extract_clusterscore" at cluster_hitscores.R:line 24
            ' biodeepMSMS.declare_function.cluster_hitscores at cluster_hitscores.R:line 3
            ' biodeepMSMS.call_function."cluster_hitscores" at treeSearch.R:line 25
            ' biodeepMSMS.call_function."cluster_hitscores" at treeSearch.R:line 25
            ' biodeepMSMS.declare_function.run_search at treeSearch.R:line 15
            ' unknown.unknown.parallel_task[deepCloneContext ~ parallel_task_group_11] at n/a:line n/a
            ' unknown.unknown.parallel_task[deepCloneContext ~ parallel_task_group_11] at n/a:line n/a
            ' applys.R#_interop::.parLapply at REnv.dll:line <unknown>
            ' biodeepMSMS.n/a.else_false at treeSearch.R:line 33
            ' biodeepMSMS.declare_function.treeSearch at treeSearch.R:line 3
            ' biodeepMSMS.call_function."treeSearch" at general.R:line 8
            ' biodeepMSMS.call_function."treeSearch" at general.R:line 8
            ' biodeepMSMS.declare_function.workflow.general_annotation at general.R:line 3
            ' biodeepMSMS.call_function."protocol" at run_workflow.R:line 23
            ' biodeepMSMS.call_function."protocol" at run_workflow.R:line 23
            ' biodeepMSMS.declare_function..protocol_workflow at run_workflow.R:line 20
            ' biodeepMSMS.call_function.".protocol_workflow" at main.R:line 93
            ' biodeepMSMS.call_function.".protocol_workflow" at main.R:line 93
            ' biodeepMSMS.declare_function.doMSMSalignment at main.R:line 29
            ' SMRUCC/R#.call_function."doMSMSalignment" at config_run.R:line 6
            ' SMRUCC/R#.call_function."doMSMSalignment" at config_run.R:line 6
            ' SMRUCC/R#.n/a.InitializeEnvironment at config_run.R:line 0
            ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a
            ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a
            '
            SyncLock parent
                SyncLock parent.funcSymbols
                    For Each symbol In parent.funcSymbols
                        Call funcs.Add(symbol)
                    Next
                End SyncLock
                SyncLock parent.symbols
                    For Each symbol In parent.symbols
                        Call symbols.Add(symbol)
                    Next
                End SyncLock
            End SyncLock

            For Each func As KeyValuePair(Of String, Symbol) In funcs
                If Not join.funcSymbols.ContainsKey(func.Key) Then
                    join.funcSymbols.Add(func.Key, func.Value)
                End If
            Next
            For Each symbol As KeyValuePair(Of String, Symbol) In symbols
                If Not join.symbols.ContainsKey(symbol.Key) Then
                    join.symbols.Add(symbol.Key, symbol.Value)
                End If
            Next
        End Sub

        ''' <summary>
        ''' join two runtime environment, this function works for
        ''' the function closure environment setup initialization.
        ''' </summary>
        ''' <param name="closure"></param>
        ''' <param name="parent"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this operator procedure calls may lock all of the parent 
        ''' environment context chain in parallel context
        ''' </remarks>
        Public Shared Operator &(closure As Environment, parent As Environment) As Environment
            ' 20221115 is_inherits = false, means create new empty
            ' symbol table and the function table
            Dim join As New Environment(closure, closure.stackFrame, isInherits:=False)

            Call PushEnvironmentContext(join, parent:=closure)

            Do
                ' ignored of the initialize stack
                ' check env after stackframe pop up
                If TypeOf parent.parent Is GlobalEnvironment Then
                    Exit Do
                Else
                    ' do nothing
                End If

                ' 20220103 try to fix of missing symbols
                PushEnvironmentContext(join, parent)
                parent = parent.parent
                ' pop to global environment
                ' parent of global env is nothing
                '
            Loop Until parent Is Nothing

            Return join
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If Not parent Is Nothing Then
                        ' 将当前环境中所产生的诸如警告消息之类的信息
                        ' 传递到顶层的全局环境之中
                        Call parent.messages.AddRange(messages)

                        Log4VB.redirectError = AddressOf parent.redirectError
                        Log4VB.redirectWarning = AddressOf parent.redirectWarning
                    End If

                    Call Clear()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
