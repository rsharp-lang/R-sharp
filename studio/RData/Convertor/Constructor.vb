#Region "Microsoft.VisualBasic::a5de4337296f89b27c47936066822d36, studio\RData\Convertor\Constructor.vb"

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

' Module Constructor
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Convertor

    Module Constructor

        Public ReadOnly wrap_constructor As AltRepConstructor
        Public ReadOnly compact_realseq_constructor As AltRepConstructor
        Public ReadOnly compact_intseq_constructor As AltRepConstructor
        Public ReadOnly deferred_string_constructor As AltRepConstructor

        ReadOnly toType As New Dictionary(Of RObjectType, RType) From {
            {RObjectType.ANY, RType.GetRSharpType(GetType(Object))},
            {RObjectType.CHAR, RType.GetRSharpType(GetType(Char))},
            {RObjectType.REAL, RType.GetRSharpType(GetType(Double))},
            {RObjectType.LGL, RType.GetRSharpType(GetType(Boolean))},
            {RObjectType.STR, RType.GetRSharpType(GetType(String))},
            {RObjectType.INT, RType.GetRSharpType(GetType(Integer))}
        }

        <Extension>
        Public Function GetRType(meta As RObjectInfo) As RType
            If toType.ContainsKey(meta.type) Then
                Return toType(meta.type)
            End If

            Throw New NotImplementedException(meta.ToString)
        End Function

        <Extension>
        Public Function DecodeCharacters(r_char As RObject) As String
            If r_char.value Is Nothing Then
                Return ""
            ElseIf r_char.info.type = RObjectType.CHAR Then
                Dim bytes As Byte() = DirectCast(r_char.value.data, Byte())
                Dim encoding As Encoding = Encoding.UTF8

                If r_char.info.gp And CharFlags.UTF8 Then
                    encoding = Encoding.UTF8
                ElseIf r_char.info.gp And CharFlags.LATIN1 Then
                    encoding = Encoding.Latin1
                ElseIf r_char.info.gp And CharFlags.ASCII Then
                    encoding = Encoding.ASCII
                ElseIf r_char.info.gp And CharFlags.BYTES Then
                    encoding = Encoding.ASCII
                End If

                Return encoding.GetString(bytes)
            Else
                Return ""
            End If
        End Function
    End Module
End Namespace