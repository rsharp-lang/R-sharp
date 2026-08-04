Namespace Runtime.Internal.Object

    Public Class CLRIterator : Inherits pipeline

        Public Sub New(input As IEnumerable, type As Type)
            MyBase.New(input, type)
        End Sub

        Public Overrides Iterator Function populates(Of T)(env As Environment) As IEnumerable(Of T)
            For Each item As T In DirectCast(pipeline, IEnumerable(Of T))
                Yield item
            Next

            If Not pipeFinalize Is Nothing Then
                Call pipeFinalize()()
            End If
        End Function

        Public Overrides Function ToString() As String
            If pipeline.GetType.IsArray Then
                Return $"clr_array[{elementType.ToString} x {DirectCast(pipeline, Array).Length}]"
            Else
                Return $"clr_iterator[{elementType.ToString}]"
            End If
        End Function

        Public Shared Function Enumerates(Of T)(x As Object, env As Environment) As IEnumerable(Of T)
            If TypeOf x Is vector Then
                Return CLRIterator.fromVector(Of T)(x, env).populates(Of T)(env)
            ElseIf TypeOf x Is T() Then
                Return DirectCast(x, T())
            Else
                Return CLRIterator.TryCreatePipeline(Of T)(x, env).populates(Of T)(env)
            End If
        End Function
    End Class
End Namespace