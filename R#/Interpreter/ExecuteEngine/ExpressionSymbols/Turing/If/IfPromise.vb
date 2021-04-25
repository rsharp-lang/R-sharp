Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Friend Class IfPromise

        Public ReadOnly Property Result As Boolean
        Public ReadOnly Property Value As Object
        Public Property assignTo As Expression

        Sub New(value As Object, result As Boolean)
            Me.Value = value
            Me.Result = result
        End Sub

        Public Function DoValueAssign(envir As Environment) As Object
            ' 没有变量需要进行closure的返回值设置
            ' 则跳过
            If assignTo Is Nothing Then
                Return Value
            End If

            Select Case assignTo.GetType
                Case GetType(ValueAssign)
                    Return DirectCast(assignTo, ValueAssign).DoValueAssign(envir, Value)
                Case Else
                    Return Internal.debug.stop(New NotImplementedException, envir)
            End Select
        End Function
    End Class
End Namespace