#Region "Microsoft.VisualBasic::401a1dd466b35297075b9114ca4c3a02, R-sharp\R#\Runtime\Internal\objects\Linq\RCType.vb"

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

        Total Lines:   36
        Code Lines:    28
        Comment Lines: 0
        Blank Lines:   8
        File Size:     1.17 KB


    '     Module RCType
    ' 
    '         Function: ascharacter, asinteger, aslogical, asnumeric, safeCharacters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

<Assembly: InternalsVisibleTo("igraph")>

Namespace Runtime.Internal.Object.Linq

    Module RCType

        <Extension>
        Public Function aslogical(vec As vector) As Boolean()
            Return REnv.asVector(Of Boolean)(vec.data)
        End Function

        <Extension>
        Public Function ascharacter(vec As vector) As String()
            Return REnv.asVector(Of String)(vec.data)
        End Function

        Public Function safeCharacters(x As Object) As String()
            Return REnv.asVector(Of String)(x).AsObjectEnumerator.Select(Function(i) If(i Is Nothing, "", any.ToString(i))).ToArray
        End Function

        <Extension>
        Public Function asnumeric(vec As vector) As Double()
            Return REnv.asVector(Of Double)(vec.data)
        End Function

        <Extension>
        Public Function asinteger(vec As vector) As Long()
            Return REnv.asVector(Of Long)(vec.data)
        End Function
    End Module
End Namespace
