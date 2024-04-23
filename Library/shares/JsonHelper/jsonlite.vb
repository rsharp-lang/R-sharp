#Region "Microsoft.VisualBasic::c50dc25fdd4dabde9390602685a37c95, R-sharp\Library\JsonHelper\jsonlite.vb"

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


' Code Statistics:

'   Total Lines: 58
'    Code Lines: 48
' Comment Lines: 0
'   Blank Lines: 10
'     File Size: 1.88 KB


' Module jsonlite
' 
'     Function: toJSON
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Module jsonlite

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <param name="maskReadonly"></param>
    ''' <param name="indent"></param>
    ''' <param name="enumToStr"></param>
    ''' <param name="unixTimestamp"></param>
    ''' <returns>
    ''' the generated json string or error <see cref="Message"/>.
    ''' </returns>
    Public Function toJSON(x As Object, env As Environment,
                           Optional maskReadonly As Boolean = False,
                           Optional indent As Boolean = False,
                           Optional enumToStr As Boolean = True,
                           Optional unixTimestamp As Boolean = True,
                           Optional args As list = Nothing) As Object

        Dim opts As New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }
        Dim encoder As Encoder = Encoder.CreateEncoderWithOptions(args, env)

        If x Is Nothing Then
            Return "null"
        ElseIf TypeOf x Is Dictionary(Of String, String) Then
            Return DirectCast(x, Dictionary(Of String, String)).GetJson
        ElseIf TypeOf x Is Dictionary(Of String, Double) Then
            Return DirectCast(x, Dictionary(Of String, Double)).GetJson
        End If

        Dim err As Message = Nothing
        Dim json As JsonElement = opts.GetJsonLiteralRaw(x, encoder, err, env)

        If Not err Is Nothing Then
            Return err
        End If

        Dim jsonStr As String = json.BuildJsonString(opts)

        Return jsonStr
    End Function

    ''' <summary>
    ''' cast R#/clr object as json element by reflection
    ''' </summary>
    ''' <param name="opts"></param>
    ''' <param name="x"></param>
    ''' <param name="err"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetJsonLiteralRaw(opts As JSONSerializerOptions,
                                      x As Object,
                                      encoder As Encoder,
                                      ByRef err As Message,
                                      env As Environment) As JsonElement
        Dim json As JsonElement

        If TypeOf x Is vector Then
            x = DirectCast(x, vector).data
        End If

        If x.GetType.IsArray Then
            If DirectCast(x, Array).Length = 1 Then
                x = DirectCast(x, Array).GetValue(Scan0)
            ElseIf DirectCast(x, Array).Length = 0 Then
                Return New JsonArray(New JsonElement() {})
            Else
                x = REnv.TryCastGenericArray(DirectCast(x, Array), env)
            End If
        End If

        If SMRUCC.Rsharp.Interpreter.Program.isException(x) Then
            err = x
            Return Nothing
        End If

        If Not TypeOf x Is JsonElement Then
            x = encoder.GetObject(x)
            json = x.GetType.GetJsonElement(x, opts)
        Else
            json = DirectCast(x, JsonElement)
        End If

        Return json
    End Function
End Module
