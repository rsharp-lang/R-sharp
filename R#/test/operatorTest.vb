#Region "Microsoft.VisualBasic::772d58ed26b9256b98aa4ceb92725874, D:/GCModeller/src/R-sharp/R#/Test//operatorTest.vb"

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

    '   Total Lines: 25
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 594 B


    ' Module operatorTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module operatorTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()

        R.Add("x", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)
        R.Add("y", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)

        R.Print("x+y")
        R.Print("x *2")
        R.Print("x*2.0")
        R.Print("typeof (1+1)")
        R.Print("typeof (x*2.0)")

        R.Print("x %y")

        R.Print("TRUE || ![TRUE, FALSE,FALSE,FALSE,TRUE]")

        Pause()
    End Sub
End Module
