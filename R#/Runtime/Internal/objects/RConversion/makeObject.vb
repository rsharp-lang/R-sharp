#Region "Microsoft.VisualBasic::2b7ef65303067392a281ef0785e9c35b, R-sharp\R#\Runtime\Internal\objects\RConversion\makeObject.vb"

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


     Code Statistics:

        Total Lines:   43
        Code Lines:    34
        Comment Lines: 1
        Blank Lines:   8
        File Size:     1.61 KB


    '     Module makeObject
    ' 
    '         Function: createObject, isObjectConversion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Runtime.Internal.Object.Converts

    Module makeObject

        Public Function isObjectConversion(type As Type, val As Object) As Boolean
            If Not TypeOf val Is list Then
                Return False
            Else
                ' only works for the type which have non-parameter constructor
                If (Not DataFramework.IsPrimitive(type)) Then
                    Return type.GetConstructors.Any(Function(cor) cor.GetParameters.IsNullOrEmpty)
                Else
                    Return False
                End If
            End If
        End Function

        Public Function createObject(type As Type, propertyVals As list, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(type)
            Dim val As Object

            For Each [property] As PropertyInfo In type _
                .GetProperties(PublicProperty) _
                .Where(Function(pi)
                           Return pi.CanWrite AndAlso pi.GetIndexParameters.IsNullOrEmpty
                       End Function)

                If propertyVals.hasName([property].Name) Then
                    val = propertyVals.getByName([property].Name)
                    val = RCType.CTypeDynamic(val, [property].PropertyType, env)

                    [property].SetValue(obj, val)
                End If
            Next

            Return obj
        End Function
    End Module
End Namespace
