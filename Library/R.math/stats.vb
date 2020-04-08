#Region "Microsoft.VisualBasic::224bf7ecf7b281c8e3c321ac9a1b8803, Library\R.math\stats.vb"

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
'     Function: spline, tabulateMode
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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("stats")>
Module stats

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
    End Function
End Module

Public Enum SplineAlgorithms
    BSpline
    CubiSpline
    CatmullRom
    Bezier
End Enum
