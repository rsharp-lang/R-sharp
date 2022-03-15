#Region "Microsoft.VisualBasic::ca2bee3582203a0928090fb5afc8e9ec, R-sharp\Library\R.base\base\IO.vb"

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

    '   Total Lines: 161
    '    Code Lines: 125
    ' Comment Lines: 20
    '   Blank Lines: 16
    '     File Size: 6.53 KB


    ' Module RawIO
    ' 
    '     Function: dumpDf, dumpJSON, dumpXml, fromPipeline
    ' 
    ' Enum sourceTypes
    ' 
    '     CSV, JSON, TSV, XML
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# raw I/O api module
''' </summary>
<Package("IO", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Module RawIO

    ''' <summary>
    ''' Dumping a large json/xml dataset into a data table file
    ''' </summary>
    ''' <param name="source">the source data object</param>
    ''' <param name="type">
    ''' the data format of the string content in source data object.
    ''' </param>
    ''' <param name="projection">
    ''' data fields in the content object from the source will be dump 
    ''' to the data table object.</param>
    ''' <param name="trim">
    ''' some fields needs removes the additional blank space and new 
    ''' line symbol. the field names in this parameter should be contains 
    ''' in projection field list.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("dumpDf")>
    Public Function dumpDf(<RRawVectorArgument>
                           source As Object, projection As String(),
                           Optional type As sourceTypes = sourceTypes.JSON,
                           Optional trim As String() = Nothing,
                           Optional filter As RFunction = Nothing,
                           Optional env As Environment = Nothing) As pipeline

        Dim data As IEnumerable(Of String())
        Dim fieldIndex As Index(Of String) = projection.Indexing
        Dim trimIndex As Integer() = trim _
            .SafeQuery _
            .Select(Function(name)
                        Return fieldIndex.IndexOf(name)
                    End Function) _
            .ToArray

        If trimIndex.Any(Function(i) -1 = i) Then
            Call New String() {
                $"there are some fields that required to strip characters is missing from the given source field names!",
                $"missing: " & which(trimIndex.Select(Function(i) -1 = i)) _
                    .Select(Function(i)
                                Return trim(i)
                            End Function) _
                    .JoinBy(", ")
            }.DoCall(AddressOf env.AddMessage)
        End If

        If TypeOf source Is pipeline Then
            data = DirectCast(source, pipeline).fromPipeline(projection, type, env)
        Else
            If TypeOf source Is vector Then
                source = DirectCast(source, vector).data
            End If

            If source.GetType.IsArray Then
                data = REnv _
                    .asVector(Of String)(DirectCast(source, Array)) _
                    .DoCall(Function(a)
                                Return pipeline.CreateFromPopulator(DirectCast(a, String()))
                            End Function) _
                    .fromPipeline(projection, type, env)
            ElseIf TypeOf source Is Stream Then
                data = Iterator Function() As IEnumerable(Of String)
                           Using reader As New StreamReader(DirectCast(source, Stream))
                               Yield reader.ReadLine
                           End Using
                       End Function() _
                    .DoCall(AddressOf pipeline.CreateFromPopulator) _
                    .fromPipeline(projection, type, env)
            Else
                Return Internal.debug.stop(New NotImplementedException(source.GetType.FullName), env)
            End If
        End If

        Dim filterFunc As Func(Of String(), Boolean) = Nothing

        If Not filter Is Nothing Then
            filterFunc =
                Function(row)

                End Function
        End If

        Return New String() {
            New RowObject(projection).AsLine
        } _
            .JoinIterates(b:=data _
                .Where(Function(ls)
                           If filterFunc Is Nothing Then
                               Return True
                           Else
                               Return filterFunc(ls)
                           End If
                       End Function) _
                .Select(Function(ls)
                            For Each i As Integer In trimIndex
                                ls(i) = ls(i).TrimNewLine.Trim
                            Next

                            Return RowObject.ToString(ls)
                        End Function)) _
            .DoCall(AddressOf pipeline.CreateFromPopulator)
    End Function

    <Extension>
    Private Function fromPipeline(source As pipeline, projection As String(), type As sourceTypes, env As Environment) As IEnumerable(Of String())
        Select Case type
            Case sourceTypes.JSON
                Return DirectCast(source, pipeline).populates(Of String)(env).dumpJSON(projection)
            Case sourceTypes.XML
                Return DirectCast(source, pipeline).populates(Of String)(env).dumpXml(projection)
            Case Else
                Return Internal.debug.stop(New NotImplementedException(type), env)
        End Select
    End Function

    <Extension>
    Private Iterator Function dumpJSON(text As IEnumerable(Of String), projection As String()) As IEnumerable(Of String())
        For Each line As String In text
            Dim json As JsonObject = New JsonParser().OpenJSON(line)
            Dim vals As String() = projection _
                .Select(Function(name)
                            Return json(name).AsString
                        End Function) _
                .ToArray

            Yield vals
        Next
    End Function

    <Extension>
    Private Iterator Function dumpXml(text As IEnumerable(Of String), projection As String()) As IEnumerable(Of String())
        Throw New NotImplementedException
    End Function
End Module

Public Enum sourceTypes
    XML
    JSON
    CSV
    TSV
End Enum
