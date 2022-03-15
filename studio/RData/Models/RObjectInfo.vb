#Region "Microsoft.VisualBasic::ee95beb2491711199fe1818828a43100, R-sharp\studio\RData\Models\RObjectInfo.vb"

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


     Code Statistics:

        Total Lines:   78
        Code Lines:    62
        Comment Lines: 3
        Blank Lines:   13
        File Size:     2.58 KB


    '     Class RObjectInfo
    ' 
    '         Properties: INTSXP, LGLSXP, LISTSXP, NILVALUESXP, REALSXP
    '                     STRSXP, SYMSXP
    ' 
    '         Function: primitiveType, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RDataSet.Flags

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

        Public Shared ReadOnly Property STRSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.STR)
            End Get
        End Property

        Public Shared ReadOnly Property REALSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.REAL)
            End Get
        End Property

        Public Shared ReadOnly Property INTSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.INT)
            End Get
        End Property

        Public Shared ReadOnly Property LGLSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.LGL)
            End Get
        End Property

        Public Shared ReadOnly Property LISTSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.LIST, has_tag:=True)
            End Get
        End Property

        Public Shared ReadOnly Property SYMSXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.SYM)
            End Get
        End Property

        Public Shared ReadOnly Property NILVALUESXP As RObjectInfo
            Get
                Return primitiveType(RObjectType.NILVALUE)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"RObjectInfo(type=<{type.ToString}: {CInt(type)}>, object={[object]}, attributes={attributes}, tag={tag}, gp={gp}, reference={reference})"
        End Function

        Public Shared Function primitiveType(baseType As RObjectType,
                                             Optional has_tag As Boolean = False,
                                             Optional is_object As Boolean = False,
                                             Optional has_attributes As Boolean = False) As RObjectInfo
            Return New RObjectInfo With {
                .type = baseType,
                .attributes = has_attributes,
                .gp = 0,
                .[object] = is_object,
                .reference = 0,
                .tag = has_tag
            }
        End Function

    End Class
End Namespace
