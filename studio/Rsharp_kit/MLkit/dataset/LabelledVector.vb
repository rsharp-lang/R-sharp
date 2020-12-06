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

    Public Shared Function CreateDataFrame(vector As LabelledVector(), Optional takes As Integer = -1) As dataframe
        Dim size As Integer = vector(Scan0).vector.Length

        If takes > 0 Then
            vector = vector.Take(takes).ToArray
        End If

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
