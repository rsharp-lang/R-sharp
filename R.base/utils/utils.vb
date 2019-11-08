Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.dataframe

<Package("utils", Category:=APICategories.UtilityTools, Description:="")>
Public Module utils

    <ExportAPI("write.csv")>
    Public Function writecsv(data As Rdataframe, file$, envir As Environment) As Object
        Dim matrix As String()() = data.GetTable
        Dim dataframe As New File(matrix.Select(Function(r) New RowObject(r)))

        Return dataframe.Save(path:=file)
    End Function
End Module
