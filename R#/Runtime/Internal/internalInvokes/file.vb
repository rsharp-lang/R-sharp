Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
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
            Call Internal.invoke.add("setwd", AddressOf file.setwd)
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
                                Return Scripting _
                                    .ToString(Runtime.getFirst(val)) _
                                    .DoCall(AddressOf FileExists)
                            End If
                        End Function) _
                .ToArray
        End Function

        Friend Function readLines(envir As Environment, params As Object()) As String()
            Return Scripting.ToString(Runtime.getFirst(params(Scan0))).ReadAllLines
        End Function

        Friend Function setwd(envir As Environment, paramVals As Object()) As Object
            Dim dir As String() = Runtime.asVector(Of String)(paramVals(Scan0))

            If dir.Length = 0 Then
                Return invoke.missingParameter(NameOf(setwd), "dir", envir)
            ElseIf dir(Scan0).StringEmpty Then
                Return invoke.invalidParameter("cannot change working directory due to the reason of NULL value provided!", NameOf(setwd), "dir", envir)
            Else
                App.CurrentDirectory = PathMapper.GetMapPath(dir(Scan0))
            End If

            Return App.CurrentDirectory
        End Function
    End Module
End Namespace