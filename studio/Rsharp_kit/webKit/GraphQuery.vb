Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

<Package("graphquery")>
Public Module HtmlGraphQuery

    <ExportAPI("parseQuery")>
    Public Function parseQuery(graphquery As String) As Query
        Return QueryParser.GetQuery(graphquery)
    End Function

    <ExportAPI("query")>
    Public Function query(document As Object, graphquery As Object,
                          Optional raw As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        If TypeOf graphquery Is String Then
            graphquery = QueryParser.GetQuery(graphquery)
        End If
        If TypeOf document Is String Then
            document = HtmlDocument.LoadDocument(document)
        End If

        Dim data As JsonElement = New Engine().Execute(DirectCast(document, HtmlDocument), graphquery)

        If raw Then
            Return data
        Else
            Return data.createRObj(env)
        End If
    End Function
End Module
