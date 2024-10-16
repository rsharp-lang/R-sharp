﻿#Region "Microsoft.VisualBasic::ab34063aff1f91295fc1d54afb38f683, Library\base\base\netCDFutils.vb"

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

    '   Total Lines: 316
    '    Code Lines: 235 (74.37%)
    ' Comment Lines: 37 (11.71%)
    '    - Xml Docs: 91.89%
    ' 
    '   Blank Lines: 44 (13.92%)
    '     File Size: 12.81 KB


    ' Module netCDFutils
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: attributeData, dataframe, dimensions, getAttributes, getDataVariable
    '               getValue, globalAttributes, openCDF, printVar, save_dataframe
    '               variableNames
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
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports renv = SMRUCC.Rsharp.Runtime
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' netCDF toolkit
''' </summary>
<Package("netCDF")>
Module netCDFutils

    Sub New()
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of variable)(AddressOf printVar)
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
            Return RInternal.debug.stop(New InvalidProgramException, env)
        End If
    End Function

    <ExportAPI("dimensions")>
    Public Function dimensions(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return RInternal.debug.stop(New NotImplementedException, env)
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
            Return RInternal.debug.stop(New NotImplementedException, env)
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
            Return RInternal.debug.stop(New NotImplementedException, env)
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
            Return RInternal.debug.stop(New NotImplementedException, env)
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
            Return RInternal.debug.stop(New NotImplementedException, env)
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

    ''' <summary>
    ''' save dataframe as netcdf file
    ''' </summary>
    ''' <param name="df">target dataframe object for save as netcdf file</param>
    ''' <param name="file">the file resource or the file path for export the netcdf file data</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("save_dataframe")>
    Public Function save_dataframe(df As Rdataframe, file As Object, Optional env As Environment = Nothing) As Object
        Dim auto_close As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env, is_filepath:=auto_close)

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        End If

        ' a unify dimension size value
        ' for non-scalar columns
        Dim nrows As Integer = df.nrows
        Dim ncols As Integer = df.ncols
        Dim v As Array
        Dim ints As New Dimension("integers", nrows)
        Dim nums As New Dimension("doubles", nrows)

        Using cdf As New CDFWriter(s.TryCast(Of Stream))
            If Not df.rownames Is Nothing Then
                cdf.AddVector("row.names", df.rownames, "rownames")
                cdf.GlobalAttributes("row.names", True)
            End If

            Call cdf.GlobalAttributes("ncols", ncols)
            Call cdf.GlobalAttributes("nrows", nrows)

            For Each name As String In df.colnames
                v = df.columns(name)
                v = renv.UnsafeTryCastGenericArray(v)

                Select Case v.GetType.GetElementType
                    Case GetType(String)
                        cdf.AddVector(name, DirectCast(v, String()), $"dimLen_{name}")
                    Case GetType(Char)
                        cdf.AddVector(name, DirectCast(v, Char()), ints)
                    Case GetType(Integer)
                        cdf.AddVector(name, DirectCast(v, Integer()), ints)
                    Case GetType(Double)
                        cdf.AddVector(name, DirectCast(v, Double()), nums)
                    Case GetType(Single)
                        cdf.AddVector(name, DirectCast(v, Single()), nums)
                    Case GetType(Long)
                        Throw New NotImplementedException(v.GetType.GetElementType.FullName)
                    Case GetType(Boolean)
                        Throw New NotImplementedException(v.GetType.GetElementType.FullName)
                    Case GetType(Byte)
                        Throw New NotImplementedException(v.GetType.GetElementType.FullName)
                    Case Else
                        Throw New NotImplementedException(v.GetType.GetElementType.FullName)
                End Select
            Next
        End Using

        If auto_close Then
            Try
                Call s.TryCast(Of Stream).Flush()
                Call s.TryCast(Of Stream).Close()
            Catch ex As Exception

            End Try
        End If

        Return True
    End Function
End Module
