#Region "Microsoft.VisualBasic::095882ccdf3acbc9c01fcb5700ad1944, G:/GCModeller/src/R-sharp/R#//Runtime/Vectorization/Arithmetic/Modulo.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 37
    '    Code Lines: 29
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 1.55 KB


    '     Class Modulo
    ' 
    '         Function: f64_op_modulo_f64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math

Namespace Runtime.Vectorization

    Public Class Modulo

        Public Shared Function f64_op_modulo_f64(x As Object, y As Object, env As Environment) As Object
            Dim vx As GetVectorElement = GetVectorElement.Create(Of Double)(x)
            Dim vy As GetVectorElement = GetVectorElement.Create(Of Double)(y)

            If vx.Mode = VectorTypes.Error Then
                Return vx.Error
            ElseIf vy.Mode = VectorTypes.Error Then
                Return vy.Error
            End If

            If vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CDbl(vx.single) Mod CDbl(vy.single)
            ElseIf vx.Mode = VectorTypes.Scalar Then
                ' scalar % vector
                Return SIMD.Modulo.f64_scalar_op_modulo_f64(CDbl(vx.single), vy.vector)
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector % scalar
                Return SIMD.Modulo.f64_op_modulo_f64_scalar(vx.vector, CDbl(vy.single))
            ElseIf vx.size <> vy.size Then
                Return Internal.debug.stop({
                    $"vector size of x({vx.size}) and y({vy.size}) should be matched!",
                    $"sizeof_x: {vx.size}",
                    $"sizeof_y: {vy.size}"
                }, env)
            Else
                ' vector % vector
                Return SIMD.Modulo.f64_op_modulo_f64(vx.vector, vy.vector)
            End If
        End Function
    End Class
End Namespace
