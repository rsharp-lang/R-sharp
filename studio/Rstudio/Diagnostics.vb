#Region "Microsoft.VisualBasic::6c46890bf3705346aaa323014fac46f0, studio\Rstudio\Diagnostics.vb"

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

' Module Diagnostics
' 
'     Function: help
' 
'     Sub: view
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("diagnostics")>
Module Diagnostics

    <ExportAPI("view")>
    Public Sub view(<RRawVectorArgument> symbol As Object, Optional env As Environment = Nothing)
        Dim buffer = env.globalEnvironment.stdout

        If symbol Is Nothing Then
            buffer.Write("null", "inspector/json")
        ElseIf TypeOf symbol Is dataframe Then
            buffer.Write(
                data:=DirectCast(symbol, dataframe),
                globalEnv:=env.globalEnvironment,
                content_type:="inspector/csv"
            )
        Else
            Dim digest As New Dictionary(Of Type, Func(Of Object, Object))

            digest.Add(GetType(list), Function(obj) DirectCast(obj, list).slots)
            digest.Add(GetType(vector), Function(obj) DirectCast(obj, vector).data)
            digest.Add(GetType(vbObject), Function(obj) DirectCast(obj, vbObject).target)

            Dim opts As New JSONSerializerOptions With {.digest = digest}
            Dim json$ = JSONSerializer.GetJson(symbol.GetType(), symbol, opts)

            Call buffer.Write(json, "inspector/json")
        End If
    End Sub

    <ExportAPI("help")>
    Public Function help(symbol As Object, Optional env As Environment = Nothing) As Message
        If TypeOf symbol Is String Then
            symbol = env.FindSymbol(symbol)?.value
        End If

        If symbol Is Nothing Then
            Return debug.stop("symbol object can not be nothing!", env)
        ElseIf Not TypeOf symbol Is RMethodInfo Then
            Return debug.stop("unsupport symbol object type!", env)
        End If

        Dim markdownText As String

        Using buffer As New MemoryStream
            Dim markdown As New RContentOutput(New StreamWriter(buffer), OutputEnvironments.Html)

            Call env.globalEnvironment _
               .packages _
               .packageDocs _
               .PrintHelp(DirectCast(symbol, RMethodInfo), markdown)
            Call markdown.Flush()

            markdownText = Encoding.UTF8.GetString(buffer.ToArray)
        End Using

        Dim html As String = New MarkdownHTML().Transform(markdownText)

        Call env.globalEnvironment.stdout.Write(html, "inspector/api")

        Return Nothing
    End Function
End Module
