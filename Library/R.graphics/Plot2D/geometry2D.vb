#Region "Microsoft.VisualBasic::318f9838769e43ed624a02a3dce839ee, Library\R.graphics\Plot2D\geometry2D.vb"

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

    ' Module geometry2D
    ' 
    '     Function: density2D
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("geometry2D")>
Module geometry2D

    ''' <summary>
    ''' Evaluate the density value of a set of 2d points.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="k"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a density value vector. the elements in the resulted density 
    ''' value vector is keeps the same order as the input [x,y] 
    ''' vector.
    ''' </returns>
    <ExportAPI("density2D")>
    <RApiReturn(GetType(Double))>
    Public Function density2D(x As Integer(), y As Integer(),
                              Optional k As Integer = 6,
                              Optional env As Environment = Nothing) As Object

        If x.IsNullOrEmpty OrElse y.IsNullOrEmpty Then
            Return Nothing
        ElseIf x.Length <> y.Length Then
            Return Internal.debug.stop($"the size({x.Length}) of vector x must be equals to the size({y.Length}) of vector y!", env)
        End If

        Dim density As Dictionary(Of String, Double) = x _
            .AsParallel _
            .Select(Function(xi, i) (xi, y(i), $"{xi},{y(i)}")) _
            .Density(Function(d) d.Item3, Function(d) d.Item1, Function(d) d.Item2, New Size(k, k)) _
            .ToDictionary(Function(d) d.Name,
                            Function(d)
                                Return d.Value
                            End Function)
        Dim vDensity As Double() = x _
            .Select(Function(xi, i) density($"{xi},{y(i)}")) _
            .ToArray

        Return vDensity
    End Function

End Module
