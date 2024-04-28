#Region "Microsoft.VisualBasic::f405e1d23702bf20de34ea16e86e0d95, E:/GCModeller/src/R-sharp/R#//Runtime/System/Variant.vb"

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

    '   Total Lines: 25
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 786 B


    '     Class [Variant]
    ' 
    '         Properties: CanbeCastTo
    ' 
    '         Function: [TryCast]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language

Namespace Runtime.Components

    Public Class [Variant](Of T) : Inherits Value(Of Object)

        Public ReadOnly Property CanbeCastTo As Boolean
            Get
                Return GetUnderlyingType() Is GetType(T) OrElse
                    GetUnderlyingType.IsInheritsFrom(GetType(T)) OrElse
                    (GetType(T).IsInterface AndAlso GetUnderlyingType.ImplementInterface(GetType(T)))
            End Get
        End Property

        Public Function [TryCast]() As T
            If CanbeCastTo Then
                Return DirectCast(Value, T)
            Else
                Return Nothing
            End If
        End Function

    End Class
End Namespace
