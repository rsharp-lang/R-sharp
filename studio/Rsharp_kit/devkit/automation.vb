
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' utils tools api for create automation pipeline script via R# scripting language.
''' </summary>
<Package("automation")>
Module automationUtils

    <RInitialize>
    Sub Main(env As Environment)
        Call ConfigJSON.LoadConfig(env)?.SetCommandLine(env)
    End Sub

    <ExportAPI("getConfig")>
    Public Function GetConfig(Optional env As Environment = Nothing) As list
        Return ConfigJSON.LoadConfig(env)?.getListConfig
    End Function

End Module

