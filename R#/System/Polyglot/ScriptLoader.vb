Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.Polyglot

    Public MustInherit Class ScriptLoader

        Public MustOverride ReadOnly Property SuffixName As String

        Public MustOverride Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
        Public MustOverride Function LoadScript(scriptfile As String, env As Environment) As Object

    End Class
End Namespace