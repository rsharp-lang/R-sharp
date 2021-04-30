Namespace Runtime.Interop.Operator

    ''' <summary>
    ''' Evaluate of a binary operator
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="right"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Delegate Function IBinaryOperator(left As Object, right As Object, env As Environment) As Object

End Namespace