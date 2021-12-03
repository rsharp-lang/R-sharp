#Region "Microsoft.VisualBasic::416f9da3ff88858a7205a994b1cb8443, studio\RData\test\Module1.vb"

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

' Module Module1
' 
'     Sub: Main, testLogical
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.RDataSet
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Struct

Module Module1

    ReadOnly R As New RInterpreter

    Sub Main()
        Call readmultiple()
        ' Call table()
        ' Call vector()
        ' Call list()
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
        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_vector.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Print(vec)
        End Using

        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_logical.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim vec = ConvertToR.ToRObject(obj.object)

            Call R.Print(vec)
        End Using
    End Sub

    Sub list()
        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_list.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim lst = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(lst)
        End Using

        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_list2.rda".Open
            Dim obj As RData = Reader.ParseData(file)
            Dim lst = ConvertToR.ToRObject(obj.object)

            Call R.Inspect(lst)
        End Using
    End Sub

    Sub table()
        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_dataframe2.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using

        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_dataframe_v3.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using

        Using file = "E:\GCModeller\src\R-sharp\studio\test\data\test_dataframe.rda".Open
            Dim obj = Reader.ParseData(file)
            Dim tbl = ConvertToR.ToRObject(obj.object)

            Call R.Print(tbl)
        End Using
    End Sub

End Module
