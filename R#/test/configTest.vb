#Region "Microsoft.VisualBasic::3e544487949e16a7f5c1b5662a02ac83, R-sharp\R#\Test\configTest.vb"

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


     Code Statistics:

        Total Lines:   20
        Code Lines:    14
        Comment Lines: 0
        Blank Lines:   6
        File Size:     417.00 B


    ' Module configTest
    ' 
    '     Sub: Main, printTest
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter

Module configTest

    Dim R As New RInterpreter

    Sub Main()
        Call printTest()
    End Sub

    Sub printTest()

        Call R.Evaluate("options(max.print = 100)")
        Call R.Print("getOption(""max.print"")")
        Call R.Print("options([""max.print"", ""lib""])")
        Call R.Print("1:500 step 0.125")

        Pause()
    End Sub
End Module
