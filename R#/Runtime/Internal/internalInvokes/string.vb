Namespace Runtime.Internal.Invokes

    Module stringr

        Sub New()
            Call Internal.invoke.add("string.replace", AddressOf stringr.replace)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Friend Function replace(envir As Environment, params As Object()) As String()
            Dim subj As String() = Runtime.asVector(Of String)(params(Scan0))
            Dim search As String = Scripting.ToString(Runtime.getFirst(params(1)))
            Dim replaceAs As String = Scripting.ToString(Runtime.getFirst(params(2)), "")
            Dim regexp As Boolean = Runtime.asLogical(params(3))(Scan0)

            If regexp Then
                Return subj.Select(Function(s) s.StringReplace(search, replaceAs)).ToArray
            Else
                Return subj.Select(Function(s) s.Replace(search, replaceAs)).ToArray
            End If
        End Function
    End Module
End Namespace