#Region "Microsoft.VisualBasic::2cef543dfab0d2c02d7126e09e2528f5, R#\runtime\PrimitiveTypes\uinteger.vb"

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

    '     Class [uinteger]
    ' 
    '         Function: ToString
    ' 
    '         Sub: New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.uinteger"/>
    ''' </summary>
    Public Class [uinteger] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.uinteger, GetType(ULong))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# uinteger"
        End Function
    End Class
End Namespace
