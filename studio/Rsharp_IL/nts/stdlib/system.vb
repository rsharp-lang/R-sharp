Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace jsstd

    Public Module system

        <ExportAPI("Date")>
        Public Function [Date]() As Object
            Return Now
        End Function
    End Module
End Namespace