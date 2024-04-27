#Region "Microsoft.VisualBasic::308214b41345c8d67703b5fe4efc9201, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataset/UnionMatrix.vb"

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

    '   Total Lines: 93
    '    Code Lines: 61
    ' Comment Lines: 19
    '   Blank Lines: 13
    '     File Size: 3.31 KB


    ' Class UnionMatrix
    ' 
    '     Function: colnames, CreateClrMatrix, CreateMatrix
    ' 
    '     Sub: Add
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DATA
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' A matrix for the sparse numeric (<see cref="Double"/>) data.
''' </summary>
Public Class UnionMatrix : Implements MatrixProvider

    ReadOnly records As New List(Of NamedValue(Of list))

    ''' <summary>
    ''' Add a sample into the current data matrix
    ''' </summary>
    ''' <param name="recordName">the sample id or the row name</param>
    ''' <param name="data">
    ''' the [key=>value] tuple data list, row data in the matrix
    ''' </param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(recordName As String, data As list)
        records.Add(New NamedValue(Of list)(recordName, data))
    End Sub

    Private Function colnames() As IEnumerable(Of String)
        Return records _
            .Select(Function(v) v.Value.getNames) _
            .IteratesALL _
            .ToArray _
            .DoCall(AddressOf CLRVector.asCharacter) _
            .Distinct
    End Function

    ''' <summary>
    ''' clr dataset collection by rows
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function CreateClrMatrix() As IEnumerable(Of DataSet) Implements MatrixProvider.GetMatrix
        Dim allFeatures As String() = colnames.ToArray

        For Each row As NamedValue(Of list) In records
            Dim x As list = row.Value
            Dim v As Object() = allFeatures _
                .Select(Function(i) If(x.hasName(i), x.slots(i), 0.0)) _
                .ToArray
            Dim vec As Double() = CLRVector.asNumeric(v)
            Dim data As New Dictionary(Of String, Double)

            For i As Integer = 0 To allFeatures.Length - 1
                Call data.Add(allFeatures(i), vec(i))
            Next

            Yield New DataSet With {
                .ID = row.Name,
                .Properties = data
            }
        Next
    End Function

    ''' <summary>
    ''' R dataframe object by column
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateMatrix() As Rdataframe
        Dim allFeatures As String() = colnames.ToArray
        Dim rownames As String() = records.Select(Function(a) a.Name).uniqueNames
        Dim matrix As New Dictionary(Of String, Array)

        For Each name As String In allFeatures
            Dim v As Object() = records _
                .Select(Function(a)
                            Return If(a.Value.hasName(name), REnv.single(a.Value.getByName(name)), 0.0)
                        End Function) _
                .ToArray

            Call matrix.Add(name, CLRVector.asNumeric(v))
        Next

        Return New Rdataframe With {
            .rownames = rownames,
            .columns = matrix
        }
    End Function

End Class
