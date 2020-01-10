Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports msXlsx = Microsoft.VisualBasic.MIME.Office.Excel.File
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' 
''' </summary>
<Package("xlsx")>
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
End Module
