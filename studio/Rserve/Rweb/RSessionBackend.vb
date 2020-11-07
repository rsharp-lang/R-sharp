Imports System.IO
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.System.Configuration
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Public Class RSessionBackend

    ReadOnly R As RInterpreter
    ReadOnly inspector As New Dictionary(Of String, Byte())

    Public Sub New(Optional workspace$ = "./", Optional showError As Boolean = True)
        Me.R = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Me.R.silent = True
        Me.R.redirectError2stdout = showError

        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(packageName:=pkgName, silent:=True)
        Next

        With $"{workspace}/.RData"
            If .FileExists(True) Then

            End If
        End With
    End Sub

    Public Sub InspectObject(guid$, output As HttpResponse)
        SyncLock inspector
            If inspector.ContainsKey(guid) Then
                Dim buffer As Byte() = inspector(guid)

                output.WriteHeader("text/json", buffer.Length)
                output.Write(buffer)
                output.Flush()

                inspector.Remove(guid)
            Else
                output.WriteError(404, "object not found!")
            End If
        End SyncLock
    End Sub

    Private Function handleRScript(scriptText$, Rstd_out As StreamWriter) As RInvoke
        Dim invokeRtvl As New RInvoke
        Dim result As Object

        result = R.RedirectOutput(Rstd_out, OutputEnvironments.Html).Evaluate(scriptText)

        If RProgram.isException(result) Then
            invokeRtvl.code = 500
            invokeRtvl.err = result
        Else
            invokeRtvl.code = 0

            If Not result Is Nothing Then
                ' 在终端显示最后的结果值
                R.Evaluate($"print({RInterpreter.lastVariableName});")
            End If
        End If

        If R.globalEnvir.stdout.recommendType Is Nothing Then
            invokeRtvl.content_type = "text/html"
        Else
            invokeRtvl.content_type = R.globalEnvir.stdout.recommendType
        End If

        Call Rstd_out.Flush()

        Return invokeRtvl
    End Function

    Private Shared Function requiredDataURI(result As RInvoke) As Boolean
        Static exclude_types As String() = {"text/html", "text/json", "text/xml", "text/csv", "application/json"}

        SyncLock exclude_types
            Return Not exclude_types _
                .Any(Function(name)
                         Return result.content_type.StartsWith(name)
                     End Function)
        End SyncLock
    End Function

    Public Sub RunCode(scriptText As String, response As HttpResponse)
        Dim result As RInvoke

        Static inspector_types As Index(Of String) = {"inspector/json", "inspector/csv", "inspector/api"}

        Using output As New MemoryStream(), Rstd_out As New StreamWriter(output, Encodings.UTF8WithoutBOM.CodePage)
            result = handleRScript(scriptText, Rstd_out)

            If result.content_type Like inspector_types Then
                Dim guid As String = App.NextTempName

                inspector.Add(guid, output.ToArray)
                result.info = guid
            Else
                ' 后端的输出应该包含有两部分的内容
                ' 终端输出的文本
                ' 以及最后的值
                If requiredDataURI(result) Then
                    result.info = New DataURI(base64:=output.ToArray.ToBase64String, mime:=result.content_type).ToString
                Else
                    result.info = output.ToArray.ToBase64String
                End If
            End If

            result.warnings = R.globalEnvir.messages.PopAll
        End Using

        Dim json As String = JSONSerializer.GetJson(result, enumToStr:=True)
        Dim buffer As Byte() = Encodings.UTF8WithoutBOM.CodePage.GetBytes(json)

        response.AccessControlAllowOrigin = "*"

        Call response.WriteHeader("application/json", buffer.Length)
        Call response.Write(buffer)
    End Sub
End Class
