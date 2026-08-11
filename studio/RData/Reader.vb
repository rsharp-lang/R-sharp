#Region "Microsoft.VisualBasic::421dcce7a0acc7a87150e3b0b6734d3e, studio\RData\Reader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 383
    '    Code Lines: 262 (68.41%)
    ' Comment Lines: 66 (17.23%)
    '    - Xml Docs: 86.36%
    ' 
    '   Blank Lines: 55 (14.36%)
    '     File Size: 13.95 KB


    ' Delegate Function
    ' 
    ' 
    ' Class Reader
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: expand_altrep_to_object, parse_all, parse_bool, parse_complex, parse_extra_info
    '               parse_R_object, parse_versions, ParseData, ParseRDataBinary, parseVector
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Numerics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList
Imports gzip = Microsoft.VisualBasic.Net.Http.GZipStreamHandler
Imports RData = SMRUCC.Rsharp.RDataSet.Struct.RData

Public MustInherit Class Reader

    Protected ReadOnly altrep_constructor_dict As New Dictionary(Of String, AltRepConstructor) From {
         {"deferred_string", AddressOf AltRepConstructorExpander.deferred_string_constructor},
         {"compact_intseq", AddressOf AltRepConstructorExpander.compact_intseq_constructor},
         {"compact_realseq", AddressOf AltRepConstructorExpander.compact_realseq_constructor},
         {"wrap_real", AddressOf AltRepConstructorExpander.wrap_constructor},
         {"wrap_character", AddressOf AltRepConstructorExpander.wrap_constructor},
         {"wrap_logical", AddressOf AltRepConstructorExpander.wrap_constructor},
         {"wrap_integer", AddressOf AltRepConstructorExpander.wrap_constructor},
         {"wrap_complex", AddressOf AltRepConstructorExpander.wrap_constructor},
         {"wrap_raw", AddressOf AltRepConstructorExpander.wrap_constructor}
    }

    Protected ReadOnly expand_altrep As Boolean
    Protected ReadOnly debug As Boolean = False

    <DebuggerStepThrough>
    Sub New(expand_altrep As Boolean, Optional debug As Boolean = False)
        Me.debug = debug
        Me.expand_altrep = expand_altrep
    End Sub

    ''' <summary>
    ''' Parse a boolean.
    ''' </summary>
    ''' <returns></returns>
    Public Function parse_bool() As Boolean
        Return CBool(parse_int())
    End Function

    ''' <summary>
    ''' Parse an integer.
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function parse_int() As Integer

    ''' <summary>
    ''' Parse a double.
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function parse_double() As Double

    ''' <summary>
    ''' Parse a complex number.
    ''' </summary>
    ''' <returns></returns>
    Public Function parse_complex() As Complex
        Return New Complex(parse_double, parse_double)
    End Function

    ''' <summary>
    ''' Parse a string.
    ''' </summary>
    ''' <param name="length"></param>
    ''' <returns></returns>
    Public MustOverride Function parse_string(length As Integer) As Byte()

    ''' <summary>
    ''' Read a single raw byte (used for R 4.x compact integer / 1-byte lengths).
    ''' </summary>
    Public MustOverride Function parse_byte() As Integer

    ''' <summary>
    ''' Read the 24-bit (3-byte) object info integer. R's serialization format
    ''' stores the per-object info header as a 3-byte big-endian integer, not a
    ''' full 4-byte XDR word. Reading it as a 4-byte int shifts every following
    ''' object (length + payload) by one byte and corrupts the parse.
    ''' </summary>
    Public MustOverride Function parse_int24() As Integer

    ''' <summary>
    ''' Parse all the file.
    ''' </summary>
    ''' <returns></returns>
    Public Function parse_all() As RData
        Dim versions As RVersions = parse_versions()
        Dim extra_info As RExtraInfo = parse_extra_info(versions)
        Dim obj As RObject = parse_R_object(is_root:=True)

        Return New RData With {
            .versions = versions,
            .extra = extra_info,
            .[object] = obj
        }
    End Function

    ''' <summary>
    ''' Parse the versions header.
    ''' </summary>
    ''' <returns></returns>
    Public Function parse_versions() As RVersions
        Dim format_version = parse_int()
        Dim r_version = parse_int()
        Dim minimum_r_version = parse_int()

        Static supportedVer As New Index(Of Integer) From {2, 3}

        If Not format_version Like supportedVer Then
            Throw New NotImplementedException($"Format version {format_version} unsupported")
        End If

        Return New RVersions With {
            .format = format_version,
            .serialized = r_version,
            .minimum = minimum_r_version
        }
    End Function

    ''' <summary>
    ''' Parse the versions header.
    ''' </summary>
    ''' <param name="versions"></param>
    ''' <returns></returns>
    Public Function parse_extra_info(versions As RVersions) As RExtraInfo
        Dim encoding As String = Nothing

        If versions.format >= 3 Then
            ' R 4.x format 3 header layout (after the 3 version int32s):
            '   int32  min_version       (4 bytes) - serialization format version
            '   char[] encoding          null-terminated C string, e.g. "UTF-8"
            ' No length prefix is written for the encoding string; it is a plain
            ' null-terminated string immediately following min_version. Consuming
            ' it as a 1-byte/4-byte length would shift every object by several bytes.
            Call parse_int()                          ' min_version int32 (skip)

            Dim buf As New List(Of Byte)
            Dim b As Integer = parse_byte()

            Do While b > 0
                buf.Add(CByte(b))
                b = parse_byte()
            Loop
            ' b = 0 (the terminating NUL) has already been consumed here.

            encoding = buf.ToArray().decode(Encodings.ASCII)
        End If

        Dim extract_info As New RExtraInfo With {
            .encoding = encoding
        }

        Return extract_info
    End Function

    ''' <summary>
    ''' Expand alternative representation to normal object.
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="state"></param>
    ''' <returns></returns>
    Public Function expand_altrep_to_object(info As RObject, state As RObject) As (RObjectInfo, Object)
        ' In R >= 4.x the ALTREP class is serialized as a LISTSXP pairlist
        ' list3(csym, psym, stype): class symbol, package symbol, storage type.
        ' The class name we need lives in the first element (caR) which is a SYMSXP.
        Dim class_list As RObject = info
        Dim class_sym As RObject = class_list.value.CAR

        Do While class_sym.info.type = RObjectType.REF
            class_sym = class_sym.referenced_object
        Loop

        Dim altrep_name As String = class_sym.characters

        If Not altrep_constructor_dict.ContainsKey(altrep_name) Then
            Throw New NotSupportedException($"unsupported ALTREP class: '{altrep_name}'")
        End If

        Dim constructor = altrep_constructor_dict(altrep_name)

        Return constructor(info, state)
    End Function

    ''' <summary>
    ''' Parse a R object.
    ''' </summary>
    ''' <param name="reference_list"></param>
    ''' <returns></returns>
    Friend Function parse_R_object(Optional reference_list As List(Of RObject) = Nothing,
                                     Optional is_root As Boolean = False) As RObject
        If reference_list Is Nothing Then
            ' Index is 1-based, so we insert a dummy object
            reference_list = New List(Of RObject)
        End If

        ' R 4.x writes the per-object info as a 3-byte (24-bit) integer for the
        ' top-level / root object, but as a full 4-byte XDR integer for every
        ' nested object (e.g. STRSXP elements, list members). Reading them with
        ' a single width corrupts the byte alignment of everything that follows.
        Dim info_int As Integer

        If is_root Then
            info_int = parse_int24()
        Else
            info_int = parse_int()
        End If

        Dim info As RObjectInfo = parse_r_object_info(info_int)
        Dim tag = Nothing
        Dim attributes As RObject = Nothing
        Dim referenced_object = Nothing
        Dim tag_read = False
        Dim attributes_read = False
        Dim add_reference = False
        Dim result As RObject = Nothing
        Dim value As Object

        If debug Then
            Call Console.WriteLine($"[{info_int}] => {info.ToString}")
        End If

        Static typeList1 As New Index(Of RObjectType) From {RObjectType.LIST, RObjectType.LANG}
        Static objType3 As New Index(Of RObjectType) From {RObjectType.STR, RObjectType.VEC, RObjectType.EXPR}

        If info.type = RObjectType.NIL Then
            value = Nothing
        ElseIf info.type = RObjectType.SYM Then
            ' Read Char
            value = parse_R_object(reference_list)
            ' Symbols can be referenced
            add_reference = True
        ElseIf info.type Like typeList1 Then
            tag = Nothing

            If info.attributes Then
                attributes = parse_R_object(reference_list)
                attributes_read = True
            ElseIf info.tag Then
                tag = parse_R_object(reference_list)
                tag_read = True
            End If

            ' Read CAR and CDR
            ' RData linked list
            Dim car As RObject = parse_R_object(reference_list)
            Dim cdr As RObject = parse_R_object(reference_list)

            value = New RList With {.CAR = car, .CDR = cdr}
        ElseIf info.type = RObjectType.ENV Then
            result = New RObject With {
                .info = info,
                .tag = tag,
                .attributes = attributes,
                .value = Nothing,
                .referenced_object = referenced_object
            }

            reference_list.Add(result)

            Dim locked = parse_bool()
            Dim enclosure = parse_R_object(reference_list)
            Dim frame = parse_R_object(reference_list)
            Dim hash_table = parse_R_object(reference_list)

            attributes = parse_R_object(reference_list)
            value = New EnvironmentValue With {
                .locked = locked,
                .enclosure = enclosure,
                .frame = frame,
                .hash_table = hash_table
            }
        ElseIf info.type = RObjectType.CHAR Then
            Dim length = parse_int()

            If length > 0 Then
                value = parse_string(length)
            ElseIf length = 0 Then
                value = ""
            ElseIf length = -1 Then
                value = Nothing
            Else
                Throw New NotImplementedException($"Length of CHAR cannot be {length}")
            End If
        ElseIf info.type = RObjectType.LGL Then
            value = parseVector(AddressOf parse_bool)
        ElseIf info.type = RObjectType.INT Then
            value = parseVector(AddressOf parse_int)
        ElseIf info.type = RObjectType.REAL Then
            value = parseVector(AddressOf parse_double)
        ElseIf info.type = RObjectType.CPLX Then
            value = parseVector(AddressOf parse_complex)
        ElseIf info.type = RObjectType.RAW Then
            value = parseVector(AddressOf parse_byte)
        ElseIf info.type Like objType3 Then
            value = parseVector(Function() parse_R_object(reference_list))
        ElseIf info.type = RObjectType.S4 Then
            ' R serializes an S4 object as: the S4 type flag, followed by a
            ' LISTSXP pairlist that holds the named slots, followed by the
            ' object attributes (which contain the "class" name). The slots
            ' pairlist must be consumed here, otherwise every byte that follows
            ' shifts out of alignment and the whole object tree corrupts.
            value = parse_R_object(reference_list)
        ElseIf info.type = RObjectType.ALTREP Then
            Dim altrep_info = parse_R_object(reference_list)
            Dim altrep_state = parse_R_object(reference_list)
            Dim altrep_attr = parse_R_object(reference_list)

            If expand_altrep AndAlso altrep_info IsNot Nothing Then
                With expand_altrep_to_object(info:=altrep_info, state:=altrep_state)
                    info = .Item1
                    value = .Item2
                End With

                attributes = altrep_attr
            Else
                ' value = (altrep_info, altrep_state, altrep_attr)
                ' 20220920 下面这个数据构造可能是错误的
                value = New RObject With {
                    .info = altrep_info.info,
                    .value = RList.CreateNode(altrep_state),
                    .attributes = altrep_attr
                }
            End If

            ' The ALTREP node itself occupies a slot in the reference table, just
            ' like a regular object, so later REF entries can resolve back to it.
            ' (R serializes ALTREP with a ref_index increment.)
            add_reference = True

        ElseIf info.type = RObjectType.EMPTYENV Then
            value = Nothing
        ElseIf info.type = RObjectType.GLOBALENV Then
            value = Nothing
        ElseIf info.type = RObjectType.NILVALUE Then
            value = Nothing
        ElseIf info.type = RObjectType.REF Then
            value = Nothing
            ' Index is 1-based
            referenced_object = reference_list(info.reference - 1)
        Else
            Call $"Type {info.type.ToString}({info.type.Description}) not implemented".Warning
            value = Nothing
        End If

        If info.tag AndAlso Not tag_read Then
            Call $"Tag not implemented for type {info.type} and ignored".Warning
        End If
        If info.attributes AndAlso Not attributes_read Then
            attributes = parse_R_object(reference_list)
        End If

        If result Is Nothing Then
            result = New RObject With {
                .info = info,
                .tag = tag,
                .attributes = attributes,
                .value = RList.CreateNode(value),
                .referenced_object = referenced_object
            }
        Else
            result.info = info
            result.attributes = attributes
            result.value = RList.CreateNode(value)
            result.referenced_object = referenced_object
        End If

        If add_reference Then
            reference_list.Add(result)
        End If

        Return result
    End Function

    Private Function parseVector(Of T)(parse As Func(Of T)) As Array
        Dim length As Integer = parse_int()
        Dim value As T() = New T(length - 1) {}

        For i As Integer = 0 To length - 1
            value(i) = parse()
        Next

        Return value
    End Function

    ''' <summary>
    ''' Select the appropiate parser and parse all the info.
    ''' </summary>
    ''' <param name="bin"></param>
    ''' <returns></returns>
    Public Shared Function ParseRDataBinary(bin As BinaryDataReader,
                                            Optional expand_altrep As Boolean = True,
                                            Optional debug As Boolean = False) As RData

        ' Re-detect the magic from the start of the stream. Note that the caller
        ' (file_type) leaves the stream positioned right after the matched magic,
        ' so we rewind before probing again; then we skip the magic deterministically.
        Call bin.Seek(0, SeekOrigin.Begin)
        Dim magic As FileTypes = Parser.file_type(bin)
        Dim skip As Long

        Select Case magic
            Case FileTypes.rdata_binary_v2, FileTypes.rdata_binary_v3
                ' rda files: "RDXn\n" (5 bytes: R, D, X, n, LF) prefix, then the
                ' "X\n" format magic follows immediately. Skip the RDX prefix so
                ' that rdata_format() can read the "X\n" magic at the right place.
                skip = 5
            Case Else
                ' Plain rds-style blob: the "X\n" format magic is at offset 0,
                ' directly followed by the version block. No prefix to skip.
                skip = 0
        End Select

        Call bin.Seek(skip, SeekOrigin.Begin)

        Dim format_type As RdataFormats = rdata_format(bin)

        If format_type = RdataFormats.XDR Then
            Return New ParserXDR(bin, bin.Position, expand_altrep, debug:=debug).parse_all
        Else
            Throw New NotImplementedException(format_type.Description)
        End If
    End Function

    ''' <summary>
    ''' Parse the data of a R file, received as a sequence of bytes.
    ''' </summary>
    ''' <param name="bin">
    ''' Data extracted of a R file.
    ''' </param>
    ''' <returns>Data contained in the file (versions and object).</returns>
    Public Shared Function ParseData(bin As Stream,
                                     Optional expand_altrep As Boolean = True,
                                     Optional debug As Boolean = False) As RData

        Dim reader As New BinaryDataReader(bin)
        Dim filetype As FileTypes = file_type(reader)

        Select Case filetype
            Case FileTypes.rdata_binary_v2, FileTypes.rdata_binary_v3, FileTypes.Unknown
                Return ParseRDataBinary(reader, expand_altrep, debug)
            Case FileTypes.gzip
                ' The whole stream (starting at the gzip magic 0x1f 0x8b) is a
                ' gzip compressed R serialization blob. Rewind and decompress it
                ' directly, then parse the decompressed stream.
                reader.Seek(Scan0, SeekOrigin.Begin)

                Using ms As New MemoryStream(reader.ReadBytes(CInt(reader.Length)))
                    Using newData As MemoryStream = gzip.UnGzipStream(ms)
                        Call newData.Seek(Scan0, SeekOrigin.Begin)

                        ' parse plain data
                        Return ParseData(newData, expand_altrep, debug)
                    End Using
                End Using
            Case Else
                Throw New NotImplementedException($"Unknown file type: {filetype.Description}")
        End Select
    End Function
End Class
