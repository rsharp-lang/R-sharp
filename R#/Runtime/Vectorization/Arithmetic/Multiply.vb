Imports Microsoft.VisualBasic.Math

Namespace Runtime.Vectorization

    Public Class Multiply

        Public Shared Function f64_op_multiply_f64(x As Object, y As Object, env As Environment) As Object
            Dim vx As GetVectorElement = GetVectorElement.Create(Of Double)(x)
            Dim vy As GetVectorElement = GetVectorElement.Create(Of Double)(y)

            If vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CDbl(vx.single) * CDbl(vy.single)
            ElseIf vx.Mode = VectorTypes.Scalar Then
                ' scalar + vector
                Return SIMD.Add.f64_op_add_f64_scalar(vy.vector, CDbl(vx.single))
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector + scalar
                Return SIMD.Add.f64_op_add_f64_scalar(vx.vector, CDbl(vy.single))
            ElseIf vx.size <> vy.size Then
                Throw New InvalidProgramException("vector size should be matched!")
            Else
                ' vector + vector
                Return SIMD.Add.f64_op_add_f64(vx.vector, vy.vector)
            End If
        End Function
    End Class
End Namespace