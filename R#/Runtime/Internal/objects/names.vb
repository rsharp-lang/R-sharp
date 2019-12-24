Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime.Internal

    Module names

        Public Function getNames([object] As Object, envir As Environment) As Object
            Dim type As Type = [object].GetType

            ' get names
            Select Case type
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).getNames
                Case GetType(vbObject)
                    Return DirectCast([object], vbObject).getNames
                Case Else
                    If type.IsArray Then
                        Dim objVec As Array = Runtime.asVector(Of Object)([object])

                        If objVec.AsObjectEnumerator.All(Function(o) o.GetType Is GetType(Group)) Then
                            Return objVec.AsObjectEnumerator _
                                .Select(Function(g)
                                            Return Scripting.ToString(DirectCast(g, Group).key, "NULL")
                                        End Function) _
                                .ToArray
                        End If
                    End If
                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        Public Function setNames([object] As Object, namelist As Array, envir As Environment) As Object
            ' set names
            Select Case [object].GetType
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).setNames(namelist, envir)
                Case Else
                    Return Internal.stop({"unsupported!", "func: names"}, envir)
            End Select
        End Function

        Public Function getRowNames() As Object

        End Function

        Public Function setRowNames() As Object

        End Function

        Public Function getColNames() As Object

        End Function

        Public Function setColNames() As Object

        End Function
    End Module
End Namespace