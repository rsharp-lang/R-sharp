#Region "Microsoft.VisualBasic::2e607b978bf7f2f8d957a6ce5587d467, R#\Runtime\Vectorization\typedefine.vb"

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

    '   Total Lines: 26
    '    Code Lines: 12
    ' Comment Lines: 10
    '   Blank Lines: 4
    '     File Size: 721 B


    '     Class typedefine
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Vectorization

    ''' <summary>
    ''' Type cache module
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public NotInheritable Class typedefine(Of T)

        ''' <summary>
        ''' The vector based type(type of scaler value)
        ''' </summary>
        Public Shared ReadOnly baseType As Type
        ''' <summary>
        ''' The abstract vector type(array, list, collection, etc)
        ''' </summary>
        Public Shared ReadOnly enumerable As Type

        Private Sub New()
        End Sub

        Shared Sub New()
            baseType = GetType(T)
            enumerable = GetType(IEnumerable(Of T))
        End Sub
    End Class
End Namespace
