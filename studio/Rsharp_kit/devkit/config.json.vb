#Region "Microsoft.VisualBasic::3eb9df6149b05fc4da44ffbd9c3b92cb, studio\Rsharp_kit\devkit\config.json.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Class ConfigJSON
    ' 
    '     Function: BuildTemplate, GetArgumentValue, GetConfigJSONFilePath, getListConfig, LoadConfig
    ' 
    '     Sub: SetCommandLine
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Development.CommandLine
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

    Public Shared Function BuildTemplate(Rscript As ShellScript) As ConfigJSON
        Dim template As New list With {
            .slots = New Dictionary(Of String, Object)
        }

        For Each arg In Rscript.argumentList
            Dim argument As ArgumentInfo = arg.Value

            If Not argument.hasAttribute("config") Then
                Continue For
            End If

            Dim config As String = argument!config
            Dim tokens As String() = config.Trim("/"c).Split("/"c, "\"c)
            Dim endPoint As list = template
            Dim defaultValue As CommandLineArgument = Rscript.GetCommandArgument(arg.Key)

            For Each section As String In tokens.Take(tokens.Length - 1)
                If Not endPoint.hasName(section) Then
                    endPoint.add(section, New list With {.slots = New Dictionary(Of String, Object)})
                End If

                endPoint = DirectCast(endPoint(section), list)
            Next

            endPoint.add(tokens.Last, If(defaultValue Is Nothing, "NULL", If(defaultValue.isLiteral, defaultValue.defaultValue.Trim(""""c), "NULL")))
        Next

        Return New ConfigJSON With {.config = template}
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

