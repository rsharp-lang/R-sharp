#Region "Microsoft.VisualBasic::eb9a01017fa05e0c83455309041515b9, Library\R.math\stats.vb"

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

' Module stats
' 
'     Function: dist, prcomp, spline, tabulateMode
' 
' Enum SplineAlgorithms
' 
'     Bezier, BSpline, CatmullRom, CubiSpline
' 
'  
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Prcomp
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("stats")>
Module stats

    Sub New()
        Internal.ConsolePrinter.AttachConsoleFormatter(Of DistanceMatrix)(AddressOf printMatrix)
    End Sub

    Private Function printMatrix(d As DistanceMatrix) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"Distance matrix of {d.Keys.Length} objects:")
        Call sb.AppendLine(d.ToString)
        Call sb.AppendLine()

        For Each row In d.PopulateRowObjects(Of DataSet).Take(6)
            Call sb.AppendLine($"{row.ID}: {row.Properties.Take(8).Select(Function(t) $"{t.Key}:{t.Value.ToString("F2")}").JoinBy(", ")} ...")
        Next

        Call sb.AppendLine("...")

        Return sb.ToString
    End Function

    ''' <summary>
    ''' Interpolating Splines
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("spline")>
    Public Function spline(<RRawVectorArgument> data As Object,
                           Optional algorithm As SplineAlgorithms = SplineAlgorithms.BSpline,
                           Optional env As Environment = Nothing) As Object

        If data Is Nothing Then
            Return Nothing
        End If

        Select Case algorithm
            Case SplineAlgorithms.Bezier
            Case SplineAlgorithms.BSpline
            Case SplineAlgorithms.CatmullRom
            Case SplineAlgorithms.CubiSpline

        End Select

        Return Internal.debug.stop($"unsupported spline algorithm: {algorithm.ToString}", env)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("tabulate.mode")>
    Public Function tabulateMode(<RRawVectorArgument> x As Object) As Double
        Return REnv _
            .asVector(Of Double)(x) _
            .DoCall(Function(vec)
                        Return Bootstraping.TabulateMode(DirectCast(vec, Double()))
                    End Function)
    End Function

    ''' <summary>
    ''' ### Principal Components Analysis
    ''' 
    ''' Performs a principal components analysis on the given data matrix 
    ''' and returns the results as an object of class ``prcomp``.
    ''' 
    ''' The calculation is done by a singular value decomposition of the 
    ''' (centered and possibly scaled) data matrix, not by using eigen on 
    ''' the covariance matrix. This is generally the preferred method for 
    ''' numerical accuracy. The print method for these objects prints the 
    ''' results in a nice format and the plot method produces a scree 
    ''' plot.
    '''
    ''' Unlike princomp, variances are computed With the usual divisor N - 1.
    ''' Note that scale = True cannot be used If there are zero Or constant 
    ''' (For center = True) variables.
    ''' </summary>
    ''' <param name="x">
    ''' a numeric or complex matrix (or data frame) which provides the 
    ''' data for the principal components analysis.
    ''' </param>
    ''' <param name="center">
    ''' a logical value indicating whether the variables should be shifted 
    ''' to be zero centered. Alternately, a vector of length equal the 
    ''' number of columns of x can be supplied. The value is passed to scale.
    ''' </param>
    ''' <param name="scale">
    ''' a logical value indicating whether the variables should be scaled to 
    ''' have unit variance before the analysis takes place. The default is 
    ''' FALSE for consistency with S, but in general scaling is advisable. 
    ''' Alternatively, a vector of length equal the number of columns of x 
    ''' can be supplied. The value is passed to scale.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The signs of the columns of the rotation matrix are arbitrary, and 
    ''' so may differ between different programs for PCA, and even between 
    ''' different builds of R.
    ''' </remarks>
    <ExportAPI("prcomp")>
    <RApiReturn(GetType(PCA))>
    Public Function prcomp(<RRawVectorArgument>
                           x As Object,
                           Optional scale As Boolean = False,
                           Optional center As Boolean = False,
                           Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Internal.debug.stop("'data' must be of a vector type, was 'NULL'", env)
        End If

        Dim matrix As Double()()

        If TypeOf x Is Rdataframe Then
            With DirectCast(x, Rdataframe)
                matrix = .nrows _
                    .Sequence _
                    .Select(Function(i)
                                Return REnv.asVector(Of Double)(.getRowList(i, drop:=False))
                            End Function) _
                    .Select(Function(v) DirectCast(v, Double())) _
                    .ToArray
            End With
        Else
            Throw New NotImplementedException
        End If

        Dim PCA As New PCA(matrix, center, scale)

        Return PCA
    End Function

    <ExportAPI("as.dist")>
    Public Function asDist(x As Rdataframe, Optional item1$ = "A", Optional item2$ = "B", Optional correlation$ = "correlation") As DistanceMatrix
        Dim raw As EntityObject() = x.getRowNames _
            .Select(Function(id, index)
                        Return x.dataframeRow(Of String, EntityObject)(id, index)
                    End Function) _
            .ToArray

        Return Builder.FromTabular(raw, item1, item2, correlation)
    End Function

    <Extension>
    Private Function dataframeRow(Of T, DataSet As {New, INamedValue, DynamicPropertyBase(Of T)})(x As Rdataframe, id As String, index%) As DataSet
        Dim row As Dictionary(Of String, Object) = x.getRowList(index, drop:=True)
        Dim props As Dictionary(Of String, T) = row _
            .ToDictionary(Function(a) a.Key,
                            Function(a)
                                Return CType(REnv.single(a.Value), T)
                            End Function)

        Return New DataSet With {
            .Key = id,
            .Properties = props
        }
    End Function

    <ExportAPI("dist")>
    <RApiReturn(GetType(DistanceMatrix))>
    Public Function dist(<RRawVectorArgument> x As Object,
                         Optional method$ = "euclidean",
                         Optional diag As Boolean = False,
                         Optional upper As Boolean = False,
                         Optional p% = 2,
                         Optional env As Environment = Nothing) As Object

        Dim raw As DataSet()

        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is Rdataframe Then
            With DirectCast(x, Rdataframe)
                raw = .rownames _
                    .Select(Function(name, i)
                                Return .dataframeRow(Of Double, DataSet)(name, i)
                            End Function) _
                    .ToArray
            End With
        ElseIf TypeOf x Is DataSet() Then
            raw = x
        Else
            Return Internal.debug.stop(New InvalidCastException(x.GetType.FullName), env)
        End If

        Select Case Strings.LCase(method)
            Case "euclidean"
                Return raw.Euclidean
            Case Else
                Return Internal.debug.stop(New NotImplementedException(method), env)
        End Select
    End Function
End Module

Public Enum SplineAlgorithms
    BSpline
    CubiSpline
    CatmullRom
    Bezier
End Enum
