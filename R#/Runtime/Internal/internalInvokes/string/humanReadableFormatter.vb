Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal.Invokes

    Module humanReadableFormatter

        <ExportAPI("byte_size")>
        Public Function size(bytes As Double()) As String()
            Return bytes.SafeQuery.Select(AddressOf StringFormats.Lanudry).ToArray
        End Function
    End Module
End Namespace