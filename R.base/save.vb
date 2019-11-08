Imports System.ComponentModel
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute

Partial Module base

    <ExportAPI("load")>
    <Description("Reload datasets written with the function save.")>
    Public Function load(file As String, envir As Environment) As Object
        If Not file.FileExists Then
            Return Internal.stop({"Disk file is unavailable...", file.GetFullPath}, envir)
        End If

        Dim tmp = App.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(8, True))

        Call UnZip.ImprovedExtractToDirectory(file, tmp, Overwrite.Always, True)

        Using reader As New netCDFReader(tmp & "/R#.Data")

        End Using
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
            Return Internal.stop("'file' must be specified!", envir)
        ElseIf objects Is Nothing Then
            Return Internal.stop("'object' is nothing!", envir)
        End If

        Dim objList As New List(Of NamedValue(Of Object))
        Dim type As Type = objects.GetType

        If type Is GetType(Dictionary(Of String, Object)) Then
            For Each item In DirectCast(objects, Dictionary(Of String, Object))
                objList += New NamedValue(Of Object) With {
                    .Name = item.Key,
                    .Value = item.Value
                }
            Next
        ElseIf type Is GetType(InvokeParameter()) Then
            For Each item As InvokeParameter In DirectCast(objects, InvokeParameter())
                objList += New NamedValue(Of Object) With {
                    .Name = item.name,
                    .Value = item.Evaluate(envir)
                }
            Next
        Else
            Throw New NotImplementedException
        End If

        ' 先保存为cdf文件
        Dim tmp As String = App.GetAppSysTempFile(".cdf", App.PID, prefix:=RandomASCIIString(5, True)).TrimSuffix & "/R#.Data"
        Dim value As CDFData
        Dim maxChartSize As Integer = 2048
        Dim length As cdfAttribute

        Using cdf As CDFWriter = New CDFWriter(tmp).GlobalAttributes(
            New cdfAttribute With {.name = "program", .type = CDFDataTypes.CHAR, .value = "SMRUCC/R#"},
            New cdfAttribute With {.name = "numOfObjects", .type = CDFDataTypes.INT, .value = objList.Count},
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
                .Value = objList.Keys.GetJson.Base64String
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

                value = (CObj(vector), elTypes.GetCDFTypeCode)
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
