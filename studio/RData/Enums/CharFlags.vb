#Region "Microsoft.VisualBasic::fc7fc4254fd41fa21cacdab3adb8be3e, studio\RData\Enums\CharFlags.vb"

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

' Enum CharFlags
' 
' 
'  
' 
' 
' 
' /********************************************************************************/

#End Region

Namespace Flags

    Public Enum CharFlags
        HAS_HASH = 1
        BYTES = 1 << 1
        ''' <summary>
        ''' const LATIN1_MASK  = (1&lt;&lt;2);
        ''' </summary>
        LATIN1 = 1 << 2
        ''' <summary>
        ''' const UTF8_MASK    = (1&lt;&lt;3);
        ''' </summary>
        UTF8 = 1 << 3
        CACHED = 1 << 5

        ''' <summary>
        ''' const ASCII_MASK   = (1&lt;&lt;6);
        ''' </summary>
        ASCII = 1 << 6
    End Enum
End Namespace