Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Hybrids
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Public Class TypeScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Property SuffixName As String
        Get
            Return "ts"
        End Get
    End Property

    Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)

    End Function

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object

    End Function
End Class
