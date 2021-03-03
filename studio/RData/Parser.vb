Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Parser interface for a R file.
''' </summary>
Module Parser

    ReadOnly magic_dict As New Dictionary(Of FileTypes, Byte()) From {
        {FileTypes.bzip2, bytes("\x42\x5a\x68")},
        {FileTypes.gzip, bytes("\x1f\x8b")},
        {FileTypes.xz, bytes("\xFD7zXZ\x00")},
        {FileTypes.rdata_binary_v2, bytes("RDX2\n")},
        {FileTypes.rdata_binary_v3, bytes("RDX3\n")}
    }

    ReadOnly format_dict As New Dictionary(Of RdataFormats, Byte()) From {
        {RdataFormats.XDR, bytes("X\n")},
        {RdataFormats.ASCII, bytes("A\n")},
        {RdataFormats.binary, bytes("B\n")}
    }

    Private Function bytes(binaryStr As String) As Byte()
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' Returns the type of the file.
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function file_type(file As BinaryDataReader) As FileTypes
        For Each item In magic_dict
            If item.Value.SequenceEqual(file.ReadBytes(item.Value.Length)) Then
                Return item.Key
            Else
                file.Seek(-item.Value.Length, SeekOrigin.Current)
            End If
        Next

        Return Nothing
    End Function

    Public Function rdata_format(file As BinaryDataReader) As RdataFormats
        For Each item In format_dict
            If item.Value.SequenceEqual(file.ReadBytes(item.Value.Length)) Then
                Return item.Key
            Else
                file.Seek(-item.Value.Length, SeekOrigin.Current)
            End If
        Next

        Return Nothing
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function decode(bytes As Byte(), encoding As Encodings) As String
        Return encoding.CodePage.GetString(bytes)
    End Function

    ''' <summary>
    ''' Parse the internal information of an object.
    ''' </summary>
    ''' <param name="info_int"></param>
    ''' <returns></returns>
    Public Function parse_r_object_info(info_int As Integer) As RObjectInfo


    End Function
End Module
