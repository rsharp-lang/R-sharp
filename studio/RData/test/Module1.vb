#Region "Microsoft.VisualBasic::dabf2b53501f95b6469004f951222940, R-sharp\studio\RData\test\Module1.vb"

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

    '   Total Lines: 240
    '    Code Lines: 99
    ' Comment Lines: 9
    '   Blank Lines: 132
    '     File Size: 4.40 KB


    ' Module Module1
    ' 
    '     Sub: list, listIO, Main, readmultiple, table
    '          testRealExample, vector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.RDataSet
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module Module1

    ReadOnly R As New RInterpreter

    Sub Main()
        App.CurrentDirectory = "E:\GCModeller\src\R-sharp\studio\test\data\"

        'Call testRealExample()

        Call listIO()

        'Call readmultiple()
        '    Call table()
        ' Call vector()
        ' Call list()
    End Sub

    Sub listIO()
        Dim list As list

        Using file = "test_list2.rda".Open
            Dim obj = Reader.ParseData(file, debug:=True)
            list = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(list)
        End Using

        Using save = "test_write.rda".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            'list.add("save_table", New dataframe With {.columns = New Dictionary(Of String, Array) From {
            '    {"col1", {3, 4, 5, 5, 5}},
            '    {"xxx", {"qqq", "www", "eee", "rr", "r"}}
            '}})

            Dim innerList = New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"aaa_x", New list With {
                        .slots = New Dictionary(Of String, Object) From {
                            {"bbx", 333}
                        }
                    }}
                }
            }

            list.add("Test_inner", innerList)

            Call Writer.Save(list, save)
        End Using

        Call Console.WriteLine(vbNewLine)

        Using read = "test_write.rda".Open
            Dim obj = Reader.ParseData(read, debug:=True)

            list = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(list)
        End Using
    End Sub

    Sub testRealExample()
        Using file = "E:\biodeep\biodeep_pipeline\biodeepNPSearch\data\Flavonoid.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(vec)
        End Using
    End Sub

    Sub readmultiple()
        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\multiple_object.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Print(vec)
        End Using

        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine("-------------------------------------------------------")
    End Sub

    Sub vector()
        Using file = "test_vector.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Print(vec)
        End Using

        Using file = "test_logical.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Print(vec)
        End Using

        Pause()
    End Sub



























    Sub list()
        Using file = "test_list.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim lst = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(lst)
        End Using

        Using file = "test_list2.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim lst = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(lst)
        End Using
    End Sub
































    Sub table()
        Using file = "test_dataframe2.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using

        Using file = "test_dataframe_v3.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using

        Using file = "test_dataframe.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using
    End Sub






































End Module
