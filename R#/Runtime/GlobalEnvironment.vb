#Region "Microsoft.VisualBasic::e0c1c17c07ef9adb64cff16472b82847, R#\Runtime\GlobalEnvironment.vb"

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
'         Properties: debugMode, options, packages, Rscript
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: LoadLibrary
' 
' 
' /********************************************************************************/

#End Region

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
        Public ReadOnly Property debugMode As Boolean
            Get
                Return Rscript.debug
            End Get
        End Property

        Sub New(scriptHost As RInterpreter, options As Options)
            Me.options = options
            Me.packages = New PackageManager(options)
            Me.global = Me
            Me.Rscript = scriptHost
        End Sub

        Public Function LoadLibrary(packageName As String) As Message
            Dim exception As Exception = Nothing
            Dim package As RPkg = packages.FindPackage(packageName, exception)
            Dim masked As String()

            If Not packageName Like packages.loadedPackages Then
                Call Console.WriteLine($"Loading required package: {packageName}")
            Else
                Return Nothing
            End If

            If package Is Nothing Then
                Return MissingPackage(packageName, exception)
            Else
                masked = ImportsPackage _
                    .ImportsStatic(Me, package.package) _
                    .ToArray

                If masked.Length > 0 Then
                    Call Console.WriteLine($"Attaching package: '{packageName}'")
                    Call Console.WriteLine()
                    Call Console.WriteLine($"The following object is masked from 'package:{packageName}':")
                    Call Console.WriteLine()

                    Call printer.printContentArray(masked, ", ", "    ")
                End If
            End If

            Return Nothing
        End Function

        Private Function MissingPackage(packageName$, exception As Exception) As Message
            Dim message As Message

            exception = If(exception, New Exception("No packages installed..."))
            message = Internal.stop(exception, Me)
            message.message = {
                $"there is no package called ‘{packageName}’",
                $"package: {packageName}"
            }.Join(message.message)

            Return message
        End Function
    End Class
End Namespace
