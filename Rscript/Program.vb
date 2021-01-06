#Region "Microsoft.VisualBasic::1f3c519f28850e8a0c803aff9b7d68ca, Rscript\Program.vb"

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
    '     Function: Main, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine, executeFile:=AddressOf Run)
    End Function

    Private Function Run(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If args("--debug") Then
            R.debug = True
        End If

        Call Console.WriteLine(args.ToString)
        Call Console.WriteLine()

        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")
        Call R.LoadLibrary("grDevices")

        Call Console.WriteLine()

        'For Each arg As NamedValue(Of String) In args.ToArgumentVector
        '    Call R.Add(CommandLine.TrimNamePrefix(arg.Name), arg.Value, TypeCodes.generic)
        'Next

        Dim result As Object = R.Source(filepath)

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function
End Module
