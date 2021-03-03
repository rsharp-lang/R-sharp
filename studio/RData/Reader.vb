Imports System.IO
Imports System.Numerics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text

Public MustInherit Class Reader

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
    ''' Parse all the file.
    ''' </summary>
    ''' <returns></returns>
    Public Function parse_all() As RData
        Dim versions = parse_versions()
        Dim extra_info = parse_extra_info(versions)
        Dim obj = parse_R_object()

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
        Dim encoding_len As Integer

        If versions.format >= 3 Then
            encoding_len = parse_int()
            encoding = parse_string(encoding_len).decode(Encodings.ASCII)
        End If

        Dim extract_info = New RExtraInfo With {.encoding = encoding}

        Return extract_info
    End Function

    ''' <summary>
    ''' Parse a R object.
    ''' </summary>
    ''' <param name="reference_list"></param>
    ''' <returns></returns>
    Public Function parse_R_object(Optional reference_list As List(Of RObject) = Nothing) As RObject
        If reference_list Is Nothing Then
            ' Index is 1-based, so we insert a dummy object
            reference_list = New List(Of RObject)
        End If

        Dim info_int = parse_int()
        Dim info = parse_r_object_info(info_int)
        Dim tag = Nothing
        Dim attributes As RObject = Nothing
        Dim referenced_object = Nothing
        Dim tag_read = False
        Dim attributes_read = False
        Dim add_reference = False
        Dim result As RObject = Nothing
        Dim value As Object

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
                Throw New NotImplementedException("Attributes not suported for LIST")
            ElseIf info.tag Then
                tag = parse_R_object(reference_list)
                tag_read = True
            End If

            ' Read CAR and CDR
            Dim car = parse_R_object(reference_list)
            Dim cdr = parse_R_object(reference_list)

            value = (car, cdr)
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
        ElseIf info.type = RObjectType.Char Then
            Dim length = parse_int()

            If length > 0 Then
                value = parse_string(length)
            ElseIf length = 0 Then
                value = ""
            ElseIf length = 1 Then
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
        ElseIf info.type Like objType3 Then
            value = parseVector(Function() parse_R_object(reference_list))
        ElseIf info.type = RObjectType.S4 Then
            value = Nothing
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
            Throw New NotImplementedException($"Type {info.type} not implemented")
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
                .value = value,
                .referenced_object = referenced_object
            }
        Else
            result.info = info
            result.attributes = attributes
            result.value = value
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
    Public Shared Function ParseRDataBinary(bin As BinaryDataReader) As RData
        Dim format_type = rdata_format(bin)

        If format_type = RdataFormats.XDR Then
            Return New ParserXDR(bin).parse_all
        Else
            Throw New NotImplementedException
        End If
    End Function

    ''' <summary>
    ''' Parse the data of a R file, received as a sequence of bytes.
    ''' </summary>
    ''' <param name="bin">
    ''' Data extracted of a R file.
    ''' </param>
    ''' <returns>Data contained in the file (versions and object).</returns>
    Public Shared Function ParseData(bin As Stream) As RData
        Dim reader As New BinaryDataReader(bin)
        Dim filetype = file_type(reader)

        Select Case filetype
            Case FileTypes.rdata_binary_v2, FileTypes.rdata_binary_v3
                Return ParseRDataBinary(reader)
            Case Else
                Throw New NotImplementedException("Unknown file type")
        End Select
    End Function
End Class
