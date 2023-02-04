Namespace Runtime.Components.Interface

    Public Interface RPipeline

        ''' <summary>
        ''' is an error message?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property isError As Boolean

        Function getError() As Message

    End Interface
End Namespace