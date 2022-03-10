#Region "Microsoft.VisualBasic::bf8c6e9d65c1fb3a2ab678260514a084, Rscript\CLI\Parallel.vb"

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
'     Function: parallelMode
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter

Partial Module CLI

    <ExportAPI("--parallel")>
    <Usage("--parallel --master <master_port> [--delegate <delegate_name>]")>
    <Description("Create a new parallel thread process for running a new parallel task.")>
    <Argument("--master", False, CLITypes.Integer, AcceptTypes:={GetType(Integer)}, Description:="the TCP port of the master node.")>
    Public Function parallelMode(args As CommandLine) As Integer
        Dim masterPort As Integer = args <= "--master"
        Dim delegateName As String = args("--delegate")
        Dim REngine As New RInterpreter
        Dim plugin As String = LibDLL.GetDllFile($"snowFall.dll", REngine.globalEnvir)

        If plugin.FileExists Then
            Dim reference As NamedValue(Of String)

            If delegateName.StringEmpty Then
                reference = "Parallel::snowFall".GetTagValue("::")
            Else
                reference = delegateName.GetTagValue("::")
            End If

            Call PackageLoader.ParsePackages(plugin) _
                .Where(Function(pkg) pkg.namespace = reference.Name) _
                .FirstOrDefault _
                .DoCall(Sub(pkg)
                            Call REngine.globalEnvir.ImportsStatic(pkg.package)
                        End Sub)
            Call REngine.Invoke(reference.Description, {masterPort})
        Else
            Return 500
        End If

        Return 0
    End Function
End Module
