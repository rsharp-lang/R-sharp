#Region "Microsoft.VisualBasic::6ba3c263287c576d1ec98e7319e96aee, R#\Interpreter\ExecuteEngine\Linq\Expression\LinqKeywordExpression.vb"

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

    '   Total Lines: 20
    '    Code Lines: 12
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 566 B


    '     Class LinqKeywordExpression
    ' 
    '         Properties: name
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq expression of a specific keyword 
    ''' </summary>
    Public MustInherit Class LinqKeywordExpression : Inherits Expression

        Public MustOverride ReadOnly Property keyword As String

        Public Overrides ReadOnly Property name As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return keyword
            End Get
        End Property

    End Class
End Namespace
