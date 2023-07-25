#Region "Microsoft.VisualBasic::cdd9e6923c1b9f8d90e425d9f03991b9, D:/GCModeller/src/R-sharp/Library/base//base/netCDFutils.vb"

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

    '   Total Lines: 241
    '    Code Lines: 179
    ' Comment Lines: 28
    '   Blank Lines: 34
    '     File Size: 9.64 KB


    ' Module netCDFutils
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: attributeData, dataframe, dimensions, getAttributes, getDataVariable
    '               getValue, globalAttributes, openCDF, printVar, variableNames
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' netCDF toolkit
''' </summary>
<Package("netCDF.utils")>
Module netCDFutils

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of variable)(AddressOf printVar)
    End Sub

    Private Function printVar(var As variable) As String
        Dim sb As New StringBuilder

        sb.AppendLine($"{var.ToString}")
        sb.AppendLine($"-----------------------------------")
        sb.AppendLine($"dimensions: {var.dimensions.GetJson}")
        sb.AppendLine($"      type: {var.type.Description}")
        sb.AppendLine($"      size: {var.size}")
        sb.AppendLine($"    offset: {var.offset}")
        sb.AppendLine($" is_record: {var.record.ToString.ToUpper}")

        If Not var.value Is Nothing Then
            Call sb.AppendLine(New String("-"c, 64))
            Call sb.AppendLine(var.value.ToString)
        End If

        Return sb.ToString
    End Function

    ''' <summary>
    ''' open a netCDF data file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("open.netCDF")>
    <RApiReturn(GetType(netCDFReader))>
    Public Function openCDF(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            Return netCDFReader.Open(DirectCast(file, String))
        ElseIf TypeOf file Is Stream Then
            Return New netCDFReader(New BinaryDataReader(DirectCast(file, Stream)))
        ElseIf TypeOf file Is pipeline AndAlso DirectCast(file, pipeline).elementType Like GetType(Byte) Then
            Return New netCDFReader(New BinaryDataReader(New MemoryStream(DirectCast(file, pipeline).populates(Of Byte)(env).ToArray)))
        Else
            Return Internal.debug.stop(New InvalidProgramException, env)
        End If
    End Function

    <ExportAPI("dimensions")>
    Public Function dimensions(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim dims = DirectCast(file, netCDFReader).dimensions
        Dim record = {DirectCast(file, netCDFReader).recordDimension}
        Dim table As New Rdataframe With {.columns = New Dictionary(Of String, Array)}

        table.columns("name") = dims.Select(Function(d) d.name).JoinIterates(record.Select(Function(d) d.name)).ToArray
        table.columns("size") = dims.Select(Function(d) d.size).JoinIterates(record.Select(Function(d) d.length)).ToArray
        table.columns("record") = dims.Select(Function(d) False).JoinIterates(record.Select(Function(d) True)).ToArray

        Return table
    End Function

    <ExportAPI("globalAttributes")>
    Public Function globalAttributes(file As Object, Optional list As Boolean = False, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim attrs As attribute() = DirectCast(file, netCDFReader).globalAttributes
        Dim data As Object = attrs.attributeData(list)

        Return data
    End Function

    <Extension>
    Private Function attributeData(attrs As attribute(), list As Object) As Object
        Dim name As String() = attrs.Select(Function(a) a.name).ToArray
        Dim value As String() = attrs.Select(Function(a) a.value).ToArray

        If list Then
            Return New list With {
                .slots = name _
                    .SeqIterator _
                    .ToDictionary(Function(i) CStr(i),
                                  Function(i)
                                      Return CObj(value(i))
                                  End Function)
            }
        Else
            Dim type As Array = attrs _
                .Select(Function(a) a.type.ToString) _
                .ToArray
            Dim table As New Rdataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {NameOf(name), name},
                    {NameOf(type), type},
                    {NameOf(value), value}
                },
                .rownames = name
            }

            Return table
        End If
    End Function

    <ExportAPI("variables")>
    Public Function variableNames(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim symbols = DirectCast(file, netCDFReader).variables
        Dim name As Array = symbols.Select(Function(a) a.name).ToArray
        Dim type As Array = symbols.Select(Function(a) a.type.ToString).ToArray
        Dim dimensions As Array = symbols.Select(Function(a) a.dimensions.GetJson).ToArray
        Dim attributes As Array = symbols.Select(Function(a) a.attributes.TryCount).ToArray
        Dim offset As Array = symbols.Select(Function(a) a.offset).ToArray
        Dim size As Array = symbols.Select(Function(a) a.size).ToArray
        Dim record As Array = symbols.Select(Function(a) a.record).ToArray

        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {NameOf(name), name},
                {NameOf(type), type},
                {NameOf(dimensions), dimensions},
                {NameOf(attributes), attributes},
                {NameOf(offset), offset},
                {NameOf(size), size},
                {NameOf(record), record}
            },
            .rownames = name
        }

        Return table
    End Function

    ''' <summary>
    ''' get attribute data of a cdf variable
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="name"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("attr")>
    Public Function getAttributes(file As Object, name As String,
                                  Optional list As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim var As variable = DirectCast(file, netCDFReader).getDataVariableEntry(name)
        Dim attrs As attribute() = var.attributes
        Dim data As Object = attrs.attributeData(list)

        Return data
    End Function

    <ExportAPI("var")>
    Public Function getDataVariable(file As Object, name As String, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim var As variable = DirectCast(file, netCDFReader).getDataVariableEntry(name)
        Dim data As ICDFDataVector = DirectCast(file, netCDFReader).getDataVariable(var)

        var.value = data

        Return var
    End Function

    ''' <summary>
    ''' get value from variable
    ''' </summary>
    ''' <param name="var"></param>
    ''' <returns></returns>
    <ExportAPI("getValue")>
    Public Function getValue(var As variable) As Object
        Return var.value.genericValue
    End Function

    ''' <summary>
    ''' read multiple variable symbols in the given cdf file and then create as dataframe object
    ''' </summary>
    ''' <param name="cdf"></param>
    ''' <param name="symbols">the symbol will be used as the columns in the generated dataframe</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("dataframe")>
    Public Function dataframe(cdf As netCDFReader, symbols As String(), rownames As String) As Rdataframe
        Dim table As New Rdataframe With {.columns = New Dictionary(Of String, Array)}

        For Each name As String In symbols
            table.columns(name) = cdf.getDataVariable(name).genericValue
        Next

        table.rownames = cdf.getDataVariable(rownames).genericValue _
            .AsObjectEnumerator _
            .Select(AddressOf any.ToString) _
            .ToArray

        Return table
    End Function
End Module
