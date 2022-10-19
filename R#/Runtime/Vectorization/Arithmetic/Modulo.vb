
Imports Microsoft.VisualBasic.Math

Namespace Runtime.Vectorization

    Public Class Modulo

        Public Shared Function f64_op_modulo_f64(x As Object, y As Object, env As Environment) As Object
            Dim vx As GetVectorElement = GetVectorElement.Create(Of Double)(x)
            Dim vy As GetVectorElement = GetVectorElement.Create(Of Double)(y)

            If vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CDbl(vx.single) Mod CDbl(vy.single)
            ElseIf vx.Mode = VectorTypes.Scalar Then
                ' scalar % vector
                Return SIMD.Modulo.f64_scalar_op_modulo_f64(CDbl(vx.single), vy.vector)
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector % scalar
                Return SIMD.Modulo.f64_op_modulo_f64_scalar(vx.vector, CDbl(vy.single))
            ElseIf vx.size <> vy.size Then
                Throw New InvalidProgramException("vector size should be matched!")
            Else
                ' vector % vector
                Return SIMD.Modulo.f64_op_modulo_f64(vx.vector, vy.vector)
            End If
        End Function
    End Class
End Namespace