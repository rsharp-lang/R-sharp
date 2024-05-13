#Region "Microsoft.VisualBasic::38200e924b7841a0d0d5e46dba67b9f8, R#\Interpreter\DebugLevels.vb"

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

    '   Total Lines: 24
    '    Code Lines: 8
    ' Comment Lines: 15
    '   Blank Lines: 1
    '     File Size: 697 B


    '     Enum DebugLevels
    ' 
    '         Memory, None, Stack
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter

    ''' <summary>
    ''' debug echo verbose level for the R# interpreter engine
    ''' </summary>
    Public Enum DebugLevels As Byte
        ''' <summary>
        ''' no debug echo, normal mode(or production mode)
        ''' </summary>
        None
        ''' <summary>
        ''' debug echo of the memory delta change after execute each expression
        ''' </summary>
        Memory
        ''' <summary>
        ''' print the expression executation stack data
        ''' </summary>
        Stack
        ''' <summary>
        ''' print all data on the console
        ''' </summary>
        All = Memory Or Stack
    End Enum
End Namespace
