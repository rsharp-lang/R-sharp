Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine

    Module dataframeValueAssign

        Public Function ValueAssign(symbolIndex As SymbolIndexer, indexStr As String(), targetObj As dataframe, value As Object, env As Environment) As Message
            If symbolIndex.indexType = SymbolIndexers.dataframeColumns Then
                If indexStr.Length = 1 Then
                    If value Is Nothing Then
                        ' removes column
                        targetObj.columns.Remove(indexStr(Scan0))
                    Else
                        If Not value.GetType.IsArray Then
                            Dim a As Array = Array.CreateInstance(value.GetType, 1)
                            a.SetValue(value, Scan0)
                            value = a
                        End If

                        targetObj.columns(indexStr(Scan0)) = value
                    End If
                Else
                    Dim seqVal As Array = Runtime.asVector(Of Object)(value)
                    Dim i As i32 = Scan0

                    For Each key As String In indexStr
                        If seqVal.Length = 1 Then
                            targetObj.columns(key) = value
                        Else
                            targetObj.columns(key) = seqVal.GetValue(++i)
                        End If
                    Next
                End If

                Return Nothing
            Else
                Return Internal.debug.stop(New NotImplementedException, env)
            End If
        End Function
    End Module
End Namespace