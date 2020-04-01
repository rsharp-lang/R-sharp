Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

<Package("netCDF.utils")>
Module netCDFutils

    <ExportAPI("open.netCDF")>
    Public Function openCDF(file As String) As netCDFReader
        Return netCDFReader.Open(file)
    End Function

    <ExportAPI("globalAttributes")>
    Public Function globalAttributes(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim attrs = DirectCast(file, netCDFReader).globalAttributes
        Dim name As Array = attrs.Select(Function(a) a.name).ToArray
        Dim type As Array = attrs.Select(Function(a) a.type).ToArray
        Dim value As Array = attrs.Select(Function(a) a.value).ToArray
        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {NameOf(name), name},
                {NameOf(type), type},
                {NameOf(value), value}
            },
            .rownames = name
        }

        Return table
    End Function
End Module
