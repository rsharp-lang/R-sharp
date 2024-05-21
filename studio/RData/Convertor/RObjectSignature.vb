#Region "Microsoft.VisualBasic::eacc443e4adca3b7e57e0d44ab2a2e4d, studio\RData\Convertor\RObjectSignature.vb"

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

    '   Total Lines: 64
    '    Code Lines: 48 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 16 (25.00%)
    '     File Size: 2.00 KB


    '     Class RObjectSignature
    ' 
    '         Function: HasFactor, IsDataFrame, IsPairList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Convertor

    Public Class RObjectSignature

        Public Shared Function IsPairList(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                If robj.info.type Like ConvertToR.elementVectorFlags Then
                    Return False
                Else
                    Return True
                End If
            End If

            If attrs.LinkVisitor("row.names") IsNot Nothing Then
                Return False
            End If

            Return True
        End Function

        Public Shared Function IsDataFrame(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                Return False
            ElseIf robj.info.type Like ConvertToR.elementVectorFlags Then
                Return False
            End If

            If attrs.tag.referenced_object IsNot Nothing AndAlso attrs.tag.referenced_object.characters = "names" Then
                Return True
            End If

            Return True
        End Function

        Public Shared Function HasFactor(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                Return False
            End If

            If attrs.tag.info.type <> RObjectType.SYM AndAlso attrs.tag.info.type <> RObjectType.REF Then
                Return False
            End If

            If attrs.tag.info.type = RObjectType.SYM AndAlso attrs.tag.characters <> "levels" Then
                Return False
            End If
            If attrs.tag.info.type = RObjectType.REF AndAlso attrs.tag.referenced_object.characters <> "levels" Then
                Return False
            End If

            Return True
        End Function

    End Class
End Namespace
