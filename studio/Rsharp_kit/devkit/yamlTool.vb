
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("yaml")>
Module yamlTool

    <ExportAPI("parse")>
    <RApiReturn(GetType(YamlStream))>
    Public Function parse(yaml As String, Optional env As Environment = Nothing) As Object
        Dim input As New TextInput(yaml)
        Dim success As Boolean = Nothing
        Dim parser As New YamlParser()
        Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)

        If success Then
            Return yamlStream
        End If

        Return RInternal.debug.stop(parser.BuildErrorMessages.ToArray, env)
    End Function
End Module
