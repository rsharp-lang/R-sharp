#Region "Microsoft.VisualBasic::dd72119a0ca7f2ffdc013b841a3c72bb, E:/GCModeller/src/R-sharp/studio/R-terminal//RunScript.vb"

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

    '   Total Lines: 35
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.03 KB


    ' Class RunScript
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports SMRUCC.Rsharp.Interpreter
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Friend Class RunScript

    ReadOnly script As String
    ReadOnly R As RInterpreter

    Sub New(R As RInterpreter, script As String)
        Me.R = R
        Me.script = script
    End Sub

    Public Async Function doRunScript(ct As CancellationToken) As Task(Of Integer)
        Dim error$ = Nothing
        Dim program As RProgram = RProgram.BuildProgram(script, [error]:=[error])
        Dim result As Object

        Await Task.Delay(1)

        If Not [error].StringEmpty Then
            result = REnv.Internal.debug.stop([error], R.globalEnvir)
        Else
            result = REnv.TryCatch(
                runScript:=Function() R.SetTaskCancelHook(Terminal.cts).Run(program),
                debug:=R.debug
            )
        End If

        Return Rscript.handleResult(result, R.globalEnvir, program)
    End Function

End Class
