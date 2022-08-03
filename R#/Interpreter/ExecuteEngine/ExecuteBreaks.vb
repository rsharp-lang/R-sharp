#Region "Microsoft.VisualBasic::5d8652976d32bce574f696a960034b23, R-sharp\R#\Interpreter\ExecuteEngine\ExecuteBreaks.vb"

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
    '    Code Lines: 7
    ' Comment Lines: 12
    '   Blank Lines: 1
    '     File Size: 536 B


    '     Enum ExecuteBreaks
    ' 
    '         BreakLoop, ContinuteNext, ReturnValue
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Enumerations of breaks the executation of the current code closure.
    ''' </summary>
    Public Enum ExecuteBreaks
        ''' <summary>
        ''' returns value for function
        ''' </summary>
        ReturnValue
        ''' <summary>
        ''' continute to next for/while/loop
        ''' </summary>
        ContinuteNext
        ''' <summary>
        ''' break of while/loop
        ''' </summary>
        BreakLoop
    End Enum
End Namespace
