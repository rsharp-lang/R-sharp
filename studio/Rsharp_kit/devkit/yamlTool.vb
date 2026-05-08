#Region "Microsoft.VisualBasic::966445a215a70ecfee679a20cb5761f5, studio\Rsharp_kit\devkit\yamlTool.vb"

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

'   Total Lines: 26
'    Code Lines: 22 (84.62%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 4 (15.38%)
'     File Size: 914 B


' Module yamlTool
' 
'     Function: parse
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.text.yaml
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("yaml")>
Module yamlTool

    <ExportAPI("yaml.load_file")>
    Public Function load_file(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim is_file As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, IO.FileAccess.Read, env, is_filepath:=is_file)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        Dim doc As Object = parse(s.TryCast(Of Stream).ReadAllLines.JoinBy(vbLf), env)

        If is_file Then
            Try
                Call s.TryCast(Of Stream).Dispose()
            Catch ex As Exception
            End Try
        End If

        Return doc
    End Function

    <ExportAPI("yaml.parse")>
    Public Function parse(yaml As String, Optional env As Environment = Nothing) As Object
        Dim doc As JsonElement = New YamlParser().Parse(yaml)
        Dim robj As Object = doc.createRObj(env)
        Return robj
    End Function
End Module
