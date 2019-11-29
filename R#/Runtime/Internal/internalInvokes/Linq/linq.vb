Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module linq

        Sub New()
            Internal.invoke.add("groupBy", AddressOf linq.groupBy)
            Call Internal.invoke.add("first", AddressOf linq.first)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Private Function first(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim test As RFunction = params(1)
            Dim pass As Boolean
            Dim arg As InvokeParameter

            For Each item As Object In sequence
                arg = New InvokeParameter() With {
                    .value = New RuntimeValueLiteral(item)
                }
                pass = Runtime.asLogical(test.Invoke(envir, {arg}))(Scan0)

                If pass Then
                    Return item
                End If
            Next

            Return Nothing
        End Function

        Private Function groupBy(envir As Environment, params As Object()) As Object
            Dim sequence As Array = Runtime.asVector(Of Object)(params(Scan0))
            Dim getKey As RFunction = params(1)
            Dim result = sequence.AsObjectEnumerator _
                .GroupBy(Function(o)
                             Dim arg As New InvokeParameter() With {
                                .value = New RuntimeValueLiteral(o)
                             }

                             Return getKey.Invoke(envir, {arg})
                         End Function) _
                .Select(Function(group)
                            Return New Group With {
                                .key = group.Key,
                                .group = group.ToArray
                            }
                        End Function) _
                .ToArray

            Return result
        End Function
    End Module
End Namespace