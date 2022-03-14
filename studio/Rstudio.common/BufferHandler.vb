Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize

Module BufferHandler

    Public Function getBuffer(result As Object, env As Environment) As Buffer
        Dim buffer As New Buffer

        If TypeOf result Is RReturn Then
            result = DirectCast(result, RReturn).Value
        ElseIf TypeOf result Is ReturnValue Then
            result = DirectCast(result, ReturnValue).Evaluate(env)
        End If

        If result Is Nothing Then
            buffer.data = rawBuffer.getEmptyBuffer
        ElseIf TypeOf result Is dataframe Then
            Throw New NotImplementedException(result.GetType.FullName)
        ElseIf TypeOf result Is vector Then
            buffer.data = vectorBuffer.CreateBuffer(DirectCast(result, vector), env)
        ElseIf TypeOf result Is list Then
            Throw New NotImplementedException(result.GetType.FullName)
        ElseIf TypeOf result Is Message Then
            If env.globalEnvironment.Rscript.debug Then
                Call Rscript.handleResult(result, env.globalEnvironment)
            End If

            buffer.data = New messageBuffer(DirectCast(result, Message))
        ElseIf TypeOf result Is BufferObject Then
            buffer.data = DirectCast(result, BufferObject)
        Else
            Throw New NotImplementedException(result.GetType.FullName)
        End If

        Return buffer
    End Function
End Module
