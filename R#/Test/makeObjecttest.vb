#Region "Microsoft.VisualBasic::7d23bdbc4546c0ccc543ee30e59e0aef, R#\Test\makeObjecttest.vb"

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

    '   Total Lines: 39
    '    Code Lines: 31 (79.49%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (20.51%)
    '     File Size: 1.16 KB


    ' Module makeObjecttest
    ' 
    '     Sub: createArguments, Main, tupleTest
    ' 
    ' Class arguments
    ' 
    '     Properties: a, b, c
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter

<Package("makeObject")>
Module makeObjecttest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call R.LoadLibrary(GetType(makeObjecttest))

        Call R.Print("tuple")
        Call R.Evaluate("tuple(list(a=1,b= '99999', c=TRUE,d = [599,3.3,9987.01]))")

        Pause()
    End Sub

    <ExportAPI("debug_echo")>
    Public Sub createArguments(args As arguments)
        Call Console.WriteLine(args.GetJson)
        Call Console.WriteLine(args.GetType.FullName)
    End Sub

    <ExportAPI("tuple")>
    Public Sub tupleTest(x As (a As Integer, b As String(), c As Boolean, d As Double()))
        Call Console.WriteLine(x.a)
        Call Console.WriteLine(x.b.GetJson)
        Call Console.WriteLine(x.c)
        Call Console.WriteLine(x.d.GetJson)
    End Sub
End Module

Public Class arguments
    Public Property a As String
    Public Property b As Integer()
    Public Property c As Boolean()
End Class
