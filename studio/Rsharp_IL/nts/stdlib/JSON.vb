Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd

    <Package("JSON")>
    Public Module JSON

        <ExportAPI("parse")>
        Public Function parse(json As String, Optional env As Environment = Nothing) As Object
            Dim rawElement As JsonElement = New JsonParser().OpenJSON(json)
            Dim obj = rawElement.createRObj(env)
            Return obj
        End Function

        <ExportAPI("stringify")>
        Public Function stringify(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            Return jsonlite.toJSON(obj, env, False, False, enumToStr:=True, unixTimestamp:=True)
        End Function
    End Module
End Namespace