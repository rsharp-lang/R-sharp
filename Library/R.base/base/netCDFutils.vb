#Region "Microsoft.VisualBasic::d081396f0ba7005afe2c1bfeb183f6a4, Library\R.base\base\netCDFutils.vb"

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

    ' Module netCDFutils
    ' 
    '     Function: globalAttributes, openCDF, variableNames
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

<Package("netCDF.utils")>
Module netCDFutils

    <ExportAPI("open.netCDF")>
    <RApiReturn(GetType(netCDFReader))>
    Public Function openCDF(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            Return netCDFReader.Open(DirectCast(file, String))
        ElseIf TypeOf file Is Stream Then
            Return New netCDFReader(New BinaryDataReader(DirectCast(file, Stream)))
        ElseIf TypeOf file Is pipeline AndAlso DirectCast(file, pipeline).elementType Like GetType(Byte) Then
            Return New netCDFReader(New BinaryDataReader(New MemoryStream(DirectCast(file, pipeline).populates(Of Byte).ToArray)))
        Else
            Return Internal.debug.stop(New InvalidProgramException, env)
        End If
    End Function

    <ExportAPI("globalAttributes")>
    Public Function globalAttributes(file As Object, Optional env As Environment = Nothing) As Object
        If TypeOf file Is String Then
            file = netCDFReader.Open(DirectCast(file, String))
        End If
        If Not TypeOf file Is netCDFReader Then
            Return Internal.debug.stop(New NotImplementedException, env)
        End If

        Dim attrs = DirectCast(file, netCDFReader).globalAttributes
        Dim name As Array = attrs.Select(Function(a) a.name).ToArray
        Dim type As Array = attrs.Select(Function(a) a.type.ToString).ToArray
        Dim value As Array = attrs.Select(Function(a) a.value).ToArray
        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {NameOf(name), name},
                {NameOf(type), type},
                {NameOf(value), value}
            },
            .rownames = name
        }

        Return table
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
End Module

