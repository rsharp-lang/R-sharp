
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.TypeScript

''' <summary>
''' Polyglot
''' </summary>
<Package("javascript")>
Public Module JavaScript

    <ExportAPI("parse")>
    Public Function Parse(script As String, Optional env As Environment = Nothing) As Object
        Dim scriptReader As New TypeScriptLoader
        Dim app As Program = scriptReader.ParseScript(script, env)
        Return app
    End Function
End Module
