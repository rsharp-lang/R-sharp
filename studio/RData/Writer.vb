#Region "Microsoft.VisualBasic::849d8182e9cc836b4839731fa73509d8, R-sharp\studio\RData\Writer.vb"

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

    '   Total Lines: 159
    '    Code Lines: 123
    ' Comment Lines: 8
    '   Blank Lines: 28
    '     File Size: 5.65 KB


    ' Class Writer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Open
    ' 
    '     Sub: dataFrame, intVector, logicalVector, realVector, (+2 Overloads) Save
    '          stringVector, write, writeExtractInfo, writeSymbols, writeVersion
    ' 
    ' /********************************************************************************/

#End Region

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

    <DebuggerStepThrough>
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

    Public Sub writeSymbols(symbols As list)
        Dim info_int As Integer = RObjectInfo.LISTSXP.EncodeInfoInt32

        For Each key As String In symbols.getNames
            Call Xdr.EncodeInt32(info_int, file)     ' LISTSXP
            Call ByteEncoder.EncodeSymbol(key, file) ' symbol name
            Call write(any:=symbols.getByName(key))  ' data
        Next

        Call Xdr.EncodeInt32(RObjectInfo.NILVALUESXP.EncodeInfoInt32, file)
    End Sub

    Public Sub dataFrame(df As dataframe)
        Dim length As Integer = df.nrows
        Dim bits As Integer = RObjectInfo.primitiveType(
            baseType:=RObjectType.VEC,
            is_object:=True,
            has_attributes:=True,
            has_tag:=False
        ).EncodeInfoInt32()

        Call Xdr.EncodeInt32(bits, file)
        Call Xdr.EncodeInt32(df.ncols, file)

        For Each colname As String In df.colnames
            Call write(df(colname))
        Next

        Dim attributes As New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"names", df.colnames},
                {"class", {"data.frame"}},
                {"row.names", NA_INT.Replicate(df.nrows).ToArray}
            }
        }

        Call writeSymbols(attributes)
    End Sub

    Public Sub write(any As Object)
        If any Is Nothing Then
            Call stringVector({})
        ElseIf TypeOf any Is list Then
            Call writeSymbols(any)
        ElseIf TypeOf any Is dataframe Then
            Call dataFrame(any)
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
        Call New Writer(file, env Or GlobalEnvironment.defaultEmpty).writeSymbols(symbols)
    End Sub

    Public Shared Function Open(file As Stream, Optional env As Environment = Nothing) As Writer
        Return New Writer(New BinaryDataWriter(file), env Or GlobalEnvironment.defaultEmpty)
    End Function

    Private Sub writeVersion()
        ' {RVersions(format=3, serialized=262147, minimum=197888)}
        '
        Call Xdr.EncodeInt32(3, Me.file) ' format 
        Call Xdr.EncodeInt32(2, Me.file) ' r_serialized 
        Call Xdr.EncodeInt32(0, Me.file) ' minimum 
    End Sub

    Private Sub writeExtractInfo()
        Dim encoding As Encoding = file.Encoding
        Dim name As String = encoding.HeaderName.ToUpper
        Dim nameBytes As Byte() = Encoding.ASCII.GetBytes(name)

        Call Xdr.EncodeInt32(nameBytes.Length, Me.file)
        Call file.Write(nameBytes)
    End Sub
End Class
