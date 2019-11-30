Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
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
            Call Internal.invoke.add("writeLines", AddressOf file.writeLines)
            Call Internal.invoke.add("setwd", AddressOf file.setwd)
            Call Internal.invoke.add("normalize.filename", AddressOf file.normalizeFileName)
            Call Internal.invoke.add("save.image", AddressOf saveImage)
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Private Function saveImage(envir As Environment, params As Object()) As Boolean
            Dim image As Image = params(Scan0)
            Dim file As String = params(1)

            Return image.SaveAs(file)
        End Function

        Friend Function normalizeFileName(envir As Environment, params As Object()) As String()
            Return params.SafeQuery _
                .Select(Function(val)
                            Return Runtime.asVector(Of Double)(val) _
                                .AsObjectEnumerator _
                                .Select(Function(file)
                                            Return Scripting.ToString(file).NormalizePathString(False)
                                        End Function)
                        End Function) _
                .IteratesALL _
                .ToArray
        End Function

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

        ' writeLines(text, con = stdout(), sep = "\n", useBytes = FALSE)
        Friend Function writeLines(envir As Environment, params As Object()) As Object
            Dim text = Runtime.asVector(Of String)(params(Scan0))
            Dim con$ = Scripting.ToString(Runtime.getFirst(params(1)))

            If con.StringEmpty Then
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
                Call text.AsObjectEnumerator _
                    .JoinBy(vbCrLf) _
                    .SaveTo(con)
            End If

            Return text
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