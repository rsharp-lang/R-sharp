#Region "Microsoft.VisualBasic::8bf02506f4e9d48029a84b044cc29b39, studio\R-terminal\CLI\Run.vb"

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

    '   Total Lines: 43
    '    Code Lines: 32 (74.42%)
    ' Comment Lines: 5 (11.63%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 6 (13.95%)
    '     File Size: 1.43 KB


    ' Module CLI
    ' 
    '     Function: (+2 Overloads) runExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

#Const DEBUG = 0

Partial Module CLI

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("-e")>
    <Description("Run a given script expression")>
    Public Function runExpression(args As CommandLine) As Integer
        Return runExpression(args.Tokens.Skip(1).JoinBy(" "))
    End Function

    Public Function runExpression(expr As String) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim [error] As String = Nothing
        Dim program As RProgram = RProgram.BuildProgram(expr, [error]:=[error])
        Dim result As Object

        If Not [error] Is Nothing Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
#If DEBUG Then
            result = R.Run(program)
#Else
            result = REnv.TryCatch(Function() R.Run(program), debug:=R.debug)
#End If
        End If

        Return Rscript.handleResult(result, R.globalEnvir, program)
    End Function
End Module
