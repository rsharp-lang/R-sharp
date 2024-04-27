#Region "Microsoft.VisualBasic::91a80bb3e546b7c55ee7cb5ed43c451c, G:/GCModeller/src/R-sharp/R#/Test//createException.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 423 B


    ' Module createException
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Module createException

    Sub Main()
        Dim env = New RInterpreter().globalEnvir
        Dim msg As Message = Internal.debug.stop({"hello", "12345"}, env, suppress:=True)
        Dim ex As Exception = msg.ToCLRException

        Call App.LogException(ex)

        Throw ex
    End Sub
End Module
