#Region "Microsoft.VisualBasic::fd084ee23e0e32ee16f427d5b3d29768, R#\Runtime\Interop\RMethodArgument.vb"

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
'     Class RArgumentList
' 
'         Function: CreateArguments
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Runtime.Components

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
            If isOptional Then
                Return $"``{name}`` as {type} = {If(type.mode = TypeCodes.boolean, [default].ToString.ToUpper, Scripting.ToString([default], "NULL"))}"
            Else
                Return $"``{name}`` as {type}"
            End If
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

    Public Class RArgumentList

        ''' <summary>
        ''' Create argument value for <see cref="MethodInfo.Invoke(Object, Object())"/>
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function CreateArguments() As IEnumerable(Of Object)

        End Function
    End Class
End Namespace
