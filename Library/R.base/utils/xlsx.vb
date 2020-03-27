#Region "Microsoft.VisualBasic::1193cae46eab0918808740e7f2618aed, Library\R.base\utils\xlsx.vb"

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

    ' Module xlsx
    ' 
    '     Function: createSheet, createWorkbook, readXlsx, writeXlsx
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports msXlsx = Microsoft.VisualBasic.MIME.Office.Excel.File
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' 
''' </summary>
<Package("xlsx", Category:=APICategories.UtilityTools)>
Module xlsx

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <ExportAPI("read.xlsx")>
    Public Function readXlsx(file$, Optional sheetIndex$ = "Sheet1", Optional env As Environment = Nothing) As Object
        Dim xlsx As msXlsx = msXlsx.Open(file)
        Dim table As csv = xlsx.GetTable(sheetName:=sheetIndex)
        Dim columns = table.Columns.ToArray
        Dim dataframe As New Rdataframe With {
            .columns = columns _
                .ToDictionary(Function(col) col(Scan0),
                              Function(col)
                                  Return DirectCast(col.Skip(1).ToArray, Array)
                              End Function)
        }

        Return dataframe
    End Function

    <ExportAPI("write.xlsx")>
    Public Function writeXlsx(x As Object, file$, Optional sheetName$ = "Sheet1") As Boolean
        Throw New NotImplementedException
    End Function

    <ExportAPI("createWorkbook")>
    Public Function createWorkbook() As msXlsx

    End Function

    <ExportAPI("createSheet")>
    Public Function createSheet(wb As msXlsx, Optional sheetName$ = "Sheet1") As worksheet
        Return wb.AddSheetTable(sheetName)
    End Function

End Module
