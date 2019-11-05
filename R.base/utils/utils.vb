Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("utils", Category:=APICategories.UtilityTools, Description:="")>
Public Module utils

    <ExportAPI("write.csv")>
    Public Function writecsv(data As File, file$, envir As Environment) As Object

    End Function
End Module
