#Region "Microsoft.VisualBasic::7d7e1393d339c942c87de7937571d9dc, studio\RData\Models\RObject.vb"

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

' Class RObject
' 
'     Properties: attributes, info, referenced_object, tag, value
' 
'     Function: ToString
' 
' /********************************************************************************/

#End Region

Imports System.Text
''' <summary>
''' Representation of a R object.
''' </summary>
Public Class RObject

    Public Property info As RObjectInfo
    Public Property value As Object
    Public Property attributes As RObject
    Public Property tag As RObject
    Public Property referenced_object As RObject

    Public ReadOnly Property characters As String
        Get
            If info.type = RObjectType.SYM Then
                Return value?.ToString
            Else
                Return DecodeCharacters
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        If info.type = RObjectType.CHAR Then
            Return characters
        Else
            Return $"[{info}] {value}"
        End If
    End Function
End Class
