Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' config of the commandline script parameters via ``config.json`` file.
''' </summary>
''' <remarks>
''' just looking for the ``config.json`` file under the current working directory.
''' </remarks>
Public Class ConfigJSON

    Dim config As list

    ''' <summary>
    ''' just looking for the ``config.json`` file under the current working directory.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function LoadConfig() As ConfigJSON
        Dim config As String = "./config.json"
        Dim json As JsonElement = JsonElement.ParseJSON(jsonStr:=config.ReadAllText)

        If Not TypeOf json Is JsonObject Then
            Return Nothing
        Else
            Return New ConfigJSON With {
                .config =
            }
        End If
    End Function

End Class
