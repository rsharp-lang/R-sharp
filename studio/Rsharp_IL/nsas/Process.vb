#Region "Microsoft.VisualBasic::95f8ac023cd8649fe90ae98999c6b3f8, studio\Rsharp_IL\nsas\Process.vb"

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

    '   Total Lines: 41
    '    Code Lines: 23
    ' Comment Lines: 7
    '   Blank Lines: 11
    '     File Size: 899 B


    ' Class Process
    ' 
    '     Properties: Parameters
    ' 
    ' Enum ProcessOperation
    ' 
    '     IMPORT, PRINT
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PrintProcess
    ' 
    '     Properties: Operation
    ' 
    ' Class ImportProcess
    ' 
    '     Properties: Operation
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' PROC keyword
''' </summary>
Public MustInherit Class Process

    ''' <summary>
    ''' IMPORT/PRINT
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride ReadOnly Property Operation As ProcessOperation

    Public Property Parameters As NamedValue(Of String)()

End Class

Public Enum ProcessOperation
    IMPORT
    PRINT
End Enum

Public Class PrintProcess : Inherits Process

    Public Overrides ReadOnly Property Operation As ProcessOperation
        Get
            Return ProcessOperation.PRINT
        End Get
    End Property

End Class

Public Class ImportProcess : Inherits Process

    Public Overrides ReadOnly Property Operation As ProcessOperation
        Get
            Return ProcessOperation.IMPORT
        End Get
    End Property

End Class
