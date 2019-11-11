#Region "Microsoft.VisualBasic::2e016028af3b7abf5f8f8d3cc6696629, R-terminal\Program.vb"

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

' Module Program
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(
            args:=App.CommandLine,
            executeFile:=AddressOf RunScript,
            executeEmpty:=AddressOf Terminal.RunTerminal
        )
    End Function

    Private Function RunScript(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(LocalPackageDatabase.localDb, ConfigFile.localConfigs)

        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")

        For Each arg As NamedValue(Of String) In args.ToArgumentVector
            Call R.Add(arg.Name, arg.Value, TypeCodes.generic)
        Next

        Dim scriptText$ = filepath.ReadAllText
        Dim result As Object = R.Evaluate(scriptText)

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).MessageLevel
        Else
            Return 0
        End If
    End Function
End Module
