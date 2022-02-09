Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Internal.Invokes

    <Package("bitView")>
    Module bitView

        <ExportAPI("doubles")>
        Public Function doubles(buffer As MemoryStream) As Double()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToDouble)
        End Function

        <ExportAPI("integers")>
        Public Function integers(buffer As MemoryStream) As Integer()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToInt32)
        End Function

        <ExportAPI("floats")>
        Public Function floats(buffer As MemoryStream) As Single()
            Return buffer.bitConvert(sizeof:=4, AddressOf BitConverter.ToSingle)
        End Function

        <ExportAPI("int16s")>
        Public Function shorts(buffer As MemoryStream) As Short()
            Return buffer.bitConvert(sizeof:=2, AddressOf BitConverter.ToInt16)
        End Function

        <ExportAPI("int64s")>
        Public Function int64(buffer As MemoryStream) As Long()
            Return buffer.bitConvert(sizeof:=8, AddressOf BitConverter.ToInt64)
        End Function

        <Extension>
        Private Function bitConvert(Of T)(buffer As MemoryStream, sizeof As Integer, load As Func(Of Byte(), Integer, T)) As T()
            Return buffer.ToArray _
                .Split(sizeof) _
                .Where(Function(b) b.Length = sizeof) _
                .Select(Function(byts) load(byts, Scan0)) _
                .ToArray
        End Function

    End Module
End Namespace