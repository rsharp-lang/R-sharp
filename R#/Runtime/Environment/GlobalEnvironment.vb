#Region "Microsoft.VisualBasic::4b7204e9b179bfb000c4f2838aabc992, D:/GCModeller/src/R-sharp/R#//Runtime/Environment/GlobalEnvironment.vb"

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

    '   Total Lines: 289
    '    Code Lines: 187
    ' Comment Lines: 60
    '   Blank Lines: 42
    '     File Size: 11.88 KB


    '     Class GlobalEnvironment
    ' 
    '         Properties: attachedNamespace, debugLevel, debugMode, factors, hybridsEngine
    '                     lastException, log4vb_redirect, options, packages, profiler2
    '                     Rscript, scriptDir, stdout, symbolLanguages, types
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [GetType], defaultEmpty, doCall, (+2 Overloads) LoadLibrary, MissingPackage
    '                   SetDebug
    ' 
    '         Sub: Dispose, RedirectOutput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Hybrids
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports anything = Microsoft.VisualBasic.Scripting
Imports RPkg = SMRUCC.Rsharp.Development.Package.Package

<Assembly: InternalsVisibleTo("Rnotebook")>

Namespace Runtime

    ''' <summary>
    ''' R#之中的全局环境对象
    ''' </summary>
    Public Class GlobalEnvironment : Inherits Environment

        Public ReadOnly Property options As Options
        Public ReadOnly Property packages As PackageManager
        Public ReadOnly Property attachedNamespace As SymbolNamespaceSolver
        Public ReadOnly Property hybridsEngine As New HybridsEngine
        Public ReadOnly Property profiler2 As New Stack(Of ProfilerFrames)

        ''' <summary>
        ''' the R# script host object
        ''' </summary>
        ''' <returns></returns>
        Public Property Rscript As RInterpreter
            Get
                Return m_REngine
            End Get
            Friend Set(value As RInterpreter)
                m_REngine = value
            End Set
        End Property

        Dim m_REngine As RInterpreter

        ''' <summary>
        ''' a <see cref="TextWriter"/> wrapper object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property stdout As RContentOutput
        Public ReadOnly Property log4vb_redirect As Boolean = True
        Public ReadOnly Property debugLevel As DebugLevels = DebugLevels.All

        ''' <summary>
        ''' <see cref="RInterpreter.debug"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property debugMode As Boolean
            Get
                Return Rscript.debug
            End Get
        End Property

        ''' <summary>
        ''' 用于traceback进行脚本函数调试使用
        ''' </summary>
        ''' <returns></returns>
        Public Property lastException As Message

        Public ReadOnly Property types As New Dictionary(Of String, RType)
        Public ReadOnly Property factors As New Dictionary(Of String, factor)
        Public ReadOnly Property symbolLanguages As SymbolLanguageProcessor

        ''' <summary>
        ''' if current executation is comes from the R script executation
        ''' then this property will returns the directory path in the ``!script``
        ''' magic symbol object, otherwise will returns nothing
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property scriptDir As String
            Get
                Dim script As Symbol = FindSymbol("!script")

                If script Is Nothing Then
                    Return Nothing
                ElseIf Not (TypeOf script.value Is vbObject AndAlso TypeOf DirectCast(script.value, vbObject).target Is MagicScriptSymbol) Then
                    Return Nothing
                Else
                    Return DirectCast(DirectCast(script.value, vbObject).target, MagicScriptSymbol).dir
                End If
            End Get
        End Property

        Friend ReadOnly dotnetCoreWarning As New List(Of Message)

        Friend Sub New(scriptHost As RInterpreter, options As Options)
            Me.options = options
            Me.packages = New PackageManager(options)
            Me.global = Me
            Me.Rscript = scriptHost
            Me.stdout = New RContentOutput(App.StdOut.DefaultValue, env:=OutputEnvironments.Console)
            Me.log4vb_redirect = options.log4vb_redirect
            Me.symbolLanguages = New SymbolLanguageProcessor(Me)
            Me.attachedNamespace = New SymbolNamespaceSolver(Me)

            Call types.Add("unit", RType.GetRSharpType(GetType(unit)))
        End Sub

        ''' <summary>
        ''' copy environment
        ''' </summary>
        ''' <param name="globalEnv"></param>
        Sub New(globalEnv As GlobalEnvironment, Optional REngine As RInterpreter = Nothing)
            Me.options = globalEnv.options
            Me.packages = globalEnv.packages
            Me.global = Me
            Me.Rscript = If(REngine, globalEnv.Rscript)
            Me.stdout = New RContentOutput(App.StdOut.DefaultValue, env:=OutputEnvironments.Console)
            Me.log4vb_redirect = globalEnv.log4vb_redirect
            Me.symbolLanguages = New SymbolLanguageProcessor(Me)
            Me.hiddenFunctions = New Dictionary(Of Symbol)(globalEnv.funcSymbols.Values)
            Me.attachedNamespace = globalEnv.attachedNamespace

            Call types.Add("unit", RType.GetRSharpType(GetType(unit)))
        End Sub

        <DebuggerStepThrough>
        Public Shared Function defaultEmpty() As [Default](Of Environment)
            Return DirectCast(New RInterpreter().globalEnvir, Environment)
        End Function

        ''' <summary>
        ''' invoke a R function by name
        ''' </summary>
        ''' <param name="func">the R function name</param>
        ''' <param name="args">the named parameter list</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function doCall(func As String, ParamArray args As NamedValue(Of Object)()) As Object
            Return Rscript.Invoke(func, args)
        End Function

        Public Function SetDebug(level As DebugLevels) As GlobalEnvironment
            _debugLevel = level
            Return Me
        End Function

        ''' <summary>
        ''' get type definition from a given type object or type name
        ''' </summary>
        ''' <param name="typeof">
        ''' the object type value, and it should be one of the:
        ''' 
        ''' 1. class name from the <see cref="RTypeExportAttribute"/>
        ''' 2. .NET CLR <see cref="Type"/> value
        ''' 3. R-sharp <see cref="RType"/> value
        ''' 4. R-sharp primitive <see cref="TypeCodes"/> value
        ''' 5. <see cref="TypeInfo"/> metadata
        ''' </param>
        ''' <returns></returns>
        Public Overloads Function [GetType]([typeof] As Object) As RType
            If TypeOf [typeof] Is Type Then
                Return RType.GetRSharpType(DirectCast([typeof], Type))
            ElseIf TypeOf [typeof] Is RType Then
                Return DirectCast([typeof], RType)
            ElseIf TypeOf [typeof] Is TypeCodes Then
                Return RType.GetType(DirectCast([typeof], TypeCodes))
            ElseIf TypeOf [typeof] Is TypeInfo Then
                Return RType.GetType(DirectCast([typeof], TypeInfo))
            End If

            Dim className As String = anything.ToString([typeof], "any")
            Dim type As RType = _types.TryGetValue(className)

            If type Is Nothing Then
                Return className.GetRTypeCode.DoCall(AddressOf RType.GetType)
            Else
                Return type
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub RedirectOutput(out As StreamWriter, env As OutputEnvironments)
            _stdout = New RContentOutput(out, env:=env)
        End Sub

        ''' <summary>
        ''' load library module
        ''' </summary>
        ''' <param name="packageName">the library package name</param>
        ''' <param name="silent">suppress of print message?</param>
        ''' <param name="ignoreMissingStartupPackages">debug used only</param>
        ''' <returns></returns>
        Public Function LoadLibrary(packageName As String,
                                    Optional silent As Boolean = False,
                                    Optional ignoreMissingStartupPackages As Boolean = False) As Message

            Dim exception As Exception = Nothing
            Dim package As RPkg = Nothing
            Dim RzipPackageFolder As String = Nothing

            If Not packages.hasLibPackage(packageName) Then
                package = packages.FindPackage(packageName, exception)
            End If

            If package Is Nothing Then
                RzipPackageFolder = PackageLoader2.GetPackageDirectory(options, packageName)
            End If

            If (Not packageName Like packages.loadedPackages) AndAlso (Not [global].attachedNamespace.hasNamespace(packageName)) Then
                If Not silent Then
                    Call _stdout.WriteLine($"Loading required package: {packageName}")
                End If

                If Not package Is Nothing Then
                    Call packages.addAttached(package)
                End If
            Else
                ' 跳过已经加在的程序包，不再进行重复加载了
                Return Nothing
            End If

            If (Not RzipPackageFolder.StringEmpty) AndAlso $"{RzipPackageFolder}/package/index.json".FileExists Then
                Return PackageLoader2.LoadPackage(RzipPackageFolder, [global])
            ElseIf package Is Nothing Then
                If Not ignoreMissingStartupPackages Then
                    Return MissingPackage(packageName, exception)
                Else
                    Return Nothing
                End If
            Else
                Return LoadLibrary(package.package, packageName, silent)
            End If
        End Function

        ''' <summary>
        ''' Imports static api function from given package module
        ''' </summary>
        ''' <param name="package"></param>
        ''' <returns></returns>
        Public Function LoadLibrary(package As Type, Optional packageName$ = Nothing, Optional silent As Boolean = False) As Message
            Dim masked As String() = ImportsPackage _
                .ImportsStatic(Me, package) _
                .ToArray

            If Not silent AndAlso masked.Length > 0 Then
                If packageName.StringEmpty Then
                    packageName = package.NamespaceEntry.Namespace
                End If

                Call _stdout.WriteLine($"Attaching package: '{packageName}'")
                Call _stdout.WriteLine()
                Call _stdout.WriteLine($"The following object is masked from 'package:{packageName}':")
                Call _stdout.WriteLine()

                Dim maxColumns As Integer = Me.getMaxColumns
                Dim dev As RContentOutput = stdout

                Call printer.printContentArray(masked, ", ", "    ", maxColumns, dev)
                Call _stdout.WriteLine()
            End If

            Return Nothing
        End Function

        Private Function MissingPackage(packageName$, exception As Exception) As Message
            Dim message As Message

            exception = If(exception, New Exception($"No packages named '{packageName}' is installed..."))
            message = Internal.debug.stop(exception, Me)
            message.message = {
                $"there is no package called ‘{packageName}’",
                $"package: {packageName}"
            }.Join(message.message)

            Return message
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not stdout Is Nothing Then
                Call stdout.Flush()
            End If

            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
