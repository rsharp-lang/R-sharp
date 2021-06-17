#Region "Microsoft.VisualBasic::8e077869bf8050a6481cb3fb442712d5, Library\JsonHelper\RJSON.vb"

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

    ' Module RJSON
    ' 
    '     Function: createRObj
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module RJSON

    <Extension>
    Friend Function createRObj(json As JsonElement, env As Environment) As Object
        If TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).GetStripString
        ElseIf TypeOf json Is JsonArray Then
            Dim array As JsonArray = DirectCast(json, JsonArray)

            If array.All(Function(a) TypeOf a Is JsonValue) Then
                Return array _
                    .Select(Function(a) a.createRObj(env)) _
                    .ToArray
            Else
                Dim list As New List With {.slots = New Dictionary(Of String, Object)}
                Dim i As i32 = 1

                For Each item As JsonElement In array
                    list.slots.Add($"[[{++i}]]", item.createRObj(env))
                Next

                Return list
            End If
        ElseIf TypeOf json Is JsonObject Then
            Dim list As New List With {
                .slots = New Dictionary(Of String, Object)
            }

            For Each item As NamedValue(Of JsonElement) In DirectCast(json, JsonObject)
                Call list.slots.Add(item.Name, item.Value.createRObj(env))
            Next

            Return list
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(JsonElement), json.GetType, env), env)
        End If
    End Function
End Module
