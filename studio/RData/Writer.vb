Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class Writer

    ReadOnly file As BinaryDataWriter

    Private Sub New(file As BinaryDataWriter)
        Me.file = file

        Call file.Write(Parser.magic_dict(FileTypes.rdata_binary_v3))
        Call file.Write(Parser.format_dict(RdataFormats.XDR))
        Call writeVersion()
        Call writeExtractInfo()
    End Sub

    Public Sub WriteValue(value As Object, length As Integer)

    End Sub

    Public Sub stringVector(vector As String())
        Dim bits As Integer = RObjectInfo.PrimitiveType(RObjectType.STR).EncodeInfoInt32
        Dim len As Integer = vector.Length

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(len, file)
        Call vector.DoEach(Sub(str) ByteEncoder.stringScalar(str, file))
    End Sub

    Public Sub WriteSymbols(symbols As list)
        Dim info_int As Integer = New RObjectInfo().EncodeInfoInt32

        Call Xdr.EncodeInt32(info_int, Me.file)
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
        ' {RVersions(format=3, serialized=262147, minimum=197888)}
        '
        Call Xdr.EncodeInt32(3, Me.file)      ' format 
        Call Xdr.EncodeInt32(262147, Me.file) ' r_serialized 
        Call Xdr.EncodeInt32(197888, Me.file) ' minimum 
    End Sub

    Private Sub writeExtractInfo()
        Dim encoding As Encoding = file.Encoding
        Dim name As String = encoding.HeaderName.ToUpper
        Dim nameBytes As Byte() = Encoding.ASCII.GetBytes(name)

        Call Xdr.EncodeInt32(nameBytes.Length, Me.file)
        Call file.Write(nameBytes)
    End Sub
End Class
