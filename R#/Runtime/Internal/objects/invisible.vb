#Region "Microsoft.VisualBasic::7fb8c11ff707e6101654e10296a07233, R#\Runtime\Internal\objects\invisible.vb"

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

    '   Total Lines: 37
    '    Code Lines: 17 (45.95%)
    ' Comment Lines: 13 (35.14%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 7 (18.92%)
    '     File Size: 1.04 KB


    '     Class invisible
    ' 
    '         Properties: value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: NULL, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' do not print the result object on the console
    ''' unless an explicit call of the ``print`` or ``cat``
    ''' function have been invoke
    ''' </summary>
    Public Class invisible

        Public Property value As Object

        ''' <summary>
        ''' construct a new invisible value object
        ''' </summary>
        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return any.ToString(value, "NULL")
        End Function

        ''' <summary>
        ''' invisible(NULL);
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function NULL() As invisible
            Return New invisible
        End Function

    End Class
End Namespace
