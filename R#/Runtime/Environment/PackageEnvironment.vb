#Region "Microsoft.VisualBasic::d77d7a4844afc9090445d269f1c0b4a3, D:/GCModeller/src/R-sharp/R#//Runtime/Environment/PackageEnvironment.vb"

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

    '   Total Lines: 76
    '    Code Lines: 60
    ' Comment Lines: 1
    '   Blank Lines: 15
    '     File Size: 2.96 KB


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
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    Public Class PackageEnvironment : Inherits Environment

        Public ReadOnly Property [namespace] As New PackageNamespace
        Public ReadOnly Property libpath As String

        Dim m_symbolSolver As SymbolNamespaceSolver

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(globalEnv As GlobalEnvironment, packageName As String, libpath As String)
            Call MyBase.New(globalEnv, pkgFrame(packageName), isInherits:=False)

            ' config for load .NET assembly module files
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

        Public Sub AddSymbols(symbols As IEnumerable(Of RFunction))
            For Each callable As RFunction In symbols
                Dim symbol As New Symbol(callable) With {
                    .name = callable.name,
                    .stacktrace = Me.stackTrace
                }

                Call symbol.setMutable([readonly]:=True)

                Me.symbols(callable.name) = symbol
                Me.globalEnvironment.funcSymbols(callable.name) = symbol

                If TypeOf callable Is DeclareNewFunction Then
                    DirectCast(callable, DeclareNewFunction).Namespace = [namespace].packageName
                End If

                If Not m_symbolSolver.funcOverloads.ContainsKey(callable.name) Then
                    m_symbolSolver.funcOverloads.Add(callable.name, New Dictionary(Of String, RFunction))
                End If

                m_symbolSolver.funcOverloads(callable.name)([namespace].packageName) = callable
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
