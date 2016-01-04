Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.HybridsScripting
Imports Microsoft.VisualBasic.Scripting.ShoalShell
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Runtime

<HybridsScripting.LanguageEntryPoint("LINQ", "SQL like query scripting language for the object oriented database.")>
<[Namespace]("LINQ", Description:="SQL like query scripting language for the object oriented database.")>
Public Module API

    Dim LINQ As Global.LINQ.Script.I_DynamicsRuntime

    <ExportAPI("__Init()")>
    <HybridsScripting.EntryInterface(EntryInterface.InterfaceTypes.EntryPointInit)>
    Public Function Initialize() As Boolean
        Global.LINQ.Extensions.ShoalShell.HybridScripting.API.LINQ = New Global.LINQ.Script.I_DynamicsRuntime
        Return LINQ.Initialize
    End Function

    <ExportAPI("EValuate")>
    <HybridsScripting.EntryInterface(EntryInterface.InterfaceTypes.Evaluate)>
    Public Function Evaluate(script As String) As Object
        Return LINQ.Evaluate(script)
    End Function

    <ExportAPI("SetValue")>
    <HybridsScripting.EntryInterface(EntryInterface.InterfaceTypes.SetValue)>
    Public Function SetValue(var As String, value As Object) As Boolean
        Return LINQ.SetVariable(var, value)
    End Function
End Module
