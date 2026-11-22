Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("DebugAudit")>
Module debugAuditRTools

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="env"></param>
    ''' <example>
    ''' DebugAudit::capture(a + 1);
    ''' </example>
    <ExportAPI("capture")>
    Public Sub capture(<RLazyExpression> value As Expression, Optional env As Environment = Nothing)
        Call DebugAudit.Capture(value.ToString, val:=value.Evaluate(env))
    End Sub
End Module
