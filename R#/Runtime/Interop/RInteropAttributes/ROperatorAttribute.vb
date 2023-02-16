#Region "Microsoft.VisualBasic::27b750bc015b0ca54a332385352177fe, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/ROperatorAttribute.vb"

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

    '   Total Lines: 21
    '    Code Lines: 12
    ' Comment Lines: 4
    '   Blank Lines: 5
    '     File Size: 576 B


    '     Class ROperatorAttribute
    ' 
    '         Properties: [operator]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class ROperatorAttribute : Inherits Attribute

        Public ReadOnly Property [operator] As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[operator]"></param>
        Sub New([operator] As String)
            Me.[operator] = [operator]
        End Sub

        Public Overrides Function ToString() As String
            Return [operator]
        End Function

    End Class
End Namespace
