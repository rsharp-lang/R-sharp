#Region "Microsoft.VisualBasic::4194f215bd011de70ec38ef94ea16027, R#\ListVector.vb"

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

    '   Total Lines: 34
    '    Code Lines: 30 (88.24%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (11.76%)
    '     File Size: 1.25 KB


    ' Module ListVector
    ' 
    '     Function: as_character, as_numeric
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Module ListVector

    <Extension>
    Public Function as_character(list As list) As Dictionary(Of String, String)
        If list Is Nothing OrElse list.slots.IsNullOrEmpty Then
            Return New Dictionary(Of String, String)
        Else
            Return list.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CLRVector.asScalarCharacter(a.Value)
                              End Function)
        End If
    End Function

    <Extension>
    Public Function as_numeric(list As list) As Dictionary(Of String, Double)
        If list Is Nothing OrElse list.slots.IsNullOrEmpty Then
            Return New Dictionary(Of String, Double)
        Else
            Return list.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CLRVector.asNumeric(a.Value).DefaultFirst
                              End Function)
        End If
    End Function

End Module
