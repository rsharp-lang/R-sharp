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
                Return SIMD.Multiply.f64_scalar_op_multiply_f64(CDbl(vx.single), vy.vector)
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector + scalar
                Return SIMD.Multiply.f64_scalar_op_multiply_f64(CDbl(vy.single), vx.vector)
            ElseIf vx.size <> vy.size Then
                Throw New InvalidProgramException("vector size should be matched!")
            Else
                ' vector + vector
                Return SIMD.Multiply.f64_op_multiply_f64(vx.vector, vy.vector)
            End If
        End Function
    End Class
End Namespace