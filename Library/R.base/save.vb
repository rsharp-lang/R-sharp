#Region "Microsoft.VisualBasic::b8809ae6ea7d029e2d1cfbdad4ee4612, Library\R.base\save.vb"

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

    ' Module base
    ' 
    '     Function: decodeStringVector, load, save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute
Imports RVariable = SMRUCC.Rsharp.Runtime.Components.Variable

Partial Module base

    <ExportAPI("load")>
    <Description("Reload datasets written with the function save.")>
    Public Function load(file As String, envir As Environment) As Object
        If Not file.FileExists Then
            Return Internal.debug.stop({"Disk file is unavailable...", file.GetFullPath}, envir)
        End If

        Dim tmp = App.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(8, True))

        Call UnZip.ImprovedExtractToDirectory(file, tmp, Overwrite.Always, True)

        Using reader As New netCDFReader(tmp & "/R#.Data")
            Dim objectNames = reader.getDataVariable("R#.objects").decodeStringVector
            Dim numOfObjects As Integer = reader("numOfObjects")
            Dim value As CDFData
            Dim var As RVariable

            If objectNames.Length <> numOfObjects Then
                Return Internal.debug.stop({"Invalid file format!", "file=" & file}, envir)
            End If

            For Each name As String In objectNames
                value = reader.getDataVariable(name)
                var = envir.FindSymbol(name)

                If var Is Nothing Then
                    var = New RVariable With {
                        .name = name,
                        .value = Nothing
                    }
                    envir.variables.Add(name, var)
                End If

                If value.cdfDataType = CDFDataTypes.CHAR Then
                    var.value = value.decodeStringVector
                Else
                    var.value = value.genericValue
                End If
            Next

            Return objectNames
        End Using
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function decodeStringVector(value As CDFData) As Object
        Return value.chars.DecodeBase64.LoadJSON(Of String())
    End Function

    ''' <summary>
    ''' 数据将会被保存为netCDF文件然后进行zip压缩保存
    ''' </summary>
    ''' <param name="objects">一般为一个list对象</param>
    ''' <param name="file"></param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("save")>
    <Description("writes an external representation of R objects to the specified file. The objects can be read back from the file at a later date by using the function load or attach (or data in some cases).")>
    <Argument("objects", False, CLITypes.Undefined, AcceptTypes:={GetType(String())}, Description:="the names of the objects to be saved (as symbols or character strings).")>
    <Argument("file", False, CLITypes.File, Description:="a (writable binary-mode) connection or the name of the file where the data will be saved (when tilde expansion is done). Must be a file name for save.image or version = 1.")>
    <Argument("envir", True, CLITypes.File, Description:="environment to search for objects to be saved.")>
    Public Function save(<RListObjectArgument> objects As Object, file$, envir As Environment) As Object
        If file.StringEmpty Then
            Return Internal.debug.stop("'file' must be specified!", envir)
        ElseIf objects Is Nothing Then
            Return Internal.debug.stop("'object' is nothing!", envir)
        End If

        ' 先保存为cdf文件
        Dim tmp As String = App.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(5, True)).TrimSuffix & "/R#.Data"
        Dim value As CDFData
        Dim maxChartSize As Integer = 2048
        Dim length As cdfAttribute
        Dim objList As NamedValue(Of Object)() = RListObjectArgumentAttribute _
            .getObjectList(objects, envir) _
            .ToArray

        Using cdf As CDFWriter = New CDFWriter(tmp).GlobalAttributes(
            New cdfAttribute With {.name = "program", .type = CDFDataTypes.CHAR, .value = "SMRUCC/R#"},
            New cdfAttribute With {.name = "numOfObjects", .type = CDFDataTypes.INT, .value = objList.Length},
            New cdfAttribute With {.name = "maxCharSize", .type = CDFDataTypes.INT, .value = maxChartSize}
        ).Dimensions(Dimension.Byte,
                     Dimension.Double,
                     Dimension.Float,
                     Dimension.Integer,
                     Dimension.Long,
                     Dimension.Short,
                     Dimension.Text(maxChartSize))

            Dim Robjects As New NamedValue(Of Object) With {
                .Name = "R#.objects",
                .Value = objList.Keys.ToArray
            }

            For Each obj As NamedValue(Of Object) In objList.JoinIterates(Robjects)
                Dim vector As Array = Runtime.asVector(Of Object)(obj.Value)
                Dim elTypes = vector.AsObjectEnumerator _
                    .Select(Function(o) o.GetType) _
                    .GroupBy(Function(t) t.FullName) _
                    .ToArray _
                    .OrderByDescending(Function(g) g.Count) _
                    .First _
                    .First

                If elTypes Is GetType(String) Then
                    value = New CDFData With {
                        .chars = vector _
                            .AsObjectEnumerator _
                            .Select(AddressOf Scripting.ToString) _
                            .GetJson _
                            .Base64String
                    }
                Else
                    value = (CObj(vector), elTypes.GetCDFTypeCode)
                End If

                length = New cdfAttribute With {
                    .name = "lengthOf",
                    .type = CDFDataTypes.INT,
                    .value = vector.Length
                }
                cdf.AddVariable(obj.Name, value, {cdf.getDimension(elTypes.FullName)}, {length})
            Next
        End Using

        ' copy to target file
        Call ZipLib.FileArchive(tmp, file, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)

        Return objList.Keys.ToArray
    End Function
End Module
