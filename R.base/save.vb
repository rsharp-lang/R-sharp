Imports SMRUCC.Rsharp.Runtime

Partial Module base

    ''' <summary>
    ''' 数据将会被保存为netCDF文件然后进行zip压缩保存
    ''' </summary>
    ''' <param name="objects"></param>
    ''' <param name="file"></param>
    ''' <param name="envir"></param>
    ''' <returns></returns>
    Public Function save(objects As Object, file$, envir As Environment) As Object
        If file.StringEmpty Then
            Return Internal.stop("'file' must be specified!", envir)
        End If
    End Function
End Module
