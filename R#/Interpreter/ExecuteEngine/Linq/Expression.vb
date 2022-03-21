#Region "Microsoft.VisualBasic::780fd0d83d8daa3462eeb9d2d488f1b4, R-sharp\R#\Interpreter\ExecuteEngine\Linq\Expression.vb"

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

    '   Total Lines: 18
    '    Code Lines: 6
    ' Comment Lines: 8
    '   Blank Lines: 4
    '     File Size: 493.00 B


    '     Class Expression
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the Linq expression
    ''' </summary>
    Public MustInherit Class Expression

        Public MustOverride ReadOnly Property name As String

        ''' <summary>
        ''' Evaluate the expression
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Public MustOverride Function Exec(context As ExecutableContext) As Object

    End Class
End Namespace
