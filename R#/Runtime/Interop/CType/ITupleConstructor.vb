#Region "Microsoft.VisualBasic::1a3e021f6b4fb8c6a34593f2030b9274, R#\Runtime\Interop\CType\ITupleConstructor.vb"

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

    '     Interface ITupleConstructor
    ' 
    '         Function: checkTuple, getByIndex, getByName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop.CType

    Public Interface ITupleConstructor

        Function getByName(name As String) As Object
        Function getByIndex(i As Integer) As Object
        Function checkTuple(names As String()) As Boolean

    End Interface
End Namespace
