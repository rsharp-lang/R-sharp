#Region "Microsoft.VisualBasic::056cc155d3678a5e2fe082937ee5d70d, R#\Runtime\Interop\RMethodArgument.vb"

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

    '     Class RMethodArgument
    ' 
    '         Properties: [default], isObjectList, isOptional, isRequireRawVector, name
    '                     type
    ' 
    '         Function: ParseArgument, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Runtime.Interop

    Public Class RMethodArgument : Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property type As RType
        Public Property [default] As Object
        Public Property isOptional As Boolean
        Public Property isObjectList As Boolean
        ''' <summary>
        ''' Do not apply the <see cref="Runtime.getFirst(Object)"/> operation
        ''' </summary>
        ''' <returns></returns>
        Public Property isRequireRawVector As Boolean

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function

        Public Shared Function ParseArgument(p As ParameterInfo) As RMethodArgument
            Return New RMethodArgument With {
                .name = p.Name,
                .type = New RType(p.ParameterType),
                .[default] = p.DefaultValue,
                .isOptional = p.HasDefaultValue,
                .isObjectList = Not p.GetCustomAttribute(Of RListObjectArgumentAttribute) Is Nothing,
                .isRequireRawVector = Not p.GetCustomAttribute(Of RRawVectorArgumentAttribute) Is Nothing
            }
        End Function
    End Class
End Namespace
