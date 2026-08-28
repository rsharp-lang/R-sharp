#Region "Microsoft.VisualBasic::d38b0830f6800ebbee5efbc76dcb095e, studio\RData\Convertor\RStreamReader.vb"

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

    '   Total Lines: 82
    '    Code Lines: 63 (76.83%)
    ' Comment Lines: 8 (9.76%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 11 (13.41%)
    '     File Size: 3.43 KB


    '     Class RStreamReader
    ' 
    '         Function: ReadIntegers, ReadLogicals, ReadNumbers, ReadString, ReadStrings
    '                   ReadVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Convertor

    Public Class RStreamReader

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ReadString(robj As RObject) As String
            Return robj.DecodeCharacters
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ReadNumbers(robj As RObject) As Double()
            Return CLRVector.asNumeric(robj.value.data)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ReadIntegers(robj As RObject) As Long()
            Return CLRVector.asLong(robj.value.data)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ReadLogicals(robj As RObject) As Boolean()
            Return CLRVector.asLogical(robj.value.data)
        End Function

        ''' <summary>
        ''' read R vector in any element data type
        ''' </summary>
        ''' <param name="robj"></param>
        ''' <returns></returns>
        Public Shared Function ReadVector(robj As RObject) As Array
            Select Case robj.info.type
                Case RObjectType.CHAR : Return ReadString(robj).ToArray
                Case RObjectType.STR : Return ReadStrings(robj)
                Case RObjectType.INT : Return ReadIntegers(robj)
                Case RObjectType.REAL : Return ReadNumbers(robj)
                Case RObjectType.LGL : Return ReadLogicals(robj)
                Case RObjectType.CPLX
                    ' complex vector: data is already a Complex() array
                    Return DirectCast(robj.value.data, Complex())
                Case RObjectType.RAW
                    ' raw bytes vector: data is an Integer()/Int32() array of 0-255
                    ' values produced by parseVector(AddressOf parse_byte); convert to Byte().
                    Return CLRVector.asRawByte(robj.value.data)
                Case Else
                    Throw New NotImplementedException(robj.info.ToString)
            End Select
        End Function

        Public Shared Function ReadStrings(robj As Object) As String()
            If TypeOf robj Is RList Then
                Dim rlist As RList = DirectCast(robj, RList)

                If rlist.nodeType = ListNodeType.LinkedList Then
                    Return ReadStrings(rlist.CAR)
                Else
                    Return DirectCast(rlist.data, RObject()) _
                        .Select(AddressOf ReadString) _
                        .ToArray
                End If

            ElseIf DirectCast(robj, RObject).info.type = RObjectType.LIST Then
                Return ReadStrings(DirectCast(robj, RObject).value)
            Else
                Dim obj As RObject = DirectCast(robj, RObject)

                If obj.info.type = RObjectType.STR Then
                    Return DirectCast(obj.value.data, RObject()) _
                        .Select(AddressOf ReadString) _
                        .ToArray
                Else
                    Return {ReadString(obj)}
                End If
            End If
        End Function
    End Class
End Namespace
