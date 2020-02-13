#Region "Microsoft.VisualBasic::793cbc877570340583302d562d53888a, R#\Runtime\Internal\objects\vbObject.vb"

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

    '     Class vbObject
    ' 
    '         Properties: target, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: existsName, (+2 Overloads) getByName, getNames, (+2 Overloads) setByName, toList
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' Proxy for VB.NET class <see cref="Object"/>
    ''' </summary>
    Public Class vbObject : Implements RNameIndex

        Public ReadOnly Property target As Object

        ''' <summary>
        ''' R# type wrapper of the type data for <see cref="target"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property type As RType

        ReadOnly properties As Dictionary(Of String, PropertyInfo)
        ReadOnly methods As Dictionary(Of String, RMethodInfo)

        Sub New(obj As Object)
            target = obj
            type = RType.GetRSharpType(obj.GetType)
            properties = type.raw.getObjProperties.ToDictionary(Function(p) p.Name)
            methods = type.raw _
                .getObjMethods _
                .GroupBy(Function(m) m.Name) _
                .Select(Function(g)
                            Return g _
                                .OrderByDescending(Function(m) m.GetParameters.Length) _
                                .First
                        End Function) _
                .ToDictionary(Function(m) m.Name,
                              Function(m)
                                  Return New RMethodInfo(m.Name, m, target)
                              End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            If type.haveDynamicsProperty Then
                Return type.getNames + DirectCast(target, IDynamicsObject).GetNames.AsList
            Else
                Return type.getNames
            End If
        End Function

        Public Function existsName(name As String) As Boolean
            If properties.ContainsKey(name) Then
                Return True
            ElseIf methods.ContainsKey(name) Then
                Return True
            ElseIf type.haveDynamicsProperty Then
                Return DirectCast(target, IDynamicsObject).HasName(name)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Get property value/method reference by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' Function will returns nothing when target not found, but 
        ''' property value read getter result may be nothing, please 
        ''' check member exists or not by method <see cref="existsName"/> 
        ''' if the result value is nothing of this function.
        ''' </returns>
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If properties.ContainsKey(name) Then
                Return properties(name).GetValue(target)
            ElseIf methods.ContainsKey(name) Then
                Return methods(name)
            ElseIf type.haveDynamicsProperty Then
                Return DirectCast(target, IDynamicsObject).GetItemValue(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Get properties value collection by a given name list
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Return names.Select(AddressOf getByName).ToArray
        End Function

        ''' <summary>
        ''' set property value by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If properties.ContainsKey(name) Then
                If properties(name).CanWrite Then
                    properties(name).SetValue(target, value)
                Else
                    Return Internal.stop($"Target property '{name}' is not writeable!", envir)
                End If
            ElseIf type.haveDynamicsProperty Then
                Call DirectCast(target, IDynamicsObject).SetValue(name, value)
            Else
                Return Internal.stop($"Missing property '{name}'", envir)
            End If

            Return value
        End Function

        ''' <summary>
        ''' set properties values by given name list
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            If names.Length = 0 Then
                Return Nothing
            ElseIf names.Length = 1 Then
                Return setByName(names(Scan0), Runtime.getFirst(value), envir)
            Else
                Return Internal.stop(New InvalidProgramException("You can not set multiple property for one VisualBasic.NET class object at once!"), envir)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If target Is Nothing Then
                Return "NULL"
            ElseIf printer.RtoString.ContainsKey(target.GetType) Then
                Return printer.RtoString(target.GetType)(target)
            Else
                Return target.ToString
            End If
        End Function

        Public Function toList() As list
            Dim list As Dictionary(Of String, Object) = properties _
                .ToDictionary(Function(p) p.Key,
                              Function(p)
                                  Return p.Value.GetValue(target)
                              End Function)

            If type.haveDynamicsProperty Then
                Dim dynamic As IDynamicsObject = target

                For Each name As String In dynamic _
                    .GetNames _
                    .Where(Function(nameKey)
                               Return Not list.ContainsKey(nameKey)
                           End Function)

                    Call list.Add(name, dynamic.GetItemValue(name))
                Next
            End If

            Return New list With {.slots = list}
        End Function
    End Class
End Namespace
