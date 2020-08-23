#Region "Microsoft.VisualBasic::e34aa1c344cba627a6e4cc29360f249a, R#\Runtime\Environment\GlobalEnvironment.vb"

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

    '     Class GlobalEnvironment
    ' 
    '         Properties: debugMode, lastException, options, packages, Rscript
    '                     scriptDir, stdout, types
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [GetType], (+2 Overloads) LoadLibrary, MissingPackage
    ' 
    '         Sub: Dispose, RedirectOutput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Configuration
Imports SMRUCC.Rsharp.System.Package
Imports RPkg = SMRUCC.Rsharp.System.Package.Package

Namespace Runtime

    ''' <summary>
    ''' R#之中的全局环境对象
    ''' </summary>
    Public Class GlobalEnvironment : Inherits Environment

        Public ReadOnly Property options As Options
        Public ReadOnly Property packages As PackageManager
        Public ReadOnly Property Rscript As RInterpreter
        Public ReadOnly Property stdout As RContentOutput

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

        Sub New(scriptHost As RInterpreter, options As Options)
            Me.options = options
            Me.packages = New PackageManager(options)
            Me.global = Me
            Me.Rscript = scriptHost
            Me.stdout = New RContentOutput(App.StdOut.DefaultValue, env:=OutputEnvironments.Console)

            Call types.Add("unit", RType.GetRSharpType(GetType(unit)))
        End Sub

        ''' <summary>
        ''' get type definition from a given type object or type name
        ''' </summary>
        ''' <param name="typeof"></param>
        ''' <returns></returns>
        Public Overloads Function [GetType]([typeof] As Object) As RType
            If TypeOf [typeof] Is Type Then
                Return RType.GetRSharpType(DirectCast([typeof], Type))
            ElseIf TypeOf [typeof] Is RType Then
                Return DirectCast([typeof], RType)
            End If

            Dim className As String = Scripting.ToString([typeof], "any")
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

        Public Function LoadLibrary(packageName As String, Optional silent As Boolean = False, Optional ignoreMissingStartupPackages As Boolean = False) As Message
            Dim exception As Exception = Nothing
            Dim package As RPkg = packages.FindPackage(packageName, exception)

            If Not packageName Like packages.loadedPackages Then
                If Not silent Then
                    Call _stdout.WriteLine($"Loading required package: {packageName}")
                End If

                If Not package Is Nothing Then
                    Call packages.addAttached(package)
                End If
            Else
                Return Nothing
            End If

            If package Is Nothing Then
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
