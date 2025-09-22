#Region "Microsoft.VisualBasic::a989c20321af62af828d1a63e5693f17, R#\Runtime\Environment\PolyglotInteropEnvironment.vb"

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

    '   Total Lines: 127
    '    Code Lines: 92 (72.44%)
    ' Comment Lines: 13 (10.24%)
    '    - Xml Docs: 61.54%
    ' 
    '   Blank Lines: 22 (17.32%)
    '     File Size: 4.72 KB


    '     Class PolyglotInteropEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: hook_jsEnv, TryGetInteropSymbol
    ' 
    '         Sub: (+3 Overloads) AddInteropSymbol, hook_interop_tree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    ''' <summary>
    ''' symbols solver for javascript/python reference to R# object
    ''' </summary>
    Public Class PolyglotInteropEnvironment : Inherits Environment

        Sub New(_global As GlobalEnvironment)
            Call MyBase.New(_global)

            ' set stack frame for current polyglot interop environment
            Call setStackInfo(New StackFrame With {
                .File = "runtime_polyglot_interop.vbs",
                .Line = "999",
                .Method = New Method With {
                    .Method = "interop_call",
                    .[Module] = "PolyglotInteropEnvironment",
                    .[Namespace] = "SMRUCC_rsharp"
                }
            })
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddInteropSymbol(symbol As String, libs As Type())
            Call Push(
                name:=symbol,
                value:=hook_jsEnv(libs),
                [readonly]:=True,
                mode:=TypeCodes.list,
                [overrides]:=True
            )
        End Sub

        Public Sub AddInteropSymbol(symbol As String, func As RMethodInfo)
            Dim t As String() = symbol.Split("."c)

            If t.Length = 1 Then
                funcSymbols(symbol) = New Symbol(func)
            Else
                ' add to symbols
                Call AddInteropSymbol(t, func)
            End If
        End Sub

        Private Function TryGetInteropSymbol(s0 As String) As SymbolPrefixTree
            If Not symbols.CheckSymbolExists(s0) Then
                Dim empty_list As New SymbolPrefixTree(s0)
                Dim interop_symbol As New Symbol(s0, empty_list, TypeCodes.list, [readonly]:=True)

                Call symbols.Add(interop_symbol)
            End If

            Return symbols(s0).value
        End Function

        Private Sub AddInteropSymbol(ref As String(), rfunc As RMethodInfo)
            Dim tree As SymbolPrefixTree = TryGetInteropSymbol(ref(Scan0))
            Dim walk = ref.Skip(1).ToArray

            Call hook_interop_tree(env:=tree, t:=walk, target_obj:=rfunc)
        End Sub

        Public Const pkg_ref_libs = "$_pkg_ref@-<libs!!!!!>*"
        Public Const js_special_call = "$_js_special_calls?*"

        Private Shared Sub hook_interop_tree(env As SymbolPrefixTree, t As String(), target_obj As Object)
            Dim modObj As SymbolPrefixTree = env

            ' 20230508 the last token in the R# name is the 
            ' function object itself, do not include into the
            ' tree path
            For Each ti As String In t
                If Not modObj.hasName(ti) Then
                    Call modObj.add(ti)
                End If

                Dim value = modObj(ti)

                If Not TypeOf value Is SymbolPrefixTree Then
                    modObj.add(ti)
                    modObj.setNodeClosure(value)
                Else
                    modObj = value
                End If
            Next

            Call modObj.setNodeClosure(target_obj)
        End Sub

        ''' <summary>
        ''' construct the interop object for javascript/python
        ''' </summary>
        ''' <param name="libs"></param>
        ''' <returns></returns>
        Public Shared Function hook_jsEnv(ParamArray libs As Type()) As SymbolPrefixTree
            Dim env As New SymbolPrefixTree(pkg_ref_libs, libs)

            For Each type As Type In libs
                For Each func As NamedValue(Of MethodInfo) In ImportsPackage.GetAllApi(type)
                    Dim t As String() = func.Name.Split("."c)
                    Dim target_obj As New RMethodInfo(func)

                    Call hook_interop_tree(env, t, target_obj)
                Next
            Next

            Return env
        End Function
    End Class

    Public Class SymbolPrefixTree

        ReadOnly tree As New Dictionary(Of String, SymbolPrefixTree)
        ReadOnly name As String
        ReadOnly libs As Type()

        Dim closure As Expression

        Default Public ReadOnly Property GetByName(name As String) As Object
            Get
                Return If(tree.ContainsKey(name), tree(name), closure)
            End Get
        End Property

        Sub New(name As String)
            Me.name = name
        End Sub

        Sub New(name As String, libs As Type())
            Me.name = name
            Me.libs = libs
        End Sub

        Public Sub setNodeClosure(expr As Expression)
            closure = expr
        End Sub

        Public Function hasName(name As String) As Boolean
            Return tree.ContainsKey(name)
        End Function

        Public Sub add(name As String)
            Call tree.Add(name, New SymbolPrefixTree(name))
        End Sub

    End Class
End Namespace
