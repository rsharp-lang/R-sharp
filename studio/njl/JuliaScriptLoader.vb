Imports SMRUCC.Rsharp.Development.Hybrids
Imports SMRUCC.Rsharp.Runtime

Public Class JuliaScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Property SuffixName As String
        Get
            Return "jl"
        End Get
    End Property

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
        Throw New NotImplementedException()
    End Function
End Class
