﻿#Region "Microsoft.VisualBasic::acc2103d18970290544a068a29fea2f4, R#\Test\docTest.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15 (68.18%)
    ' Comment Lines: 2 (9.09%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 541 B


    ' Module docTest
    ' 
    '     Sub: GetDocs, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop

Module docTest

    Dim r As New RInterpreter

    Sub Main()
        Call r.Print("source")
        'Call r.Evaluate("print(length)")
        'Call r.Evaluate("print(names)")

        Pause()
    End Sub

    Sub GetDocs()
        Dim doc As New AnnotationDocs
        Dim testApi As RMethodInfo = r.Evaluate("log")
        Dim result = doc.GetAnnotations(testApi.GetNetCoreCLRDeclaration)
    End Sub
End Module
