#Region "Microsoft.VisualBasic::46086fa661c605fc1edddcbb8444247d, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataset/LabelledVector.vb"

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

    '   Total Lines: 45
    '    Code Lines: 30
    ' Comment Lines: 8
    '   Blank Lines: 7
    '     File Size: 1.59 KB


    ' Class LabelledVector
    ' 
    '     Properties: UID, vector
    ' 
    '     Function: CreateDataFrame, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' the underlying data model for read ``MNIST-LabelledVectorArray-60000x100.msgpack``
''' </summary>
''' <remarks>
''' Note: The MNIST data here consist of normalized vectors (so the CosineForNormalizedVectors distance function can be safely used)
''' 
''' http://yann.lecun.com/exdb/mnist/
''' </remarks>
Public NotInheritable Class LabelledVector : Implements INamedValue

    <MessagePackMember(0)>
    Public Property UID As String Implements IKeyedEntity(Of String).Key
    <MessagePackMember(1)>
    Public Property vector As Single()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function CreateDataFrame(vector As LabelledVector()) As dataframe
        Dim size As Integer = vector(Scan0).vector.Length

        Dim data As New dataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = vector.Keys
        }

        For i As Integer = 0 To size - 1
#Disable Warning
            data.columns($"#{i + 1}") = vector _
                .Select(Function(v) v.vector(i)) _
                .ToArray
#Enable Warning
        Next

        Return data
    End Function
End Class
