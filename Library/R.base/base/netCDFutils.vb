Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("netCDF.utils")>
Module netCDFutils

    <ExportAPI("open.netCDF")>
    Public Function openCDF(file As String) As netCDFReader
        Return netCDFReader.Open(file)
    End Function
End Module
