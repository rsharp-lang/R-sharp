#Region "Microsoft.VisualBasic::f4cddd59f29e3bf35cc60613a4e9ee97, R#\Runtime\GlobalEnvironment.vb"

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
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: (+2 Overloads) LoadLibrary, MissingPackage
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
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

        Sub New(scriptHost As RInterpreter, options As Options)
            Me.options = options
            Me.packages = New PackageManager(options)
            Me.global = Me
            Me.Rscript = scriptHost
            Me.stdout = New RContentOutput(App.StdOut.DefaultValue)
        End Sub

        Public Sub RedirectOutput(out As StreamWriter)
            _stdout = New RContentOutput(out)
        End Sub

        Public Function LoadLibrary(packageName As String, Optional silent As Boolean = False) As Message
            Dim exception As Exception = Nothing
            Dim package As RPkg = packages.FindPackage(packageName, exception)

            If Not packageName Like packages.loadedPackages Then
                If Not silent Then
                    Call _stdout.WriteLine($"Loading required package: {packageName}")
                End If

                Call packages.addAttached(package)
            Else
                Return Nothing
            End If

            If package Is Nothing Then
                Return MissingPackage(packageName, exception)
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

                Call printer.printContentArray(masked, ", ", "    ", Me)
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
