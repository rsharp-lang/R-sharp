#Region "Microsoft.VisualBasic::04e2a44d260d7b6756967bd2cc54cf62, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/dataset/matrix.vb"

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

    '   Total Lines: 65
    '    Code Lines: 39
    ' Comment Lines: 14
    '   Blank Lines: 12
    '     File Size: 2.13 KB


    '     Class matrix
    ' 
    '         Properties: colnames, mat, rownames
    ' 
    '         Function: getColumn, getNames, getRow, getRowNames, hasName
    '                   setNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' all element inside the matrix has the sample data type
    ''' </summary>
    ''' <remarks>
    ''' kind of liked <see cref="dataframe"/> type
    ''' </remarks>
    Public Class matrix : Inherits RsharpDataObject
        Implements RNames
        Implements IdataframeReader

        ''' <summary>
        ''' a rectangle array
        ''' </summary>
        ''' <returns></returns>
        Public Property mat As Array()

        Dim cols As Index(Of String)

        Public Property colnames As String()
            Get
                Return cols.Objects
            End Get
            Set(value As String())
                cols = value.Indexing
            End Set
        End Property

        Public Property rownames As String()

        Public Function getColumn(index As Object, env As Environment) As Object Implements IdataframeReader.getColumn
            Throw New NotImplementedException()
        End Function

        Public Function getRow(index As Object, env As Environment) As Object Implements IdataframeReader.getRow
            Throw New NotImplementedException()
        End Function

        Public Function getRowNames() As String() Implements IdataframeReader.getRowNames
            Return rownames
        End Function

        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            colnames = names
            Return names.ToArray
        End Function

        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return name Like cols
        End Function

        ''' <summary>
        ''' get the matrix col names
        ''' </summary>
        ''' <returns></returns>
        Public Function getNames() As String() Implements IReflector.getNames
            Return cols.Objects
        End Function
    End Class
End Namespace
