#Region "Microsoft.VisualBasic::47b421c74cc018559fa96b53d69e0c0c, R#\test\docTest.vb"

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

    ' Module docTest
    ' 
    '     Sub: GetDocs, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System

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
        Dim result = doc.GetAnnotations(testApi.GetRawDeclares)
    End Sub
End Module
