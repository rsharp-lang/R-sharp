#Region "Microsoft.VisualBasic::c1e9ba3e294f0eee5a1566ff0bfefc9b, R-sharp\Library\JsonHelper\RJSON.vb"

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

    '   Total Lines: 89
    '    Code Lines: 60
    ' Comment Lines: 15
    '   Blank Lines: 14
    '     File Size: 3.17 KB


    ' Module RJSON
    ' 
    '     Function: createRObj, SchemaIdentical
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="strictSchemaCheck">
    ''' 是否对json对象类型进行严格的类型检查
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Private Function SchemaIdentical(array As JsonArray, strictSchemaCheck As Boolean) As Boolean
        If array.Length <= 1 Then
            Return True
        End If

        Dim ref As JsonElement = array(Scan0)

        If Not array.All(Function(i) i.GetType Is ref.GetType) Then
            Return False
        End If

        If TypeOf ref Is JsonObject AndAlso strictSchemaCheck Then
            Dim keys As String() = DirectCast(ref, JsonObject).ObjectKeys

            If Not array.All(Function(i) keys.All(AddressOf DirectCast(i, JsonObject).HasObjectKey)) Then
                Return False
            End If
        End If

        Return True
    End Function

    ''' <summary>
    ''' convert json object to R object
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function createRObj(json As JsonElement,
                               env As Environment,
                               Optional strictSchemaCheck As Boolean = False) As Object

        If TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).GetStripString
        ElseIf TypeOf json Is JsonArray Then
            Dim array As JsonArray = DirectCast(json, JsonArray)

            If array.All(Function(a) TypeOf a Is JsonValue) OrElse array.SchemaIdentical(strictSchemaCheck) Then
                ' is a element vector
                Return array _
                    .Select(Function(a) a.createRObj(env, strictSchemaCheck)) _
                    .ToArray
            Else
                Dim list As New list With {
                    .slots = New Dictionary(Of String, Object)
                }
                Dim i As i32 = 1

                For Each item As JsonElement In array
                    list.slots.Add(++i, item.createRObj(env, strictSchemaCheck))
                Next

                Return list
            End If
        ElseIf TypeOf json Is JsonObject Then
            Dim list As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            For Each item As NamedValue(Of JsonElement) In DirectCast(json, JsonObject)
                Call list.slots.Add(item.Name, item.Value.createRObj(env, strictSchemaCheck))
            Next

            Return list
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(JsonElement), json.GetType, env), env)
        End If
    End Function
End Module
