Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts

Namespace Runtime.Interop.Operator

    Public Class ROperatorInvoke

        ReadOnly left, right As RType
        ReadOnly method As MethodInfo

        Sub New(left As RType, right As RType, api As MethodInfo)
            Me.left = left
            Me.right = right
            Me.method = api
        End Sub

        Public Function GetInvoke(argsN As Integer) As IBinaryOperator
            ' fix of System.Reflection.TargetParameterCountException: Parameter count mismatch.

            If argsN = 2 Then
                Return AddressOf Invoke2
            ElseIf argsN = 3 Then
                Return AddressOf Invoke3
            Else
                Throw New InvalidProgramException
            End If
        End Function

        Public Function Invoke2(x As Object, y As Object, internal As Environment) As Object
            x = RCType.CTypeDynamic(x, left, internal)
            y = RCType.CTypeDynamic(y, right, internal)

            If TypeOf x Is Message Then
                Return x
            ElseIf TypeOf y Is Message Then
                Return y
            Else
                Return method.Invoke(Nothing, {x, y})
            End If
        End Function

        Public Function Invoke3(x As Object, y As Object, internal As Environment) As Object
            x = RCType.CTypeDynamic(x, left, internal)
            y = RCType.CTypeDynamic(y, right, internal)

            If TypeOf x Is Message Then
                Return x
            ElseIf TypeOf y Is Message Then
                Return y
            Else
                Return method.Invoke(Nothing, {x, y, internal})
            End If
        End Function

    End Class
End Namespace