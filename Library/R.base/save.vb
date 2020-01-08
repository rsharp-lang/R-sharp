#Region "Microsoft.VisualBasic::d6cc02cf2ce55cccdaf413306e20a783, Library\R.base\save.vb"

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

    ''' <summary>
    ''' ### Reload Saved Datasets
    ''' 
    ''' Reload datasets written with the function ``save``.
    ''' </summary>
    ''' <param name="file">a (readable binary-mode) connection or a character 
    ''' string giving the name of the file to load (when tilde expansion is 
    ''' done).</param>
    ''' <param name="envir">the environment where the data should be loaded.</param>
    ''' <param name="verbose">
    ''' should item names be printed during loading?
    ''' </param>
    ''' <returns>
    ''' A character vector of the names of objects created, invisibly.
    ''' </returns>
    ''' <remarks>
    ''' load can load R objects saved in the current or any earlier format. 
    ''' It can read a compressed file (see save) directly from a file or from 
    ''' a suitable connection (including a call to url).
    '''
    ''' A Not-open connection will be opened in mode "rb" And closed after 
    ''' use. Any connection other than a gzfile Or gzcon connection will be 
    ''' wrapped in gzcon to allow compressed saves to be handled: note that 
    ''' this leaves the connection In an altered state (In particular, 
    ''' binary-only), And that it needs To be closed explicitly (it will 
    ''' Not be garbage-collected).
    '''
    ''' Only R objects saved In the current format (used since R 1.4.0) can 
    ''' be read from a connection. If no input Is available On a connection 
    ''' a warning will be given, but any input Not In the current format 
    ''' will result In a Error.
    '''
    ''' Loading from an earlier version will give a warning about the 'magic number’: 
    ''' magic numbers 1971:1977 are from R &lt; 0.99.0, and RD[ABX]1 from 
    ''' R 0.99.0 to R 1.3.1. These are all obsolete, and you are strongly 
    ''' recommended to re-save such files in a current format.
    '''
    ''' The verbose argument Is mainly intended For debugging. If it Is True, 
    ''' Then As objects from the file are loaded, their names will be printed 
    ''' To the console. If verbose Is Set To an Integer value greater than 
    ''' one, additional names corresponding To attributes And other parts Of 
    ''' individual objects will also be printed. Larger values will print names 
    ''' To a greater depth.
    '''
    ''' Objects can be saved With references To namespaces, usually As part Of 
    ''' the environment Of a Function Or formula. Such objects can be loaded 
    ''' even If the Namespace Is Not available: it Is replaced by a reference 
    ''' to the global environment with a warning. The warning identifies the 
    ''' first object with such a reference (but there may be more than one).
    ''' </remarks>
    <ExportAPI("load")>
    Public Function load(file As String,
                         Optional envir As Environment = Nothing,
                         Optional verbose As Boolean = False) As Object

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
    ''' ### Save R Objects
    ''' 
    ''' writes an external representation of R objects to the specified file. 
    ''' The objects can be read back from the file at a later date by using 
    ''' the function load or attach (or data in some cases).
    ''' </summary>
    ''' <param name="objects">the names of the objects to be saved (as symbols or character strings).</param>
    ''' <param name="file">
    ''' a (writable binary-mode) connection or the name of the file where 
    ''' the data will be saved (when tilde expansion is done). Must be a 
    ''' file name for save.image or version = 1.
    ''' </param>
    ''' <param name="envir">environment to search for objects to be saved.</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("save")>
    Public Function save(<RListObjectArgument> objects As Object, file$, envir As Environment) As Object
        ' 数据将会被保存为netCDF文件然后进行zip压缩保存
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
