Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module dev

        ''' <summary>
        ''' Load R script in directory
        ''' 
        ''' Load all of the R script in a given working directory,
        ''' by default is load all script in current directory.
        ''' </summary>
        ''' <param name="dir">The script source directory, by default is current workspace.</param>
        <ExportAPI("flash_load")>
        Public Sub flash_load(<RDefaultExpression> Optional dir As String = "~getwd()", Optional env As GlobalEnvironment = Nothing)
            Dim Rlist As String() = dir _
                .EnumerateFiles("*.r", "*.R") _
                .Select(Function(path) path.GetFullPath) _
                .Distinct _
                .ToArray

            For Each script As String In Rlist
                Try
                    Dim err As Object = env.Rscript.Source(script)

                    If Program.isException(err) Then
                        Call debug.PrintMessageInternal(DirectCast(err, Message), env)
                    End If
                Catch ex As Exception
                    Call base.print($"Error while loading script: {script}", env)
                    Call App.LogException(ex)
                    Call base.print(ex, env)
                End Try
            Next

            Dim zzz As String = $"{dir}/zzz.R"

            If zzz.FileExists Then
                Call env.doCall(".onLoad")
            End If
        End Sub
    End Module
End Namespace