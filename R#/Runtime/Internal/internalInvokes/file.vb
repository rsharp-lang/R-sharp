Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' ## File Manipulation
    ''' 
    ''' These functions provide a low-level interface to the computer's file system.
    ''' </summary>
    Module file

        Sub New()
            Call Internal.invoke.add("file.exists", AddressOf file.exists)
            Call Internal.invoke.add("readLines", AddressOf file.readLines)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Friend Function exists(envir As Environment, params As Object()) As Boolean()
            Return params.SafeQuery _
                .Select(Function(val)
                            If val Is Nothing Then
                                Return False
                            Else
                                Return FileExists(Scripting.ToString(val))
                            End If
                        End Function) _
                .ToArray
        End Function

        Friend Function readLines(envir As Environment, params As Object()) As String()
            Return Scripting.ToString(params(Scan0)).ReadAllLines
        End Function
    End Module
End Namespace