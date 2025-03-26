#Region "Microsoft.VisualBasic::97b7744717fcee331826d65c6b34d0ea, R#\Runtime\Internal\objects\dataset\generator.vb"

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

    '   Total Lines: 22
    '    Code Lines: 13 (59.09%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 586 B


    '     Class generator
    ' 
    '         Properties: current, fun
    ' 
    '         Function: moveNext
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' A generator function wrapper
    ''' </summary>
    Public Class generator : Inherits RsharpDataObject

        Public Property fun As RFunction
        Public ReadOnly Property current As Object

        Dim generator As IEnumerator

        Public Function moveNext() As Boolean
            _current = generator.Current
            Return generator.MoveNext
        End Function

    End Class
End Namespace
