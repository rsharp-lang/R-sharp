#Region "Microsoft.VisualBasic::f3590cd0fa89ae7ed33f73d5cd798cd9, R#\Runtime\Interop\RInteropAttribute.vb"

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

    '     Class RInteropAttribute
    ' 
    ' 
    ' 
    '     Class RByRefValueAssignAttribute
    ' 
    ' 
    ' 
    '     Class RRawVectorArgumentAttribute
    ' 
    ' 
    ' 
    '     Class RListObjectArgumentAttribute
    ' 
    '         Function: getObjectList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' 这个参数是接受``a(x) &lt;- y``操作之中的``y``结果值的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RByRefValueAssignAttribute : Inherits RInteropAttribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RRawVectorArgumentAttribute : Inherits RInteropAttribute
    End Class

    ''' <summary>
    ''' The return value will not be print on the console
    ''' </summary>
    <AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Public Class RSuppressPrintAttribute : Inherits RInteropAttribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RListObjectArgumentAttribute : Inherits RInteropAttribute

        Public Shared Iterator Function getObjectList(<RListObjectArgument> objects As Object, envir As Environment) As IEnumerable(Of NamedValue(Of Object))
            Dim type As Type

            If objects Is Nothing Then
                Return
            Else
                type = objects.GetType
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
