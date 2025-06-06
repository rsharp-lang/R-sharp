﻿#Region "Microsoft.VisualBasic::93800aaad21ee10d52d8d8c1375fbebe, R#\System\Config\OptionHooks.vb"

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

    '   Total Lines: 82
    '    Code Lines: 59 (71.95%)
    ' Comment Lines: 12 (14.63%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 11 (13.41%)
    '     File Size: 3.14 KB


    '     Module OptionHooks
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckHook
    ' 
    '         Sub: Add, AddDefaultHooks, configAvxSIMD, configMemoryLoads, UpdateConfigurationCallback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Parallel

Namespace Development.Configuration

    ''' <summary>
    ''' hooks for set options
    ''' </summary>
    Public Module OptionHooks

        ReadOnly hooks As New Dictionary(Of String, Action(Of String))

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="opt">
        ''' the option name
        ''' </param>
        ''' <param name="callback"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(opt As String, callback As Action(Of String))
            hooks(opt) = callback
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CheckHook(opt As String) As Boolean
            Return hooks.ContainsKey(opt)
        End Function

        Public Sub UpdateConfigurationCallback(opt As String, configVal As String)
            If hooks.ContainsKey(opt) Then
                Call hooks(opt)(configVal)
            Else
                ' do nothing
            End If
        End Sub

        Public Sub AddDefaultHooks()
            hooks("memory.load") = New Action(Of String)(AddressOf configMemoryLoads)
            hooks("memory.loads") = New Action(Of String)(AddressOf configMemoryLoads)
            hooks("avx_simd") = New Action(Of String)(AddressOf configAvxSIMD)
            hooks("simd") = New Action(Of String)(AddressOf configAvxSIMD)
            hooks("SIMD") = New Action(Of String)(AddressOf configAvxSIMD)
            hooks("AVX") = New Action(Of String)(AddressOf configAvxSIMD)
            hooks("avx") = New Action(Of String)(AddressOf configAvxSIMD)
            hooks("n_thread") = New Action(Of String)(Sub(n) VectorTask.n_threads = Val(n))
            hooks("n_threads") = New Action(Of String)(Sub(n) VectorTask.n_threads = Val(n))
        End Sub

        Private Sub configMemoryLoads(optVal As String)
            If optVal = "max" Then
                Call FrameworkInternal.ConfigMemory(MemoryLoads.Heavy)
            Else
                Call FrameworkInternal.ConfigMemory(MemoryLoads.Light)
            End If
        End Sub

        Private Sub configAvxSIMD(optVal As String)
            If PrimitiveParser.IsBooleanFactor(optVal) Then
                If optVal.ParseBoolean Then
                    SIMDEnvironment.config = SIMDConfiguration.auto
                Else
                    SIMDEnvironment.config = SIMDConfiguration.disable
                End If
            ElseIf optVal.ToLower = "legacy" Then
                SIMDEnvironment.config = SIMDConfiguration.legacy
            ElseIf optVal.ToLower = "enable" Then
                SIMDEnvironment.config = SIMDConfiguration.enable
            Else
                Call $"invalid SIMD environment configuration: {optVal}!".Warning
            End If
        End Sub

    End Module
End Namespace
