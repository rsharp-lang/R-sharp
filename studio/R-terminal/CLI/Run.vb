#Region "Microsoft.VisualBasic::642e1ea68b74bf33a41e530376d73659, E:/GCModeller/src/R-sharp/studio/R-terminal//CLI/Run.vb"

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

    '   Total Lines: 36
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.23 KB


    ' Module CLI
    ' 
    '     Function: (+2 Overloads) runExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
#Const DEBUG = 0

Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Partial Module CLI

    <ExportAPI("-e")>
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
