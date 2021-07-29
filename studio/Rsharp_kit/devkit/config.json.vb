Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' config of the commandline script parameters via ``config.json`` file.
''' </summary>
''' <remarks>
''' just looking for the ``config.json`` file under the current working directory.
''' </remarks>
Public Class ConfigJSON

    Dim config As list

    Public Function getListConfig() As list
        Return config
    End Function

    Public Function GetArgumentValue(configKey As String) As Object
        Dim path As String() = configKey.Trim("/"c).Split("/"c)
        Dim value As Object = Nothing
        Dim config As list = Me.config

        For Each name As String In path
            value = config.getByName(name)

            If Not TypeOf value Is list Then
                Exit For
            Else
                config = DirectCast(value, list)
            End If
        Next

        Return value
    End Function

    Public Sub SetCommandLine(env As Environment)
        ArgumentValue.SetArgumentHandler(AddressOf GetArgumentValue)
    End Sub

    Public Shared Function GetConfigJSONFilePath(env As Environment) As String
        Dim config As String = "./config.json".GetFullPath

        If Not config.FileExists Then
            Dim fromCommandLineArgument As String = App.CommandLine("--config")

            If Not fromCommandLineArgument.StringEmpty Then
                config = fromCommandLineArgument
            End If
        End If

        Return config
    End Function

    ''' <summary>
    ''' just looking for the ``config.json`` file under the current working directory.
    ''' or the config.json file path can be specific by the commandline parameter
    ''' ``--config``
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function LoadConfig(env As Environment) As ConfigJSON
        Dim config As String = GetConfigJSONFilePath(env)

        If config.FileLength < 0 Then
            Call env.AddMessage($"CommandLine argument file '{config}' is missing...", MSG_TYPES.WRN)
            Return Nothing
        End If

        Dim json As JsonElement = JsonElement.ParseJSON(jsonStr:=config.ReadAllText)

        If Not TypeOf json Is JsonObject Then
            Return Nothing
        Else
            Return New ConfigJSON With {
                .config = json.createRObj(env)
            }
        End If
    End Function

End Class
