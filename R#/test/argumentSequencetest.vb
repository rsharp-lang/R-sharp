#Region "Microsoft.VisualBasic::5b3870cf17d49df2992308aa743c9f9d, D:/GCModeller/src/R-sharp/R#/Test//argumentSequencetest.vb"

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

    '   Total Lines: 24
    '    Code Lines: 17
    ' Comment Lines: 1
    '   Blank Lines: 6
    '     File Size: 673 B


    ' Module argumentSequencetest
    ' 
    '     Sub: Main, method1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter

<Package("aaaaa")>
Module argumentSequencetest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()
        Call R.LoadLibrary(GetType(argumentSequencetest))

        ' Call R.Evaluate("test(1, 2)")
        Call R.Evaluate("test(5, c = 6, d = 'a999')")

        Pause()
    End Sub

    <ExportAPI("test")>
    Sub method1(a$, b$, Optional c$ = "111", Optional d$ = "222")
        Call Console.WriteLine({a, b, c, d}.GetJson)
    End Sub
End Module
