#Region "Microsoft.VisualBasic::23b982ca41c411fb3797385d58cf3c66, R-sharp\R#\Runtime\Vectorization\Arithmetic\Add.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 1.17 KB


    '     Class Add
    ' 
    '         Function: f64_op_add_f64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math

Namespace Runtime.Vectorization

    Public Class Add

        Public Shared Function f64_op_add_f64(x As Object, y As Object, env As Environment) As Object
            Dim vx As GetVectorElement = GetVectorElement.Create(Of Double)(x)
            Dim vy As GetVectorElement = GetVectorElement.Create(Of Double)(y)

            If vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CDbl(vx.single) + CDbl(vy.single)
            ElseIf vx.Mode = VectorTypes.Scalar Then
                ' scalar + vector
                Return SIMD.Add.f64_op_add_f64_scalar(vy.vector, CDbl(vx.single))
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector + scalar
                Return SIMD.Add.f64_op_add_f64_scalar(vx.vector, CDbl(vy.single))
            ElseIf vx.size <> vy.size Then
                Throw New InvalidProgramException($"vector size of x({vx.size}) and y({vy.size}) should be matched!")
            Else
                ' vector + vector
                Return SIMD.Add.f64_op_add_f64(vx.vector, vy.vector)
            End If
        End Function
    End Class
End Namespace
