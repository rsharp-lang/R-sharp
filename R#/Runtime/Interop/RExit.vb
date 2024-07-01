#Region "Microsoft.VisualBasic::9ee72306306a1d6a637793008f0bd827, R#\Runtime\Interop\RExit.vb"

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

    '   Total Lines: 23
    '    Code Lines: 11 (47.83%)
    ' Comment Lines: 7 (30.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (21.74%)
    '     File Size: 567 B


    '     Class RExit
    ' 
    '         Properties: exit_code
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' A signal object for make R# exit the script executation
    ''' </summary>
    Public Class RExit

        ''' <summary>
        ''' the program exit status code
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property exit_code As Integer

        Sub New(status As Integer)
            exit_code = status
        End Sub

        Public Overrides Function ToString() As String
            Return $"exit({exit_code});"
        End Function

    End Class
End Namespace
