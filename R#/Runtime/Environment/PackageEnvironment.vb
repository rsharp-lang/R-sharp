#Region "Microsoft.VisualBasic::452c2ee9445c36bd7ce2ff60d8d49b25, R#\Runtime\Environment\PackageEnvironment.vb"

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

    '   Total Lines: 95
    '    Code Lines: 64
    ' Comment Lines: 14
    '   Blank Lines: 17
    '     File Size: 3.61 KB


    '     Class PackageEnvironment
    ' 
    '         Properties: [namespace], libpath
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Attach, pkgFrame, SetPackage, ToString
    ' 
    '         Sub: AddSymbols
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.FileIO
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    ''' <summary>
    ''' The package namespace
    ''' </summary>
    Public Class PackageEnvironment : Inherits Environment

        Public ReadOnly Property [namespace] As New PackageNamespace

        ''' <summary>
        ''' the package directory path that would be used for:
        ''' 
        ''' 1. config for load .NET assembly module files
        ''' 2. get resource files inside the package
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property libpath As IFileSystemEnvironment

        Dim m_symbolSolver As SymbolNamespaceSolver

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(globalEnv As GlobalEnvironment, packageName As String, libpath As IFileSystemEnvironment)
            Call MyBase.New(globalEnv, pkgFrame(packageName), isInherits:=False)

            Me.libpath = libpath
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Private Shared Function pkgFrame(pkgName As String) As StackFrame
            Return New StackFrame With {
                .File = pkgName,
                .Line = "n/a",
                .Method = New Method With {
                   .Method = "loadEnvironment",
                   .[Module] = "package",
                   .[Namespace] = pkgName
                }
            }
        End Function

        ''' <summary>
        ''' handling of the function overloads between difference package namespace.
        ''' </summary>
        ''' <param name="symbols"></param>
        Public Sub AddSymbols(symbols As IEnumerable(Of RFunction))
            For Each callable As RFunction In symbols
                Dim symbol As New Symbol(callable) With {
                    .name = callable.name,
                    .stacktrace = Me.stackTrace
                }
                Dim pkg As String = [namespace].packageName
                Dim callName As String = callable.name

                Call symbol.setMutable([readonly]:=True)

                Me.symbols(callName) = symbol
                Me.globalEnvironment.funcSymbols(callName) = symbol

                If TypeOf callable Is DeclareNewFunction Then
                    DirectCast(callable, DeclareNewFunction).Namespace = pkg
                End If

                If Not m_symbolSolver.funcOverloads.ContainsKey(callName) Then
                    m_symbolSolver.funcOverloads.Add(callName, New Dictionary(Of String, RFunction))
                End If

                m_symbolSolver.funcOverloads(callName)(pkg) = callable
            Next
        End Sub

        Public Function Attach(solver As SymbolNamespaceSolver) As PackageEnvironment
            Me.m_symbolSolver = solver
            Return Me
        End Function

        Public Function SetPackage(pkg As PackageNamespace) As PackageEnvironment
            [_namespace] = pkg
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return [namespace].ToString & $"({libpath})"
        End Function
    End Class
End Namespace
