Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

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

    <ExportAPI("check")>
    Public Sub check(<RLazyExpression> condition As Expression, Optional msg As String = Nothing, Optional env As Environment = Nothing)
        Dim expr As String = condition.ToString
        Dim stack As IRuntimeTrace = TryCast(condition, IRuntimeTrace)
        Dim frame As StackFrame = If(If(stack Is Nothing, env.stackFrame, stack.stackFrame), StackFrame.FromUnknownLocation(expr))
        Dim boolVal As Object = condition.Evaluate(env)
        Dim bool As Boolean = CLRVector.asScalarLogical(boolVal)

        Call DebugAudit.Check(bool, msg, expr, frame.File, frame.Line, frame.Method.ToString)
    End Sub

    <ExportAPI("dump")>
    Public Function dump(Optional json As Boolean = False) As String
        If json Then
            Return DebugAudit.ToJson
        Else
            Return DebugAudit.Dump
        End If
    End Function
End Module
