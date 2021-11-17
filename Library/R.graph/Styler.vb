Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("styler")>
Module Styler

    ''' <summary>
    ''' set or get node size data
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <ExportAPI("size")>
    <RApiReturn(GetType(V))>
    Public Function size(v As V,
                         <RRawVectorArgument>
                         Optional val As Object = Nothing,
                         Optional env As Environment = Nothing) As Object

        Dim valType As RType = RType.TypeOf(val)

        If val Is Nothing Then
            Return New list(RType.GetRSharpType(GetType(Double))) With {
               .slots = v.vertex _
                   .ToDictionary(Function(d) d.label,
                                 Function(d)
                                     Return CObj(d.data.size.ElementAtOrDefault(0))
                                 End Function)
            }
        ElseIf valType.mode = TypeCodes.double OrElse valType.mode = TypeCodes.integer Then
            Dim vec As Double() = REnv.asVector(Of Double)(val)

            If vec.Length = 1 Then
                ' set unify size
                Dim sz As Double = vec(Scan0)

                For Each vex As Node In v.vertex
                    vex.data.size = {sz}
                Next
            Else
                ' set size one by one
                If vec.Length <> v.size Then
                    Return Internal.debug.stop($"the size of the data vector is not equals to the size of the vertex list!", env)
                Else
                    For i As Integer = 0 To vec.Length - 1
                        v.vertex(i).data.size = {vec(i)}
                    Next
                End If
            End If
        ElseIf valType Is RType.list Then
            For Each attr In DirectCast(val, list).slots
                Dim vex As Node = v.getByName(attr.Key)

                If Not vex Is Nothing Then
                    vex.data.size = REnv.asVector(Of Double)(attr.Value)
                End If
            Next
        End If

        Return v
    End Function
End Module
