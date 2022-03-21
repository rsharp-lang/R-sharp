#Region "Microsoft.VisualBasic::0e1a9af209d5afbcf44c8624ee8ead70, R-sharp\studio\RData\Models\LinkedList\RObject.vb"

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

    '   Total Lines: 55
    '    Code Lines: 46
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 1.90 KB


    '     Class RObject
    ' 
    '         Properties: attributes, characters, info, referenced_object, symbolName
    '                     tag, value
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Flags

Namespace Struct.LinkedList

    ''' <summary>
    ''' Representation of a R object.
    ''' </summary>
    Public Class RObject

        Public Property info As RObjectInfo
        Public Property value As RList
        Public Property attributes As RObject
        Public Property tag As RObject
        Public Property referenced_object As RObject

        Public ReadOnly Property symbolName As String
            Get
                If Not tag Is Nothing Then
                    Return If(tag.symbolName.StringEmpty, tag.characters, tag.symbolName)
                ElseIf Not referenced_object Is Nothing Then
                    If referenced_object.symbolName.StringEmpty Then
                        Return referenced_object.characters
                    Else
                        Return referenced_object.symbolName
                    End If
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property characters As String
            Get
                If info.type = RObjectType.SYM Then
                    Return value?.ToString
                Else
                    Return DecodeCharacters()
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            If info.type = RObjectType.CHAR Then
                Return characters
            ElseIf tag IsNot Nothing AndAlso tag.info.type = RObjectType.SYM Then
                Return tag.characters
            ElseIf referenced_object IsNot Nothing Then
                Return referenced_object.characters
            Else
                Return $"[{info}] {value}"
            End If
        End Function
    End Class
End Namespace
