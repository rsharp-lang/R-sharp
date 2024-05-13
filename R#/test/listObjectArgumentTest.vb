#Region "Microsoft.VisualBasic::5d08d0627b5c9e86610120907bbe464e, R#\Test\listObjectArgumentTest.vb"

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

    '   Total Lines: 42
    '    Code Lines: 33
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.49 KB


    ' Module listObjectArgumentTest
    ' 
    '     Function: left, right
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("bbbbb")>
Module listObjectArgumentTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call R.LoadLibrary(GetType(listObjectArgumentTest))

        Call R.Evaluate("left(aa=1,bb=3,cc='d',a='99999')")
        Call R.Evaluate("right(888,b='a', a= 'b', aa = 1111,c=FALSE,d=[12,34,6])")

        Pause()
    End Sub

    <ExportAPI("left")>
    Public Function left(<RListObjectArgument> l As Object, Optional XX As Integer = 9, Optional a As String = "aaaa", Optional env As Environment = Nothing)
        Dim list = base.Rlist(l, env)

        Console.WriteLine(a)
        Console.WriteLine(XX)
        Console.WriteLine(DirectCast(list, list).slots.GetJson)
    End Function

    <ExportAPI("right")>
    Public Function right(XX As Integer, a As String, b As String, <RListObjectArgument> l As Object, Optional env As Environment = Nothing)
        Dim list = base.Rlist(l, env)

        Console.WriteLine(a)
        Console.WriteLine(b)
        Console.WriteLine(XX)
        Console.WriteLine(DirectCast(list, list).slots.GetJson)
    End Function
End Module
