Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Rscript

    Dim echo As Index(Of String) = {"print", "cat", "echo", "q", "quit", "require", "library", "str"}

    Friend Function handleResult(result As Object, globalEnv As GlobalEnvironment, program As RProgram) As Integer
        Dim requirePrintErr As Boolean = False

        If RProgram.isException(result, globalEnv, isDotNETException:=requirePrintErr) Then
            If requirePrintErr Then
                Call REnv.Internal.debug.PrintMessageInternal(result)
            End If

            Return 500
        End If

        If program.EndWithFuncCalls(echo.Objects) Then
            ' do nothing
            Dim funcName As Literal = DirectCast(program.Last, FunctionInvoke).funcName

            If funcName = "cat" Then
                Call Console.WriteLine()
            End If
        ElseIf Not program.isValueAssign AndAlso Not program.isImports Then
            If Not isInvisible(result) Then
                Call base.print(result, globalEnv)
            End If
        End If

        Return 0
    End Function

    Private Function isInvisible(result As Object) As Boolean
        If result Is Nothing Then
            Return False
        ElseIf result.GetType Is GetType(RReturn) Then
            Return DirectCast(result, RReturn).invisible
        ElseIf result.GetType Is GetType(invisible) Then
            Return True
        Else
            Return False
        End If
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isImports(program As RProgram) As Boolean
        If program.Count <> 1 Then
            Return False
        Else
            Dim Rexp As Expression = program.First

            If TypeOf Rexp Is [Imports] OrElse TypeOf Rexp Is Require Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isValueAssign(program As RProgram) As Boolean
        ' 如果是赋值表达式的话，也不会在终端上打印结果值
        Return TypeOf program.Last Is ValueAssign OrElse TypeOf program.Last Is DeclareNewVariable
    End Function
End Module
