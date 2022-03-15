#Region "Microsoft.VisualBasic::6498805f851abb9196596d8e452ca93c, R-sharp\R#\Interpreter\RInterpreter.vb"

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


     Code Statistics:

        Total Lines:   608
        Code Lines:    379
        Comment Lines: 134
        Blank Lines:   95
        File Size:     24.86 KB


    '     Class RInterpreter
    ' 
    '         Properties: configFile, debug, globalEnvir, redirectError2stdout, Rsharp
    '                     silent, strict, warnings
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [Imports], [Set], (+2 Overloads) Evaluate, FromEnvironmentConfiguration, getDataStream
    '                   InitializeEnvironment, (+3 Overloads) Invoke, (+2 Overloads) LoadLibrary, options, Parse
    '                   RedirectOutput, Run, RunInternal, Source
    ' 
    '         Sub: _construct, (+3 Overloads) Add, (+2 Overloads) Dispose, Inspect, (+2 Overloads) Print
    '              PrintMemory
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports stdNum = System.Math
Imports Strings = Microsoft.VisualBasic.Strings

Namespace Interpreter

    Public Class RInterpreter : Implements IDisposable

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As GlobalEnvironment
        Public ReadOnly Property warnings As New List(Of Message)

        ''' <summary>
        ''' R# running in debug mode.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 调试模式下，除了输出表达式的字符串信息之外
        ''' 还会改变stop的行为，在非调试模式下，stop函数只会丢出错误消息并且终止脚本的运行
        ''' 但是在调试模式下面，stop函数则会令程序抛出异常方便开发人员进行错误的定位
        ''' </remarks>
        Public Property debug As Boolean = False
        Public Property silent As Boolean = False
        ''' <summary>
        ''' 是否重定向错误消息输出至<see cref="RedirectOutput"/>函数所定义的输出设备之中
        ''' </summary>
        ''' <returns></returns>
        Public Property redirectError2stdout As Boolean = False

        ''' <summary>
        ''' 是否在严格模式下运行R#脚本？默认为严格模式，即：
        ''' 
        ''' 1. 所有的变量必须使用``let``关键词进行申明
        ''' </summary>
        ''' <returns></returns>
        Public Property strict As Boolean = True

        ''' <summary>
        ''' Get value of a <see cref="Symbol"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetValue(name As String) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return globalEnvir(name).value
            End Get
        End Property

        Public Const lastVariableName$ = "$"

        Public ReadOnly Property configFile As ConfigFile
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return globalEnvir.options.file
            End Get
        End Property

        ''' <summary>
        ''' 直接无参数调用这个构造函数，则会使用默认的配置文件创建R#脚本解释器引擎实例
        ''' </summary>
        ''' <param name="envirConf"></param>
        Sub New(Optional envirConf As Options = Nothing)
            If envirConf Is Nothing Then
                envirConf = New Options(ConfigFile.localConfigs, saveConfig:=False)
            End If

            Call _construct(New GlobalEnvironment(Me, envirConf))
        End Sub

        Private Sub _construct(env As GlobalEnvironment)
            _globalEnvir = env
            _globalEnvir.Push(lastVariableName, Nothing, False, TypeCodes.generic)
            _globalEnvir.Push("PI", stdNum.PI, True, TypeCodes.double)
            _globalEnvir.Push("E", stdNum.E, True, TypeCodes.double)
            _globalEnvir.Push(".GlobalEnv", globalEnvir, True, TypeCodes.generic)

            For Each name As String In env.options.environments.SafeQuery
                Dim dllfile As String = $"{App.HOME}/{name}"

                If dllfile.FileExists Then
                    Try
                        Call env.hybridsEngine.Register(dllpath:=dllfile)
                    Catch ex As Exception
                        Call env.AddMessage({$"can not load assembly: {dllfile}!", ex.ToString}, MSG_TYPES.WRN)
                    End Try
                Else
                    Call env.AddMessage($"ignore missing script engine module: {dllfile}...", MSG_TYPES.WRN)
                End If
            Next

            ' config R# interpreter engine
            strict = env.options.strict
        End Sub

        Sub New(env As GlobalEnvironment)
            Call _construct(env)
        End Sub

        Public Function RedirectOutput(out As StreamWriter, env As OutputEnvironments) As RInterpreter
            Call globalEnvir.RedirectOutput(out, env)
            Return Me
        End Function

        ''' <summary>
        ''' open the data file as stream from a given 
        ''' package with a specific resource reference 
        ''' name.
        ''' </summary>
        ''' <param name="dataName"></param>
        ''' <param name="package"></param>
        ''' <returns></returns>
        Public Function getDataStream(dataName As String, package As String) As Stream
            Dim pkgDir As String
            Dim alternativeName As String = dataName.createAlternativeName

            ' 优先从已经加载的程序包位置进行加载操作
            If globalEnvir.attachedNamespace.hasNamespace(package) Then
                pkgDir = globalEnvir.attachedNamespace(package).libpath
            ElseIf Not RFileSystem.PackageInstalled(package, globalEnvir) Then
                Return Nothing
            Else
                pkgDir = $"{RFileSystem.GetPackageDir(globalEnvir)}/{package}"
            End If

            dataName = $"{pkgDir}/{dataName}".GetFullPath

            If dataName.FileExists Then
                Return dataName.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            ElseIf $"{pkgDir}/{alternativeName}".FileExists Then
                Return $"{pkgDir}/{alternativeName}".Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            End If

            ' for missing data package just
            ' returns nothing
            Return Nothing
        End Function

        Public Function options(Optional names As String() = Nothing,
                                Optional verbose As Boolean? = Nothing,
                                Optional strict As Boolean? = Nothing) As Object

            Dim setOption As Boolean = False

            If Not verbose Is Nothing Then
                globalEnvir.options.setOption(NameOf(verbose), verbose.Value)
                setOption = True
            ElseIf Not strict Is Nothing Then
                Me.globalEnvir.options.setOption(NameOf(strict), strict.Value)
                Me.strict = strict

                setOption = True
            End If

            If setOption AndAlso Not names.IsNullOrEmpty Then
                Return Internal.debug.stop({"can not set options with get options!"}, globalEnvir)
            ElseIf setOption Then
                Return globalEnvir
            Else
                Return New list With {
                    .slots = names _
                        .ToDictionary(Function(name) name,
                                      Function(opt)
                                          Return CObj(globalEnvir.options.getOption(opt))
                                      End Function)
                }
            End If
        End Function

        Public Sub PrintMemory(Optional dev As TextWriter = Nothing)
            Dim table$()() = globalEnvir _
                .Select(Function(v)
                            Dim value$ = Symbol.GetValueViewString(v)

                            Return {
                                v.name,
                                v.typeCode.ToString,
                                v.typeof.FullName,
                                $"[{v.length}] {value}"
                            }
                        End Function) _
                .ToArray

            With dev Or App.StdOut
                Call .DoCall(Sub(device)
                                 Call table.PrintTable(
                                    dev:=device,
                                    leftMargin:=3,
                                    title:={"name", "mode", "typeof", "value"}
                                 )
                             End Sub)
            End With
        End Sub

        ''' <summary>
        ''' A shortcut of ``print(expr)``
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <param name="auto">
        ''' 在自动条件下，会忽略掉<paramref name="expr"/>为<see cref="invisible"/>
        ''' 的结果打印
        ''' </param>
        Public Sub Print(expr As String, Optional auto As Boolean = True)
            Dim result As Object = Evaluate(expr)

            If auto AndAlso Not result Is Nothing AndAlso TypeOf result Is invisible Then
                Return
            Else
                ' do expression evaluation and then 
                ' print($expr)
                Call REnv.print(result, , globalEnvir)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Print(obj As Object)
            Call REnv.print(obj,, globalEnvir)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Inspect(obj As Object)
            Call REnv.str(obj,, globalEnvir)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Imports](pkgs As String(), baseDll As String) As Object
            Return New [Imports](VectorLiteral.FromArray(pkgs), New Literal(baseDll)).Evaluate(globalEnvir)
        End Function

        ''' <summary>
        ''' Load packages from package name or dll module file
        ''' </summary>
        ''' <param name="packageName">
        ''' package namespace or dll module file path
        ''' </param>
        Public Function LoadLibrary(packageName$, Optional silent As Boolean = False, Optional ignoreMissingStartupPackages As Boolean = False) As RInterpreter
            If packageName.FileExists Then
                ' is a dll file
                Call ExpressionSymbols.[Imports].LoadLibrary(packageName, globalEnvir, {"*"})
            Else
                Dim result As Message = globalEnvir.LoadLibrary(packageName, silent, ignoreMissingStartupPackages:=ignoreMissingStartupPackages)

                If Not result Is Nothing Then
                    Call Internal.debug.PrintMessageInternal(result, globalEnvir)
                End If
            End If

            Return Me
        End Function

        ''' <summary>
        ''' Imports static api function from given package module
        ''' </summary>
        ''' <param name="package"></param>
        ''' <returns></returns>
        Public Function LoadLibrary(package As Type) As RInterpreter
            Call globalEnvir.LoadLibrary(package)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub Add(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic)
            Call globalEnvir.Push(name, value, [readonly]:=False, mode:=type)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub Add(name$, closure As [Delegate])
            globalEnvir.Push(name, New RMethodInfo(name, closure), [readonly]:=False, mode:=TypeCodes.closure)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub Add(name$, closure As MethodInfo, Optional target As Object = Nothing)
            globalEnvir.Push(name, New RMethodInfo(name, closure, target), [readonly]:=False, mode:=TypeCodes.closure)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Set](name As String, value As Object) As Object
            Return globalEnvir.Push(name, value, [readonly]:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Invoke(Of T)(funcName$, ParamArray args As Object()) As T
            Return DirectCast(Invoke(funcName, args), T)
        End Function

        ''' <summary>
        ''' direct invoke
        ''' </summary>
        ''' <param name="funcName$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Invoke(funcName$, ParamArray args As Object()) As Object
            Dim find As Object

            If Strings.InStr(funcName, "::") > 0 Then
                Dim nsRef As NamedValue(Of String) = funcName.GetTagValue("::")

                find = FunctionInvoke.GetFunctionVar(
                    funcName:=New Literal(nsRef.Value),
                    env:=globalEnvir,
                    [namespace]:=nsRef.Name
                )
            Else
                find = FunctionInvoke.GetFunctionVar(New Literal(funcName), globalEnvir)
            End If

            If TypeOf find Is Message Then
                Return find
            Else
                Return DirectCast(find, RFunction).Invoke(args, globalEnvir)
            End If
        End Function

        ''' <summary>
        ''' invoke a R function by name
        ''' </summary>
        ''' <param name="funcName">the R function name</param>
        ''' <param name="args">the named parameter list</param>
        ''' <returns></returns>
        Public Function Invoke(funcName$, args As NamedValue(Of Object)()) As Object
            Dim find As Object = FunctionInvoke.GetFunctionVar(New Literal(funcName), globalEnvir)
            Dim parameters As InvokeParameter() = args _
                .Select(Function(a, i)
                            Return New InvokeParameter(a.Name, a.Value, i + 1)
                        End Function) _
                .ToArray

            If TypeOf find Is Message Then
                Return find
            Else
                Return DirectCast(find, RFunction).Invoke(globalEnvir, parameters)
            End If
        End Function

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script">The script text</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Evaluate(script As String) As Object
            Return RunInternal(Rscript.FromText(script), {}, Nothing)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Run(program As Program) As Object
            ' Return finalizeResult(program.Execute(globalEnvir))
            Dim last As Symbol = Me.globalEnvir(lastVariableName)
            Dim result As Object = program.Execute(globalEnvir)

            ' set last variable in current environment
            Call last.SetValue(result, globalEnvir)

            Return result
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">The script file name</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Private Function InitializeEnvironment(source$, arguments As NamedValue(Of Object)()) As Environment
            Dim env As Environment

            If source Is Nothing OrElse Strings.InStr(source, "<in_memory_") = 1 Then
                env = globalEnvir
            Else
                env = New StackFrame With {
                    .File = source,
                    .Line = 0,
                    .Method = New Method With {
                        .Method = MethodBase.GetCurrentMethod.Name,
                        .[Module] = "n/a",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }.DoCall(Function(stackframe)
                             Return New Environment(globalEnvir, stackframe, isInherits:=True)
                         End Function)
            End If

            Dim symbol$
            Dim obj As Object

            For Each var As NamedValue(Of Object) In arguments
                symbol = var.Name
                obj = var.Value

                Call env.Push(symbol, obj, [readonly]:=False)
            Next

            If debug AndAlso arguments.Length > 0 Then
                Call "Initialize of the environment with pre-define symbols:".__DEBUG_ECHO
                Call arguments.Keys.GetJson.__INFO_ECHO

                If arguments.Any(Function(a) a.Name = "!script") Then
                    Dim magic As vbObject = arguments _
                        .Where(Function(a) a.Name = "!script") _
                        .First _
                        .Value
                    Dim magicList As list = DirectCast(magic.target, MagicScriptSymbol).toList

                    Call Invokes.base.str(magicList, env:=env)
                End If
            End If

            Return env
        End Function

        'Friend Function finalizeResult(result As Object) As Object
        '    Dim last As Symbol = Me.globalEnvir(lastVariableName)

        '    ' set last variable in current environment
        '    Call last.SetValue(result, globalEnvir)

        '    'If Program.isException(result) Then
        '    '    Call VBDebugger.WaitOutput()
        '    '    Call Internal.debug.PrintMessageInternal(message:=result)
        '    'End If

        '    If globalEnvir.messages > 0 Then
        '        Call VBDebugger.WaitOutput()

        '        For Each message As Message In globalEnvir.messages
        '            Call Internal.debug.PrintMessageInternal(message, globalEnvir)
        '        Next

        '        Call globalEnvir.messages.Clear()
        '    End If

        '    Return result
        'End Function

        Public Function Parse(text As String) As Program
            Dim error$ = Nothing
            Dim Rscript As Rscript = Rscript.AutoHandleScript(text)
            Dim program As Program = Program.CreateProgram(Rscript, debug:=debug, [error]:=[error])

            Return program
        End Function

        Private Function RunInternal(Rscript As Rscript, arguments As NamedValue(Of Object)(), ByRef globalEnvir As Environment) As Object
            Dim error$ = Nothing
            Dim program As Program = Program.CreateProgram(Rscript, debug:=debug, [error]:=[error])
            Dim result As Object

            If debug Then
#Disable Warning
                Call Console.WriteLine(vbNewLine)
                Call Console.WriteLine(program.ToString)
                Call Console.WriteLine(vbNewLine)
#Enable Warning
            End If

            globalEnvir = InitializeEnvironment(Rscript.fileName, arguments)

            If Not [error].StringEmpty Then
                result = Internal.debug.stop([error], globalEnvir)
            Else
                result = program.Execute(globalEnvir)
            End If

            ' fix bugs of warning message populates
            ' to upper global environment
            If Not globalEnvir Is Me.globalEnvir Then
                Call globalEnvir.Dispose()
            End If

            ' set last variable in current environment
            Call Me.globalEnvir(lastVariableName).SetValue(result, globalEnvir)

            Return result
        End Function

        ''' <summary>
        ''' Run R# script program from a given script file.
        ''' (运行脚本的时候调用的是<see cref="globalEnvir"/>全局环境)
        ''' </summary>
        ''' <param name="filepath">The script file path.</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Source(filepath$,
                               Optional arguments As NamedValue(Of Object)() = Nothing,
                               Optional ByRef globalEnv As Environment = Nothing) As Object

            ' when source a given script by path
            ' then an object list variable with special name will be push into 
            ' the environment
            ' 
            ' let !script = list(dir = dirname, file = filename, fullName = filepath)
            Dim script As MagicScriptSymbol = CreateMagicScriptSymbol(filepath, R:=Me)
            Dim result As Object

            If filepath.FileExists Then
                Dim Rscript As Rscript = Rscript.FromFile(filepath)

                arguments = arguments _
                    .SafeQuery _
                    .JoinIterates(New NamedValue(Of Object)("!script", New vbObject(script))) _
                    .ToArray
                result = RunInternal(Rscript, arguments, globalEnv)
            Else
                result = Internal.debug.stop({
                    $"cannot open the connection.",
                    $"cannot open file '{filepath.FileName}': No such file or directory",
                    $"file: {filepath.GetFullPath}",
                    $"function: source"
                }, globalEnvir)
            End If

            Return result
        End Function

        Public Shared ReadOnly Property Rsharp As New RInterpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        Dim name$
                        Dim value As Object

                        For Each var As NamedValue(Of Object) In args
                            name = var.Name
                            value = var.Value

                            Call .globalEnvir.Push(name, value, [readonly]:=False, mode:=TypeCodes.generic)
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function FromEnvironmentConfiguration(configs As String) As RInterpreter
            Return New RInterpreter(New Options(configs, saveConfig:=False))
        End Function

        Private disposedValue As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call warnings.Clear()
                    Call globalEnvir.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Call Dispose(disposing:=True)
            Call System.GC.SuppressFinalize(Me)
        End Sub

        Public Shared Narrowing Operator CType(R As RInterpreter) As Environment
            Return R.globalEnvir
        End Operator
    End Class
End Namespace
