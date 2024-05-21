#Region "Microsoft.VisualBasic::c95ce4d8bdb74b6f2e03e5072b9fc524, studio\RData\Models\RExtraInfo.vb"

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

    '   Total Lines: 16
    '    Code Lines: 8 (50.00%)
    ' Comment Lines: 5 (31.25%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 3 (18.75%)
    '     File Size: 406 B


    '     Class RExtraInfo
    ' 
    '         Properties: encoding
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Struct

    ''' <summary>
    ''' Extra information.
    '''
    ''' Contains the Default encoding (only In version 3).
    ''' </summary>
    Public Class RExtraInfo

        Public Property encoding As String = Nothing

        Public Overrides Function ToString() As String
            Return $"RExtraInfo(encoding='{encoding}')"
        End Function
    End Class
End Namespace
