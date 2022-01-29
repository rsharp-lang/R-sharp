Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class Writer

    ReadOnly file As BinaryDataWriter

    Private Sub New(file As BinaryDataWriter)
        Me.file = file

        Call file.Write(Parser.magic_dict(FileTypes.rdata_binary_v3))
        Call file.Write(Parser.format_dict(RdataFormats.XDR))
        Call writeVersion()
    End Sub

    Public Sub WriteSymbols(symbols As list)

    End Sub

    ''' <summary>
    ''' save R# symbols into RData to a specific file stream buffer.
    ''' </summary>
    ''' <param name="symbols"></param>
    ''' <param name="file"></param>
    ''' 
    Public Shared Sub Save(symbols As list, file As Stream)
        Using writer As New BinaryDataWriter(file)
            Call Save(symbols, writer)
        End Using
    End Sub

    Public Shared Sub Save(symbols As list, file As BinaryDataWriter)
        Call New Writer(file).WriteSymbols(symbols)
    End Sub

    Public Shared Function Open(file As Stream) As Writer
        Return New Writer(New BinaryDataWriter(file))
    End Function

    Private Sub writeVersion()
        Call Xdr.EncodeInt32(0, Me.file) ' format 
        Call Xdr.EncodeInt32(0, Me.file) ' r_serialized 
        Call Xdr.EncodeInt32(0, Me.file) ' minimum 
    End Sub

    Private Sub writeExtractInfo()

    End Sub
End Class
