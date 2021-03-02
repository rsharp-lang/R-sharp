Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Bzip2

Module Module1

    Sub Main()
        Dim output2 = New MemoryStream()
        Dim decompressor = New BZip2InputStream("F:\report.rda.tar".Open, True)
        decompressor.CopyTo(output2)

        Call output2.FlushStream("F:\report.rda\report2.rda")
    End Sub

End Module
