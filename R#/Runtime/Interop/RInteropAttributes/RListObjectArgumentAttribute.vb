#Region "Microsoft.VisualBasic::42407dc47e0653c21e759ca44555c994, R#\Runtime\Interop\RInteropAttributes\RListObjectArgumentAttribute.vb"

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

    '     Class RListObjectArgumentAttribute
    ' 
    '         Function: getObjectList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    ''' <summary>
    ''' 表示当前的函数参数为一个 ``...`` 可以产生一个字典list对象值的参数列表
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RListObjectArgumentAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' Safe get a collection of argument name and value tuple
        ''' </summary>
        ''' <param name="objects"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Shared Iterator Function getObjectList(<RListObjectArgument> objects As Object, envir As Environment) As IEnumerable(Of NamedValue(Of Object))
            Dim type As Type

            If objects Is Nothing Then
                Return
            Else
                type = objects.GetType
            End If

            If type Is GetType(List) Then
                objects = DirectCast(objects, List).slots
                type = GetType(Dictionary(Of String, Object))
            End If

            If type Is GetType(Dictionary(Of String, Object)) Then
                For Each item In DirectCast(objects, Dictionary(Of String, Object))
                    Yield New NamedValue(Of Object) With {
                        .Name = item.Key,
                        .Value = item.Value
                    }
                Next
            ElseIf type Is GetType(InvokeParameter()) Then
                For Each item As InvokeParameter In DirectCast(objects, InvokeParameter())
                    Yield New NamedValue(Of Object) With {
                        .Name = item.name,
                        .Value = item.Evaluate(envir)
                    }
                Next
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Class
End Namespace
