#Region "Microsoft.VisualBasic::25ad35ff48a3ff0ac38094a59d91b185, Library\R.base\save\save.vb"

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
    '     Function: load, parseRData, readRDS, save, saveRDS
    ' 
    '     Sub: saveImage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.RDataSet
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute
Imports RSymbol = SMRUCC.Rsharp.Runtime.Components.Symbol

Partial Module base

    ''' <summary>
    ''' read ``*.rda`` data file which is saved from R environment. 
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("readRData")>
    Public Function parseRData(file As String) As Object
        Using buffer As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            Dim obj As Struct.RData = Reader.ParseData(buffer)
            Dim symbols As list = ConvertToR.ToRObject(obj.object)

            Return symbols
        End Using
    End Function

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
                         Optional envir As GlobalEnvironment = Nothing,
                         Optional verbose As Boolean = False) As Object

        If Not file.FileExists Then
            Return Internal.debug.stop({"Disk file is unavailable...", file.GetFullPath}, envir)
        End If

        Dim tmp = TempFileSystem.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(8, True))

        Call UnZip.ImprovedExtractToDirectory(file, tmp, Overwrite.Always, True)

        Using reader As New netCDFReader(tmp & "/R#.Data")
            Dim objectNames = reader.getDataVariable("R#.objects").decodeStringVector
            Dim numOfObjects As Integer = reader("numOfObjects")
            Dim objects As NamedValue(Of Object)() = rawDeserializer.loadObject(reader, objectNames).ToArray

            If objects.Length <> numOfObjects Then
                Return Internal.debug.stop({"Invalid file format!", "file=" & file}, envir)
            Else
                Return objects
            End If

            For Each obj As NamedValue(Of Object) In objects
                Dim var As RSymbol = envir.FindSymbol(obj.Name)

                If var Is Nothing Then
                    envir.Push(obj.Name, Nothing, [readonly]:=False)
                    var = envir.FindSymbol(obj.Name)
                End If

                Call var.SetValue(obj.Value, envir)
            Next

            Return objectNames
        End Using
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
        Dim tmp As String = TempFileSystem.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(5, True)).TrimSuffix & "/R#.Data"
        Dim maxChartSize As Integer = 2048
        Dim objList As NamedValue(Of Object)() = RListObjectArgumentAttribute _
            .getObjectList(objects, envir) _
            .ToArray

        Using cdf As CDFWriter = New CDFWriter(tmp).GlobalAttributes(
            New cdfAttribute With {.name = "program", .type = CDFDataTypes.CHAR, .value = "SMRUCC/R#"},
            New cdfAttribute With {.name = "numOfObjects", .type = CDFDataTypes.INT, .value = objList.Length},
            New cdfAttribute With {.name = "maxCharSize", .type = CDFDataTypes.INT, .value = maxChartSize},
            New cdfAttribute With {.name = "level", .type = CDFDataTypes.INT, .value = CInt(RData.RDA)},
            New cdfAttribute With {.name = "time", .type = CDFDataTypes.DOUBLE, .value = UnixTimeStamp},
            New cdfAttribute With {.name = "github", .type = CDFDataTypes.CHAR, .value = LICENSE.githubURL}
        )

            Dim Robjects As New NamedValue(Of Object) With {
                .Name = "R#.objects",
                .Value = objList.Keys.ToArray
            }

            For Each obj As NamedValue(Of Object) In objList.JoinIterates(Robjects)
                Call cdf.writeObject(obj.Name, obj.Value)
            Next
        End Using

        ' copy to target file
        Call ZipLib.FileArchive(tmp, file, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)

        Return objList.Keys.ToArray
    End Function

    ''' <summary>
    ''' Serialization Interface for Single Objects
    ''' 
    ''' Functions to write a single R object to a file, and to restore it.
    ''' </summary>
    ''' <param name="object">R object to serialize.</param>
    ''' <param name="file">
    ''' a connection Or the name Of the file where the R Object 
    ''' Is saved To Or read from.</param>
    ''' <param name="ascii"></param>
    ''' <param name="version"></param>
    ''' <param name="compress"></param>
    ''' <param name="refhook">
    ''' a hook function for handling reference objects.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("saveRDS")>
    <RApiReturn(GetType(Boolean))>
    Public Function saveRDS(<RRawVectorArgument>
                            [object] As Object,
                            file$,
                            Optional ascii As Boolean = False,
                            Optional version$ = "classic",
                            Optional compress As Boolean = True,
                            Optional refhook$ = Nothing,
                            Optional env As Environment = Nothing) As Object

        ' 数据将会被保存为netCDF文件然后进行zip压缩保存
        If file.StringEmpty Then
            Return Internal.debug.stop("'file' must be specified!", env)
        ElseIf [object] Is Nothing Then
            Return Internal.debug.stop("'object' is nothing!", env)
        End If

        ' 先保存为cdf文件
        Dim tmp As String = TempFileSystem.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(5, True)).TrimSuffix & "/R#.Data"
        Dim maxChartSize As Integer = 2048

        Using cdf As CDFWriter = New CDFWriter(tmp).GlobalAttributes(
            New cdfAttribute With {.name = "program", .type = CDFDataTypes.CHAR, .value = "SMRUCC/R#"},
            New cdfAttribute With {.name = "maxCharSize", .type = CDFDataTypes.INT, .value = maxChartSize},
            New cdfAttribute With {.name = "level", .type = CDFDataTypes.INT, .value = CInt(RData.RDS)},
            New cdfAttribute With {.name = "version", .type = CDFDataTypes.CHAR, .value = version}
        )

            Call cdf.writeObject("data", [object])
        End Using

        ' copy to target file
        Call ZipLib.FileArchive(tmp, file, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)

        Return True
    End Function

    ''' <summary>
    ''' Serialization Interface for Single Objects
    ''' 
    ''' Functions to write a single R object to a file, and to restore it.
    ''' </summary>
    ''' <param name="file">
    ''' a connection Or the name Of the file where the R Object 
    ''' Is saved To Or read from.</param>
    ''' <param name="refhook">
    ''' a hook function for handling reference objects.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("readRDS")>
    Public Function readRDS(file$, Optional refhook As Object = Nothing, Optional env As Environment = Nothing) As Object

    End Function

    ''' <summary>
    ''' ### Save R Objects
    ''' 
    ''' ``save.image()`` is just a short-cut for ‘save my current workspace’, 
    ''' i.e., save(list = ls(all.names = TRUE), file = ".RData", envir = .GlobalEnv). 
    ''' It is also what happens with q("yes").
    ''' </summary>
    ''' <param name="file">a (writable binary-mode) connection or the name of the file 
    ''' where the data will be saved (when tilde expansion is done). Must be a file 
    ''' name for save.image or version = 1.</param>
    ''' <param name="version">the workspace format version to use. NULL specifies the 
    ''' current default format (2). Version 1 was the default from R 0.99.0 to R 1.3.1 
    ''' and version 2 from R 1.4.0. Version 3 is supported from R 3.5.0.</param>
    ''' <param name="ascii">if TRUE, an ASCII representation of the data is written. 
    ''' The default value of ascii is FALSE which leads to a binary file being written. 
    ''' If NA and version >= 2, a different ASCII representation is used which writes 
    ''' double/complex numbers as binary fractions.</param>
    ''' <param name="compress">logical Or character string specifying whether saving to 
    ''' a named file Is to use compression. TRUE corresponds to gzip compression, And 
    ''' character strings "gzip", "bzip2" Or "xz" specify the type of compression. Ignored 
    ''' when file Is a connection And for workspace format version 1.</param>
    ''' <param name="safe">logical. If TRUE, a temporary file is used for creating the 
    ''' saved workspace. The temporary file is renamed to file if the save succeeds. This 
    ''' preserves an existing workspace file if the save fails, but at the cost of using 
    ''' extra disk space during the save.</param>
    ''' <param name="envir">environment to search for objects to be saved.</param>
    <ExportAPI("save.image")>
    Public Sub saveImage(Optional file$ = ".RData",
                         Optional version$ = Nothing,
                         Optional ascii As Boolean = False,
                         Optional compress As Object = "!ascii",
                         Optional safe As Boolean = True,
                         Optional envir As Environment = Nothing)

    End Sub
End Module
