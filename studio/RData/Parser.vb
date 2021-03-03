Imports System.IO

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
    Public Function file_type(file As Stream) As FileTypes
        For Each item In magic_dict

        Next
    End Function
End Module
