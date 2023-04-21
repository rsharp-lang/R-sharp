Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace jsstd

    <Package("JSON")>
    Public Module JSON

        <ExportAPI("parse")>
        Public Function parse(json As String) As Object

        End Function

        <ExportAPI("stringfy")>
        Public Function stringfy(obj As Object) As Object

        End Function
    End Module
End Namespace