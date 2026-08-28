#Region "Microsoft.VisualBasic::33b7db9f1b251dec1fd766f12e8d2f2b, studio\RData\Convertor\AltRepConstructor.vb"

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

    '   Total Lines: 122
    '    Code Lines: 66 (54.10%)
    ' Comment Lines: 38 (31.15%)
    '    - Xml Docs: 84.21%
    ' 
    '   Blank Lines: 18 (14.75%)
    '     File Size: 5.35 KB


    '     Delegate Function
    ' 
    ' 
    '     Module AltRepConstructorExpander
    ' 
    '         Function: compact_intseq_constructor, compact_realseq_constructor, deferred_string_constructor, makeVector, wrap_constructor
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Convertor

    ''' <summary>
    ''' Expand an ALTREP object into a plain R vector.
    ''' </summary>
    ''' <param name="info">
    ''' The ALTREP <see cref="RObject"/> whose <see cref="RObject.value"/>.data[0] is
    ''' the class symbol (e.g. "compact_intseq").
    ''' </param>
    ''' <param name="state">
    ''' The ALTREP state object read immediately after the class symbol. This carries
    ''' the compact representation data (start/step/n for sequences, the backing
    ''' character vector for deferred strings, or the wrapped real object for wrap_*).
    ''' </param>
    ''' <returns>
    ''' A tuple of the expanded vector's <see cref="RObjectInfo"/> and its materialized
    ''' data array. The data array must be in the same shape as produced by ReadVector.
    ''' </returns>
    Public Delegate Function AltRepConstructor(info As RObject, state As RObject) As (info As RObjectInfo, value As Object)

    Public Module AltRepConstructorExpander

        ''' <summary>
        ''' compact_intseq: state is a REALSXP of length 3 -> (n, step, start).
        ''' Produces integer(start .. start + (n-1)*step).
        ''' </summary>
        Public Function compact_intseq_constructor(info As RObject, state As RObject) As (RObjectInfo, Object)
            Dim values As Double() = DirectCast(state.value.data, Double())
            Dim n As Integer = CInt(values(0))
            Dim start As Integer = CInt(values(1))
            Dim [step] As Integer = CInt(values(2))
            Dim buffer As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                buffer(i) = start + i * [step]
            Next

            Return makeVector(RObjectType.INT, buffer)
        End Function

        ''' <summary>
        ''' compact_realseq: state is a REALSXP of length 3 -> (n, step, start).
        ''' Produces double(start .. start + (n-1)*step).
        ''' </summary>
        Public Function compact_realseq_constructor(info As RObject, state As RObject) As (RObjectInfo, Object)
            Dim values As Double() = DirectCast(state.value.data, Double())
            Dim n As Double = values(0)
            Dim start As Double = values(1)
            Dim [step] As Double = values(2)
            Dim length As Integer = CInt(n)
            Dim buffer As Double() = New Double(length - 1) {}

            For i As Integer = 0 To length - 1
                buffer(i) = start + i * [step]
            Next

            Return makeVector(RObjectType.REAL, buffer)
        End Function

        ''' <summary>
        ''' deferred_string: the state holds the materialized backing character vector
        ''' (an STRSXP). We return that vector directly.
        ''' </summary>
        Public Function deferred_string_constructor(info As RObject, state As RObject) As (RObjectInfo, Object)
            ' The deferred_string state is a list (VECSXP) whose first element is the
            ' backing STRSXP data buffer. Fall back to the whole state if it already is
            ' a plain character vector.
            Dim dataObj As RObject = state

            If state.info.type = RObjectType.VEC AndAlso state.value IsNot Nothing AndAlso state.value.data IsNot Nothing Then
                Dim elements As RObject() = TryCast(state.value.data, RObject())

                If elements IsNot Nothing Then
                    For Each e In elements
                        If e IsNot Nothing AndAlso e.info.type = RObjectType.STR Then
                            dataObj = e
                            Exit For
                        End If
                    Next
                End If
            End If

            If dataObj.value Is Nothing OrElse dataObj.value.data Is Nothing Then
                ' No usable backing data, return an empty character vector.
                Return makeVector(RObjectType.STR, New String() {})
            End If

            Return (dataObj.info, dataObj.value.data)
        End Function

        ''' <summary>
        ''' wrap_*: the state IS the real materialized object. Return it as-is so the
        ''' conversion pipeline handles it through the normal read path.
        ''' </summary>
        Public Function wrap_constructor(info As RObject, state As RObject) As (RObjectInfo, Object)
            If state.value Is Nothing OrElse state.value.data Is Nothing Then
                ' Wrapped object may be a list/VECSXP expressed as CAR/CDR; surface it
                ' through the plain object value so callers can recurse.
                Return (state.info, state.value)
            End If

            Return (state.info, state.value.data)
        End Function

        Private Function makeVector(type As RObjectType, value As Object) As (RObjectInfo, Object)
            Dim info As New RObjectInfo With {
                .type = type,
                .reference = -1,
                .gp = 0,
                .[object] = False,
                .attributes = False,
                .tag = False
            }

            Return (info, value)
        End Function
    End Module
End Namespace
