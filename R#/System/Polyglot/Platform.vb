﻿#Region "Microsoft.VisualBasic::965a1d9b282d82f62bdb29317de2021f, R#\System\Polyglot\Platform.vb"

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

    '   Total Lines: 116
    '    Code Lines: 53 (45.69%)
    ' Comment Lines: 45 (38.79%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 18 (15.52%)
    '     File Size: 4.99 KB


    '     Class Platform
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CanHandle, LoadScript, ParseScript, (+2 Overloads) Register
    ' 
    '         Sub: __init_load_base_internals
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development.Polyglot

    ''' <summary>
    ''' polyglot programming
    ''' 
    ''' Polyglot programming is the practice of writing code in multiple 
    ''' languages to capture additional functionality and efficiency not 
    ''' available in a single language. The use of domain specific languages
    ''' (DSLs) has become a standard practice for enterprise application
    ''' development. For example, a mobile development team might employ
    ''' Java, JavaScript and HTML5 to create a fully functional application.
    ''' Other DSLs such as SQL (for data queries), XML (embedded configuration)
    ''' and CSS (document formatting) are often built into enterprise 
    ''' applications as well. One developer may be proficient in multiple 
    ''' languages, or a team with varying language skills may work together 
    ''' to perform polyglot programming.
    '''
    ''' Polyglot programming Is considered necessary When a Single, 
    ''' general-purpose language cannot offer the desired level Of functionality 
    ''' Or speed, interact properly With the database Or the desired delivery 
    ''' platform, Or meet End user expectations. Proponents Of polyglot 
    ''' programming contend that Using the most effective language For Each 
    ''' aspect Of a program enables faster development, greater comprehension
    ''' For business stakeholders, And a more optimal End product. However,
    ''' integrating a wide variety Of languages into a Single application may
    ''' entail added complexity. Resource consumption may increase In terms
    ''' Of training, testing And maintenance. Polyglot programming may also
    ''' make code difficult To deploy If operations Is Not familiar With the
    ''' same languages used In development.
    ''' </summary>
    Public Class Platform

        ReadOnly engine As New Dictionary(Of String, ScriptLoader)
        ReadOnly suffix As New List(Of String)

        ''' <summary>
        ''' symbols solver for javascript/python reference to R# object
        ''' </summary>
        ''' <remarks>
        ''' use this interop environment for the external function closure
        ''' initialization
        ''' </remarks>
        Friend ReadOnly interop As PolyglotInteropEnvironment

        Sub New(_global As GlobalEnvironment)
            Call Register(New RScriptLoader)

            ' 20230508
            ' initialize the R# symbols query helper
            interop = New PolyglotInteropEnvironment(_global)

            ' add internal methods to polyglot environment
            Call __init_load_base_internals()
        End Sub

        Private Sub __init_load_base_internals()
            For Each rfunc As RMethodInfo In Internal.invoke.getAllInternals
                Call interop.AddInteropSymbol(rfunc.name, rfunc)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CanHandle(scriptfile As String) As Boolean
            Return scriptfile.ExtensionSuffix(suffix.ToArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadScript(scriptfile As String, env As Environment) As Object
            Dim key As String = scriptfile.ExtensionSuffix.ToLower
            Dim loader As ScriptLoader = engine(key)

            Return loader.LoadScript(scriptfile, If(key = "r", env, interop))
        End Function

        Public Function ParseScript(scriptfile As String, env As Environment) As Object
            Dim key As String = scriptfile.ExtensionSuffix.ToLower
            Dim loader As ScriptLoader = engine(key)

            Return loader.ParseScript(scriptfile, If(key = "r", env, interop))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dllpath">
        ''' dll full path
        ''' </param>
        ''' <returns></returns>
        Public Function Register(dllpath As String) As Platform
            Dim types As Type() = deps.LoadAssemblyOrCache(dllFile:=dllpath).GetTypes

            For Each type As Type In types
                If type.IsInheritsFrom(GetType(ScriptLoader)) Then
                    Call Me.Register(DirectCast(Activator.CreateInstance(type), ScriptLoader))
                End If
            Next

            Return Me
        End Function

        Public Function Register(loader As ScriptLoader) As Platform
            For Each name As String In loader.SuffixNames.Select(Function(si) si.ToLower)
                engine(name) = loader
                suffix.Add(name)
            Next

            Return Me
        End Function
    End Class
End Namespace
