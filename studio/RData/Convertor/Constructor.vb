#Region "Microsoft.VisualBasic::274df2f9b8e1a43c560c1e1e5f663096, G:/GCModeller/src/R-sharp/studio/RData//Convertor/Constructor.vb"

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

    '   Total Lines: 115
    '    Code Lines: 89
    ' Comment Lines: 6
    '   Blank Lines: 20
    '     File Size: 4.06 KB


    '     Module Constructor
    ' 
    '         Function: DecodeCharacters, GetRType, LinkValue, LinkVisitor
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

    Public Module Constructor

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
            If r_char.value Is Nothing OrElse r_char.value.data.IsNullOrEmpty Then
                Return ""
            ElseIf r_char.info.type <> RObjectType.CHAR Then
                Return ""
            End If

            If TypeOf r_char.value.data Is Char() Then
                Return New String(DirectCast(r_char.value.data, Char()))
            End If

            Dim bytes As Byte() = DirectCast(r_char.value.data, Byte())
            Dim encoding As Encoding = Encoding.UTF8

            If r_char.info.gp And CharFlags.UTF8 Then
                encoding = Encoding.UTF8
            ElseIf r_char.info.gp And CharFlags.LATIN1 Then
#If NETCOREAPP Then
                encoding = Encoding.GetEncoding("Latin1")
#Else
#If Not NET48 Then
                encoding = Encoding.Latin1
#Else
                encoding = Encoding.GetEncoding("Latin1")
#End If
#End If
            ElseIf r_char.info.gp And CharFlags.ASCII Then
                encoding = Encoding.ASCII
            ElseIf r_char.info.gp And CharFlags.BYTES Then
                encoding = Encoding.ASCII
            End If

            Return encoding.GetString(bytes)
        End Function

        ''' <summary>
        ''' 基于访问CDR链表来进行对象查找
        ''' </summary>
        ''' <param name="robj"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LinkVisitor(robj As RObject, key As String) As RObject
            Dim tag As RObject

            Do While Not robj Is Nothing
                tag = robj.tag

                If tag Is Nothing AndAlso robj.referenced_object Is Nothing Then
                    Return Nothing
                End If

                If tag IsNot Nothing Then
                    If tag.characters = key Then
                        Return robj
                    End If

                    If tag.referenced_object IsNot Nothing AndAlso tag.referenced_object.characters = key Then
                        Return robj
                    End If
                End If

                robj = robj.value.CDR
            Loop

            Return Nothing
        End Function

        <Extension>
        Public Function LinkValue(robj As RObject, key As String) As RObject
            Dim target As RObject = robj.LinkVisitor(key)

            If target Is Nothing Then
                Return Nothing
            Else
                Return target.value.CAR
            End If
        End Function
    End Module
End Namespace
