Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class Writer

    ReadOnly file As BinaryDataWriter
    ReadOnly env As Environment

    Private Sub New(file As BinaryDataWriter, env As Environment)
        Me.file = file
        Me.env = env

        Call file.Write(Parser.magic_dict(FileTypes.rdata_binary_v3))
        Call file.Write(Parser.format_dict(RdataFormats.XDR))
        Call writeVersion()
        Call writeExtractInfo()
    End Sub

    Public Sub stringVector(vector As String())
        Dim bits As Integer = RObjectInfo.STRSXP.EncodeInfoInt32
        Dim len As Integer = vector.Length

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(len, file)
        Call vector.DoEach(Sub(str) ByteEncoder.stringScalar(str, file))
    End Sub

    Public Sub realVector(vector As Double())
        Dim bits As Integer = RObjectInfo.REALSXP.EncodeInfoInt32
        Dim len As Integer = vector.Length

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(len, file)
        Call vector.DoEach(Sub(d) ByteEncoder.realScalar(d, file))
    End Sub

    Public Sub intVector(vector As Integer())
        Dim bits As Integer = RObjectInfo.INTSXP.EncodeInfoInt32
        Dim len As Integer = vector.Length

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(len, file)
        Call vector.DoEach(Sub(i) ByteEncoder.intScalar(i, file))
    End Sub

    Public Sub logicalVector(vector As Boolean())
        Dim bits As Integer = RObjectInfo.LGLSXP.EncodeInfoInt32
        Dim len As Integer = vector.Length

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(len, file)
        Call vector.DoEach(Sub(f) ByteEncoder.logicalScalar(f, file))
    End Sub

    Public Sub WriteSymbols(symbols As list)
        Dim info_int As Integer = RObjectInfo.LISTSXP.EncodeInfoInt32

        For Each key As String In symbols.getNames
            Call Xdr.EncodeInt32(info_int, file)
            Call ByteEncoder.EncodeSymbol(key, file)
            Call Write(any:=symbols.getByName(key))
        Next
    End Sub

    Public Sub Write(any As Object)
        If any Is Nothing Then
            Call stringVector({})
        ElseIf TypeOf any Is list Then
            Call WriteSymbols(any)
        ElseIf TypeOf any Is dataframe Then
            Throw New NotImplementedException
        Else
            Dim vec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(any), env)
            Dim baseType As Type = vec.GetType.GetElementType

            Select Case baseType
                Case GetType(String) : Call stringVector(vec)
                Case GetType(Integer) : Call intVector(vec)
                Case GetType(Double) : Call realVector(vec)
                Case GetType(Boolean) : Call logicalVector(vec)
                Case Else
                    Throw New NotImplementedException(baseType.FullName)
            End Select
        End If
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

    Public Shared Sub Save(symbols As list, file As BinaryDataWriter, Optional env As Environment = Nothing)
        If env Is Nothing Then
            env = GlobalEnvironment.defaultEmpty
        End If

        Call New Writer(file, env).WriteSymbols(symbols)
    End Sub

    Public Shared Function Open(file As Stream, Optional env As Environment = Nothing) As Writer
        If env Is Nothing Then
            env = GlobalEnvironment.defaultEmpty
        End If

        Return New Writer(New BinaryDataWriter(file), env)
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
