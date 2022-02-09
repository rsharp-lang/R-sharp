Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Internal.Invokes

    <Package("bitView")>
    Module bitView

        <ExportAPI("doubles")>
        Public Function doubles(buffer As MemoryStream) As Double()
            Return buffer.ToArray.Split(8).Where(Function(b) b.Length = 8).Select(AddressOf BitConverter.ToDouble).ToArray
        End Function

    End Module
End Namespace