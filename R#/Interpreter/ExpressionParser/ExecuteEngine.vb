Imports System.Runtime.CompilerServices

Public Module ExecuteEngine

    ''' <summary>
    ''' Operator expression evaluate
    ''' </summary>
    ''' <param name="left"></param>
    ''' <param name="[next]"></param>
    ''' <param name="operator$"></param>
    ''' <returns></returns>
    <Extension> Public Function EvaluateBinary(envir As Environment, left As Variable, [next] As Variable, operator$) As Object
        Dim ta As RType = envir.Types(left.TypeID)
        Dim tb As RType = envir.Types([next].TypeID)

    End Function

    <Extension> Public Function EvaluateUnary(envir As Environment, x As Variable, operator$) As Object
        Dim type As RType = envir.Types(x.TypeID)
        Dim method = type.GetUnaryOperator([operator])

        If method Is Nothing Then
            Throw New NotImplementedException($"Operator '{[operator]}' is not defined!")
        End If

        Dim result As Object = method(x.Value)
        Return result
    End Function
End Module
