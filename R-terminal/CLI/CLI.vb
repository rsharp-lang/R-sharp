#Region "Microsoft.VisualBasic::6cab97a720193a1da453254662b1ad2c, R-terminal\CLI\CLI.vb"

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

' Module CLI
' 
'     Function: Install
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports R.exec
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll> [--verbose]")>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim verboseMode As Boolean = args("--verbose")
        Dim config As New Options(ConfigFile.localConfigs)

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        Else
            Dim pkgMgr As New PackageManager(config)

            Call pkgMgr.InstallLocals(dllFile:=[module])
            Call pkgMgr.Flush()
        End If

        Return 0
    End Function

    <ExportAPI("/compile")>
    <Usage("/compile --script <script.R> [--out <app.exec>]")>
    Public Function Compile(args As CommandLine) As Integer
        Dim script$ = args <= "--script"
        Dim out$ = args("--out") Or $"{script.TrimSuffix}.exec"
        Dim assembly = Compiler.Build(script.ReadAllText).ToArray
    End Function
End Module
