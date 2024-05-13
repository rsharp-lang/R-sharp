#Region "Microsoft.VisualBasic::e8f98689b924093360ea2894db149d66, R#\Test\defaultValueTest.vb"

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

    '   Total Lines: 44
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.22 KB


    ' Module defaultValueTest
    ' 
    '     Sub: Main, nullableTest, testApi
    ' 
    ' Class defaultVector
    ' 
    '     Properties: data
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop

Module defaultValueTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call nullableTest()

        Call R.LoadLibrary(GetType(defaultValueTest))
        Call R.Print("testApi")

        Pause()
    End Sub

    Sub nullableTest()
        Dim type = RType.GetRSharpType(GetType(ConsoleColor?))

        Pause()
    End Sub

    <ExportAPI(NameOf(testApi))>
    Sub testApi(<RDefaultValue("1,1,1,1,0,0,0,0,0,0,yes,false,no,0,0,0,0,0,0,1,0,1,0,1,0,0")>
                Optional flags As defaultVector = Nothing)

        Call Console.WriteLine(flags.data.GetJson)
    End Sub
End Module

Class defaultVector

    Public Property data As Boolean()

    Public Shared Widening Operator CType(default$) As defaultVector
        Return New defaultVector With {.data = [default].Split(","c).Select(AddressOf ParseBoolean).ToArray}
    End Operator

    Public Shared Narrowing Operator CType(vec As defaultVector) As String
        Return vec.data.GetJson
    End Operator
End Class
