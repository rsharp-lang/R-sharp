Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute

Partial Module base

    Public Function load(file As String, envir As Environment) As Object
        If Not file.FileExists Then
            Return Internal.stop({"Disk file is unavailable...", file.GetFullPath}, envir)
        End If
    End Function

    ''' <summary>
    ''' 数据将会被保存为netCDF文件然后进行zip压缩保存
    ''' </summary>
    ''' <param name="objects">一般为一个list对象</param>
    ''' <param name="file"></param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
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
        Else
            Throw New NotImplementedException
        End If

        ' 先保存为cdf文件
        Dim tmp As String = App.GetAppSysTempFile
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

            For Each obj As NamedValue(Of Object) In objList
                Dim vector As Array = Runtime.asVector(obj.Value, GetType(Object))
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

        Return objList.Keys.ToArray
    End Function
End Module
