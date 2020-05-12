Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    Public Delegate Function IBinaryOperator(left As Object, right As Object, env As Environment) As Object

    Public Class BinaryOperator

        Public Property operatorSymbol As String
        Public Property left As RType
        Public Property right As RType

        ReadOnly operation As IBinaryOperator

        Sub New(op As IBinaryOperator)
            operation = op
        End Sub

        ''' <summary>
        ''' 主要是为了兼容R#语言的<see cref="unit"/>特性
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function Execute(left As Object, right As Object, env As Environment) As Object
            Dim result As Object = operation(left, right, env)
            Dim unitL As unit = Nothing
            Dim unitR As unit = Nothing
            Dim unit As unit

            If TypeOf left Is vector Then
                unitL = DirectCast(left, vector).unit
            End If
            If TypeOf right Is vector Then
                unitR = DirectCast(right, vector).unit
            End If

            If (Not result Is Nothing) AndAlso (Not (unitL Is Nothing AndAlso unitR Is Nothing)) Then
                If unitL Is Nothing Then
                    unit = unitR
                ElseIf unitR Is Nothing Then
                    unit = unitL
                Else
                    unit = New unit With {
                        .name = $"{unitL.name}{operatorSymbol}{unitR.name}"
                    }
                End If

                If TypeOf result Is vector Then
                    DirectCast(result, vector).unit = unit
                ElseIf result.GetType.IsArray Then
                    result = New vector(MeasureRealElementType(result), result, env)
                    DirectCast(result, vector).unit = unit
                Else
                    env.AddMessage({
                         $"the result value is not supported by R# unit, unit ({unit}) will be ignored...",
                         $"unit: " & unit.name,
                         $"value: " & result.GetType.FullName
                    }, MSG_TYPES.WRN)
                End If
            End If

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} {operatorSymbol} {right})"
        End Function

    End Class
End Namespace