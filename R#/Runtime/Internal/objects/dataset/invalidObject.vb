#Region "Microsoft.VisualBasic::0a924f029bd46be80534af0762582c6b, R#\Runtime\Internal\objects\dataset\invalidObject.vb"

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

    '   Total Lines: 40
    '    Code Lines: 24 (60.00%)
    ' Comment Lines: 7 (17.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (22.50%)
    '     File Size: 1.09 KB


    '     Class invalidObject
    ' 
    '         Properties: literalAny, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' NA literal value
    ''' </summary>
    Public Class invalidObject

        ''' <summary>
        ''' a single object across the entire R# runtime environment
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property value As New invalidObject

        Public ReadOnly Property literalAny As Object
            Get
                Return Nothing
            End Get
        End Property

        Private Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return "NA"
        End Function

        Public Shared Narrowing Operator CType(na As invalidObject) As Double
            Return Double.NaN
        End Operator

        Public Shared Narrowing Operator CType(na As invalidObject) As Integer
            Return Integer.MinValue
        End Operator

        Public Shared Narrowing Operator CType(na As invalidObject) As String
            Return "NA"
        End Operator

    End Class
End Namespace
