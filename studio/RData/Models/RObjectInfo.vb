#Region "Microsoft.VisualBasic::865b111825e997c0b93c20c4840d267d, studio\RData\Models\RObjectInfo.vb"

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

' Class RObjectInfo
' 
'     Function: ToString
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RData.Flags

Namespace Struct

    ''' <summary>
    ''' Internal attributes of a R object.
    ''' </summary>
    Public Class RObjectInfo

        Public type As RObjectType
        Public [object] As Boolean
        Public attributes As Boolean
        Public tag As Boolean
        Public gp As Integer
        Public reference As Integer

        Public Overrides Function ToString() As String
            Return $"RObjectInfo(type=<{type.ToString}: {CInt(type)}>, object={[object]}, attributes={attributes}, tag={tag}, gp={gp}, reference={reference})"
        End Function

    End Class
End Namespace