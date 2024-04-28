#Region "Microsoft.VisualBasic::9ed7f325f829f1d6fec15a65fbb218e5, E:/GCModeller/src/R-sharp/R#//Runtime/Environment/PolyglotInteropEnvironment.vb"

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
    '    Code Lines: 92
    ' Comment Lines: 13
    '   Blank Lines: 22
    '     File Size: 4.71 KB


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

        Private Function TryGetInteropSymbol(s0 As String) As list
            If Not symbols.ContainsKey(s0) Then
                Dim empty_list As New list With {.slots = New Dictionary(Of String, Object)}
                Dim interop_symbol As New Symbol(s0, empty_list, TypeCodes.list, [readonly]:=True)

                Call symbols.Add(interop_symbol)
            End If

            Return symbols(s0).value
        End Function

        Private Sub AddInteropSymbol(ref As String(), rfunc As RMethodInfo)
            Dim tree As list = TryGetInteropSymbol(ref(Scan0))
            Dim walk = ref.Skip(1).ToArray

            Call hook_interop_tree(env:=tree, t:=walk, target_obj:=rfunc)
        End Sub

        Public Const pkg_ref_libs = "$_pkg_ref@-<libs!!!!!>*"
        Public Const js_special_call = "$_js_special_calls?*"

        Private Shared Sub hook_interop_tree(env As list, t As String(), target_obj As Object)
            Dim modObj As list = env
            Dim interop_target As String = t.Last

            ' 20230508 the last token in the R# name is the 
            ' function object itself, do not include into the
            ' tree path
            For Each ti As String In t.Take(t.Length - 1)
                If Not modObj.hasName(ti) Then
                    Call modObj.add(ti, New list With {
                       .slots = New Dictionary(Of String, Object)
                    })
                End If

                Dim value = modObj.getByName(ti)

                If Not TypeOf value Is list Then
                    modObj.slots(ti) = New list With {.slots = New Dictionary(Of String, Object)}
                    modObj = modObj.slots(ti)
                    modObj.add(js_special_call, value)
                Else
                    modObj = value
                End If
            Next

            modObj.slots(interop_target) = target_obj
        End Sub

        ''' <summary>
        ''' construct the interop object for javascript/python
        ''' </summary>
        ''' <param name="libs"></param>
        ''' <returns></returns>
        Public Shared Function hook_jsEnv(ParamArray libs As Type()) As list
            Dim env As New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {pkg_ref_libs, libs}
                }
            }

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
End Namespace
