Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Enumerations of breaks the executation of the current code closure.
    ''' </summary>
    Public Enum ExecuteBreaks
        ''' <summary>
        ''' returns value for function
        ''' </summary>
        ReturnValue
        ''' <summary>
        ''' continute to next for/while/loop
        ''' </summary>
        ContinuteNext
        ''' <summary>
        ''' break of while/loop
        ''' </summary>
        BreakLoop
    End Enum
End Namespace