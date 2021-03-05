Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Bzip2
Imports RData

Module Module1

    Sub Main()
        ' Dim output2 = New MemoryStream()
        ' Dim decompressor = New BZip2InputStream("F:\report.rda.tar".Open, True)
        ' decompressor.CopyTo(output2)

        ' Call output2.FlushStream("F:\report.rda\report2.rda")
        Using file = "E:\GCModeller\src\R-sharp\studio\RData\test\x.rda".Open
            Dim obj = Reader.ParseData(file)

            Pause()
        End Using
    End Sub

End Module
