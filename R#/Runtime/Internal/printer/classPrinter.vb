#Region "Microsoft.VisualBasic::cfd8b913736a253dd56160d16ba7b522, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/printer/classPrinter.vb"

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

    '   Total Lines: 117
    '    Code Lines: 84
    ' Comment Lines: 14
    '   Blank Lines: 19
    '     File Size: 4.56 KB


    '     Module classPrinter
    ' 
    '         Function: getMemberValueString, getObjMethods, getObjProperties, getTypeDisplay, printClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal.ConsolePrinter

    ''' <summary>
    ''' Console print formatter for non System user class type
    ''' </summary>
    Public Module classPrinter

        ''' <summary>
        ''' A utils function for print user defined structure and class object instance. 
        ''' </summary>
        ''' <param name="obj">
        ''' The object class is ensure that not nothing!
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' All of the method is non generic, and the property without arguments!
        ''' All of the sub program will be mapping as returns nothing
        ''' </remarks>
        Public Function printClass(obj As Object) As String
            Dim type As Type = obj.GetType
            Dim properties As PropertyInfo() = type.getObjProperties.ToArray
            Dim methods As MethodInfo() = type.getObjMethods.ToArray
            Dim sb As New StringBuilder

            Call sb.AppendLine($"instance of '{type.Name}' from namespace: {type.Namespace}")
            Call sb.AppendLine()
            Call sb.AppendLine($" {properties.Length} properties")

            Dim valueStr$
            Dim typeCode$
            Dim rw$

            For Each [property] As PropertyInfo In properties
                valueStr = [property].getMemberValueString(obj)
                typeCode = [property].PropertyType.getTypeDisplay
                rw = If([property].CanRead, "r", "")
                rw = If([property].CanWrite, If(rw.Length = 0, "w", rw & "/w"), rw)

                Call sb.AppendLine($"  ${[property].Name} [{rw}] as {typeCode}: {valueStr}")
            Next

            Call sb.AppendLine()
            Call sb.AppendLine($" {methods.Length} methods")

            Static systemObject As Index(Of String) = {"GetHashCode", "Equals", "GetType", "ToString"}

            For Each method As MethodInfo In methods
                If Not method.Name Like systemObject Then
                    Call sb.AppendLine($"  &{method.Name} -> {method.ReturnType.getTypeDisplay}")
                End If
            Next

            Return sb.ToString
        End Function

        <Extension>
        Private Function getTypeDisplay(type As Type) As String
            Dim code As TypeCodes = type.GetRTypeCode

            If code = TypeCodes.generic Then
                Return type.Name
            Else
                Return type.Description
            End If
        End Function

        <Extension>
        Private Function getMemberValueString([property] As PropertyInfo, obj As Object) As String
            Dim value As Object = Nothing
            Dim type As Type = [property].PropertyType

            Try
                value = [property].GetValue(obj, Nothing)
            Catch ex As Exception
                value = $"<Error> {ex.Message}"
            End Try

            If value Is Nothing Then
                Return "NULL"
            ElseIf DataFramework.IsPrimitive(type) Then
                Return value.ToString
            End If

            Dim valStr As String = Scripting.ToString(value, "NULL", True)

            If valStr Is Nothing Then
                Return "<unavailable>"
            Else
                Return valStr
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function getObjMethods(type As Type) As IEnumerable(Of MethodInfo)
            Return type _
                .GetMethods(PublicProperty) _
                .Where(Function(m)
                           Return Not m.ContainsGenericParameters AndAlso
                                      m.GetCustomAttribute(GetType(CompilerGeneratedAttribute)) Is Nothing AndAlso
                                  Not m.Attributes.HasFlag(MethodAttributes.SpecialName)
                       End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function getObjProperties(type As Type) As IEnumerable(Of PropertyInfo)
            Return type _
                .GetProperties(PublicProperty) _
                .Where(Function(p)
                           Return p.GetIndexParameters.IsNullOrEmpty
                       End Function)
        End Function
    End Module
End Namespace
