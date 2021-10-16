
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
