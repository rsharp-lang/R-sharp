Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' R# 脚本侧的 DebugAudit 测试工具包：把 <see cref="DebugAudit"/> 的运行时审计能力
''' (变量快照、条件断言、标记、计时、报告导出) 暴露为一组 R# 可调用的 ExportAPI。
''' </summary>
''' <remarks>
''' 底层 <c>DebugAudit</c> 模块中的 <c>Capture(value, name)</c>、<c>Check(condition, msg, expr, frame)</c>
''' 等非条件编译重载专供脚本解释器调用；本模块的所有 API 在 Release 构建下同样有效。
''' 表达式参数统一使用 <c>&lt;RLazyExpression&gt;</c> 接收原始 AST，从而在失败信息中保留脚本源码文本与调用位置。
''' </remarks>
<Package("DebugAudit")>
Module debugAuditRTools

    ''' <summary>
    ''' 获取表达式的规范化源码文本，作为 CallerArgumentExpression 在 R# 侧的替代方案
    ''' </summary>
    Private Function ExpressionText(value As Expression) As String
        If value Is Nothing Then
            Return "<unknown>"
        End If

        Return value.ToString
    End Function

    ''' <summary>
    ''' 解析表达式对应的脚本堆栈帧：优先使用表达式自身的 trace，其次当前环境堆栈，最后回退到未知位置
    ''' </summary>
    Private Function ExpressionFrame(value As Expression, env As Environment, expr As String) As StackFrame
        Dim trace As IRuntimeTrace = TryCast(value, IRuntimeTrace)
        Dim frame As StackFrame = If(trace Is Nothing, Nothing, trace.stackFrame)

        If frame Is Nothing AndAlso env IsNot Nothing Then
            frame = env.stackFrame
        End If

        If frame Is Nothing Then
            frame = StackFrame.FromUnknownLocation(expr)
        End If

        Return frame
    End Function

    Private Function FrameLine(frame As StackFrame) As Integer
        If frame Is Nothing OrElse String.IsNullOrEmpty(frame.Line) Then
            Return -1
        End If

        Dim line As Integer

        If Integer.TryParse(frame.Line, line) Then
            Return line
        Else
            Return -1
        End If
    End Function

    Private Function FrameMember(frame As StackFrame) As String
        If frame Is Nothing OrElse frame.Method Is Nothing Then
            Return Nothing
        End If

        Return frame.Method.Method
    End Function

    Private Function EvalLogical(value As Expression, env As Environment) As Boolean
        If value Is Nothing Then
            Return False
        End If

        Return CLRVector.asScalarLogical(value.Evaluate(env))
    End Function

    ''' <summary>
    ''' 比较两个运行时值是否相等：先做引用/值相等，再逐元素比较数组，最后做数值与文本回退
    ''' </summary>
    Private Function ValuesEqual(a As Object, b As Object) As Boolean
        If a Is Nothing AndAlso b Is Nothing Then
            Return True
        ElseIf a Is Nothing OrElse b Is Nothing Then
            Return False
        ElseIf Object.Equals(a, b) Then
            Return True
        End If

        Dim va As Array = TryCast(a, Array)
        Dim vb As Array = TryCast(b, Array)

        If va IsNot Nothing AndAlso vb IsNot Nothing Then
            If va.Length <> vb.Length Then
                Return False
            End If

            For i As Integer = 0 To va.Length - 1
                If Not ValuesEqual(va.GetValue(i), vb.GetValue(i)) Then
                    Return False
                End If
            Next

            Return True
        End If

        Try
            Dim na As Double() = CLRVector.asNumeric(a)
            Dim nb As Double() = CLRVector.asNumeric(b)

            If na.Length = nb.Length Then
                For i As Integer = 0 To na.Length - 1
                    If na(i) <> nb(i) Then
                        Return False
                    End If
                Next

                Return True
            End If
        Catch ex As Exception
            ' 非数值类型走下面的文本回退
        End Try

        Return DebugAudit.ValueText(a) = DebugAudit.ValueText(b)
    End Function

    ''' <summary>
    ''' 解析用户给出的异常类型名称，支持完整类型名、程序集内简单名以及 any/error 通配符
    ''' </summary>
    Private Function ResolveExceptionType(name As String) As Type
        If String.IsNullOrEmpty(name) Then
            Return Nothing
        End If

        Dim normalized As String = name.Trim

        If normalized.Equals("any", StringComparison.OrdinalIgnoreCase) OrElse
           normalized.Equals("error", StringComparison.OrdinalIgnoreCase) Then
            Return GetType(Exception)
        End If

        Dim t As Type = Type.GetType(normalized, throwOnError:=False, ignoreCase:=True)

        If t IsNot Nothing Then
            Return t
        End If

        For Each assembly In AppDomain.CurrentDomain.GetAssemblies()
            t = assembly.GetType(normalized, throwOnError:=False, ignoreCase:=True)

            If t IsNot Nothing Then
                Return t
            End If

            Try
                t = assembly.GetTypes() _
                    .FirstOrDefault(Function(x As Type) x.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase))

                If t IsNot Nothing Then
                    Return t
                End If
            Catch ex As Exception
                ' 某些动态程序集不允许枚举类型，忽略即可
            End Try
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' 判断实际抛出的异常是否匹配用户期望的类型
    ''' </summary>
    Private Function ExceptionMatches(raised As Exception, expected As String) As Boolean
        If String.IsNullOrEmpty(expected) Then
            Return True
        End If

        Dim normalized As String = expected.Trim

        If normalized.Equals("any", StringComparison.OrdinalIgnoreCase) OrElse
           normalized.Equals("error", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        If raised Is Nothing Then
            Return False
        End If

        Dim expectedType As Type = ResolveExceptionType(normalized)

        If expectedType IsNot Nothing AndAlso expectedType.IsInstanceOfType(raised) Then
            Return True
        End If

        Dim actualType As Type = raised.GetType

        Return actualType.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase) OrElse
               actualType.FullName.IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ' =====================================================================
    ' 变量快照
    ' =====================================================================

    ''' <summary>
    ''' 记录一个表达式的名称和值到全局变量快照
    ''' </summary>
    ''' <example>
    ''' DebugAudit::capture(a * 86.3332);
    ''' ' captures["a * 86.3332"] = value of the expression
    ''' </example>
    <ExportAPI("capture")>
    Public Sub capture(<RLazyExpression> value As Expression, Optional env As Environment = Nothing)
        Call DebugAudit.Capture(ExpressionText(value), If(value Is Nothing, Nothing, value.Evaluate(env)))
    End Sub

    ''' <summary>
    ''' 以显式名称记录一个表达式的值到全局变量快照
    ''' </summary>
    ''' <example>
    ''' DebugAudit::capture_as("answer", 42);
    ''' </example>
    <ExportAPI("capture_as")>
    Public Sub capture_as(name As String, <RLazyExpression> value As Expression, Optional env As Environment = Nothing)
        If String.IsNullOrEmpty(name) Then
            Return
        End If

        Call DebugAudit.Capture(name, If(value Is Nothing, Nothing, value.Evaluate(env)))
    End Sub

    ''' <summary>
    ''' 用一个命名列表整体替换当前的变量快照，并且写入一条标记事件
    ''' </summary>
    ''' <example>
    ''' DebugAudit::snapshot(list(x = 1, y = 2));
    ''' </example>
    <ExportAPI("snapshot")>
    Public Sub snapshot(<RRawVectorArgument> vars As Object, Optional env As Environment = Nothing)
        Dim names As New List(Of String)

        Call DebugAudit.ClearCaptures()

        If vars IsNot Nothing Then
            For Each item In RListObjectArgumentAttribute.getObjectList(vars, env)
                If Not String.IsNullOrEmpty(item.Name) Then
                    Call DebugAudit.Capture(item.Name, item.Value)
                    Call names.Add(item.Name)
                End If
            Next
        End If

        Call DebugAudit.Mark("snapshot")
    End Sub

    ' =====================================================================
    ' 条件断言
    ' =====================================================================

    ''' <summary>
    ''' 检查一个条件是否成立，失败时记录一条断言事件但不中断脚本
    ''' </summary>
    ''' <example>
    ''' DebugAudit::check(all(a &gt; 0), "all values should be positive");
    ''' </example>
    <ExportAPI("check")>
    Public Sub check(<RLazyExpression> condition As Expression, Optional msg As String = Nothing, Optional env As Environment = Nothing)
        Dim expr As String = ExpressionText(condition)
        Dim frame As StackFrame = ExpressionFrame(condition, env, expr)

        Call DebugAudit.Check(EvalLogical(condition, env), msg, expr, frame)
    End Sub

    ''' <summary>
    ''' 检查一个条件并返回布尔结果，便于在脚本的 if 语句中直接使用
    ''' </summary>
    <ExportAPI("test")>
    Public Function test(<RLazyExpression> condition As Expression, Optional msg As String = Nothing, Optional env As Environment = Nothing) As Boolean
        Dim expr As String = ExpressionText(condition)
        Dim frame As StackFrame = ExpressionFrame(condition, env, expr)

        Return DebugAudit.Test(EvalLogical(condition, env), msg, expr,
                                frame.File, FrameLine(frame), FrameMember(frame))
    End Function

    ''' <summary>
    ''' 强制要求一个条件成立，失败时抛出 <see cref="DebugAssertionException"/> 异常(fail-fast)
    ''' </summary>
    <ExportAPI("require")>
    Public Function require(<RLazyExpression> condition As Expression, Optional msg As String = Nothing, Optional env As Environment = Nothing) As Boolean
        Dim expr As String = ExpressionText(condition)
        Dim frame As StackFrame = ExpressionFrame(condition, env, expr)

        Return DebugAudit.Require(EvalLogical(condition, env), msg, expr,
                                  frame.File, FrameLine(frame), FrameMember(frame))
    End Function

    ''' <summary>
    ''' 比较两个值是否相等，不相等时记录一条断言失败事件
    ''' </summary>
    <ExportAPI("are_equal")>
    Public Function are_equal(expected As Object, actual As Object, Optional msg As String = Nothing, Optional env As Environment = Nothing) As Boolean
        Dim expr As String = $"{DebugAudit.ValueText(expected)} == {DebugAudit.ValueText(actual)}"
        Dim frame As StackFrame = ExpressionFrame(Nothing, env, expr)

        Return DebugAudit.Test(ValuesEqual(expected, actual), msg, expr,
                               frame.File, FrameLine(frame), FrameMember(frame))
    End Function

    ''' <summary>
    ''' 检查一段脚本代码是否抛出异常，可指定期望的异常类型名称
    ''' </summary>
    ''' <example>
    ''' DebugAudit::check_throws(stop("boom"));
    ''' DebugAudit::check_throws(stop("boom"), "RuntimeError");
    ''' </example>
    <ExportAPI("check_throws")>
    Public Function check_throws(<RLazyExpression> code As Expression,
                                 Optional expected As String = Nothing,
                                 Optional msg As String = Nothing,
                                 Optional env As Environment = Nothing) As Boolean
        Dim expr As String = ExpressionText(code)
        Dim frame As StackFrame = ExpressionFrame(code, env, expr)
        Dim raised As Exception = Nothing
        Dim result As Object = Nothing

        If code IsNot Nothing Then
            Try
                result = code.Evaluate(env)
            Catch ex As Exception
                raised = ex
            End Try
        End If

        ' R# 的 stop() 返回 Message 对象而不是直接抛出 CLR 异常
        If raised Is Nothing AndAlso TypeOf result Is Message Then
            raised = DirectCast(result, Message).ToCLRException()
        End If

        Dim passed As Boolean = raised IsNot Nothing AndAlso ExceptionMatches(raised, expected)
        Dim detail As String = msg

        If String.IsNullOrEmpty(detail) Then
            If raised Is Nothing Then
                detail = $"no exception was thrown, but {If(expected, "an error")} was expected"
            Else
                detail = $"{raised.GetType.Name} was thrown"
            End If
        End If

        Return DebugAudit.Test(passed, detail, expr,
                               frame.File, FrameLine(frame), FrameMember(frame))
    End Function

    ' =====================================================================
    ' 标记、计时与辅助
    ' =====================================================================

    ''' <summary>
    ''' 写入一个手动标记点，可选地同时记录一个变量值到快照
    ''' </summary>
    <ExportAPI("mark")>
    Public Sub mark(name As String, Optional value As Object = Nothing, Optional env As Environment = Nothing)
        Call DebugAudit.Mark(name, value)
    End Sub

    ''' <summary>
    ''' 计时执行一段脚本代码，返回其求值结果并自动写入一条 Scope 审计事件
    ''' </summary>
    ''' <example>
    ''' let result = DebugAudit::watch("loading", read.csv("data.csv"));
    ''' </example>
    <ExportAPI("watch")>
    Public Function watch(name As String, <RLazyExpression> code As Expression, Optional env As Environment = Nothing) As Object
        Dim scope As DebugAuditScope = DebugAudit.Watch(name, logOnExit:=False)

        Try
            Return If(code Is Nothing, Nothing, code.Evaluate(env))
        Finally
            Call scope.Dispose()
        End Try
    End Function

    ''' <summary>
    ''' 获取一个表达式的源码文本，用于验证 lazy expression 参数绑定
    ''' </summary>
    <ExportAPI("expr_text")>
    Public Function expr_text(<RLazyExpression> value As Expression) As String
        Return ExpressionText(value)
    End Function

    ' =====================================================================
    ' 报告输出
    ' =====================================================================

    ''' <summary>
    ''' 导出当前的变量快照与审计事件报告
    ''' </summary>
    ''' <param name="json">为 True 时输出 JSON 文本，否则输出对齐的文本表格</param>
    ''' <param name="includeRecords">文本表格模式下是否包含审计事件列表</param>
    <ExportAPI("dump")>
    Public Function dump(Optional json As Boolean = False, Optional includeRecords As Boolean = True) As String
        If json Then
            Return DebugAudit.ToJson()
        Else
            Return DebugAudit.Dump(includeRecords)
        End If
    End Function

    <ExportAPI("to_json")>
    Public Function to_json(Optional indent As Boolean = True) As String
        Return DebugAudit.ToJson(indent)
    End Function

    ''' <summary>
    ''' 将审计报告保存为 JSON 文件，返回目标路径
    ''' </summary>
    <ExportAPI("save_report")>
    Public Function save_report(path As String) As String
        Call DebugAudit.SaveReport(path)
        Return path
    End Function

    <ExportAPI("value_text")>
    Public Function value_text(value As Object) As String
        Return DebugAudit.ValueText(value)
    End Function

    ' =====================================================================
    ' 状态查询与管理
    ' =====================================================================

    <ExportAPI("clear_captures")>
    Public Sub clear_captures()
        Call DebugAudit.ClearCaptures()
    End Sub

    <ExportAPI("reset")>
    Public Sub reset()
        Call DebugAudit.Reset()
    End Sub

    <ExportAPI("failure_count")>
    Public Function failure_count() As Integer
        Return DebugAudit.FailureCount
    End Function

    <ExportAPI("last_failure")>
    Public Function last_failure() As DebugAuditEvent
        Return DebugAudit.LastFailure
    End Function

    <ExportAPI("records")>
    Public Function records() As DebugAuditEvent()
        Return DebugAudit.Records
    End Function

    <ExportAPI("captures")>
    Public Function captures() As Dictionary(Of String, Object)
        Return DebugAudit.GetCaptures()
    End Function

    ''' <summary>
    ''' 查询或设置审计事件缓冲区的长度上限
    ''' </summary>
    <ExportAPI("max_events")>
    Public Function max_events(Optional value As Object = Nothing) As Integer
        If value Is Nothing Then
            Return DebugAudit.MaxEvents
        End If

        DebugAudit.MaxEvents = CInt(CLRVector.asScalarNumber(value))
        Return DebugAudit.MaxEvents
    End Function

    ''' <summary>
    ''' 查询或设置断言失败时是否直接抛出异常
    ''' </summary>
    <ExportAPI("throw_on_failure")>
    Public Function throw_on_failure(Optional value As Object = Nothing) As Boolean
        If value Is Nothing Then
            Return DebugAudit.ThrowOnFailure
        End If

        DebugAudit.ThrowOnFailure = CLRVector.asScalarLogical(value)
        Return DebugAudit.ThrowOnFailure
    End Function
End Module
