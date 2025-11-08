#Region "Microsoft.VisualBasic::17dc8e16f6b25a2e3ceebbda7148d495, R#\Runtime\Internal\objects\dataset\vbObject.vb"

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

'   Total Lines: 269
'    Code Lines: 196 (72.86%)
' Comment Lines: 40 (14.87%)
'    - Xml Docs: 90.00%
' 
'   Blank Lines: 33 (12.27%)
'     File Size: 11.27 KB


'     Class vbObject
' 
'         Properties: target, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: [TryCast], CreateInstance, existsName, (+2 Overloads) getByName, getNames
'                   getObjMethods, getObjProperties, propertyParserInternal, (+2 Overloads) setByName, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' Proxy for VB.NET class <see cref="Object"/>
    ''' </summary>
    Public Class vbObject : Inherits RsharpDataObject
        Implements RNameIndex

        Public ReadOnly Property target As Object

        ''' <summary>
        ''' R# type wrapper of the type data for <see cref="target"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property elementType As RType
            Get
                Return m_type
            End Get
            Protected Friend Set(value As RType)
                m_type = value
            End Set
        End Property

        Friend ReadOnly properties As Dictionary(Of String, PropertyInfo)
        ReadOnly methods As Dictionary(Of String, RMethodInfo)

        Sub New(obj As Object)
            Call Me.New(obj, obj.GetType)
        End Sub

        Friend Sub New(obj As Object, vbType As Type)
            Static propertySchemaCache As New Dictionary(Of RType, Dictionary(Of String, PropertyInfo))
            Static methodSchemaCache As New Dictionary(Of RType, Dictionary(Of String, RMethodInfo))

            target = obj
            elementType = RType.GetRSharpType(vbType)
            properties = propertySchemaCache.ComputeIfAbsent(
                key:=m_type,
                lazyValue:=Function()
                               Return getObjProperties(m_type.raw) _
                                   .ToDictionary(Function(p) p.Name,
                                                 Function(p)
                                                     Return p.Value
                                                 End Function)
                           End Function)
            methods = methodSchemaCache.ComputeIfAbsent(
                key:=m_type,
                lazyValue:=Function()
                               Return getObjMethods(m_type.raw) _
                                   .ToDictionary(Function(m) m.Name,
                                                 Function(m)
                                                     Return New RMethodInfo(m.Name, m.Value, target)
                                                 End Function)
                           End Function)
        End Sub

        Public Function [TryCast](Of T)() As T
            Return DirectCast(target, T)
        End Function

        Private Shared Function getObjMethods(raw As Type) As NamedValue(Of MethodInfo)()
            Dim schema As NamedValue(Of MethodInfo)() = raw _
               .getObjMethods _
               .GroupBy(Function(m) m.Name) _
               .Select(Iterator Function(g)
                           Dim first As MethodInfo = g _
                               .OrderByDescending(Function(m) m.GetParameters.Length) _
                               .First
                           Dim api As ExportAPIAttribute

                           Yield New NamedValue(Of MethodInfo) With {.Name = g.Key, .Value = first}

                           For Each [overloads] As MethodInfo In g
                               api = [overloads].GetCustomAttribute(Of ExportAPIAttribute)

                               If Not api Is Nothing Then
                                   Yield New NamedValue(Of MethodInfo) With {
                                       .Name = api.Name,
                                       .Value = [overloads]
                                   }
                               End If
                           Next
                       End Function) _
               .IteratesALL _
               .ToArray

            Return schema
        End Function

        Private Shared Function getObjProperties(raw As Type) As NamedValue(Of PropertyInfo)()
            Dim schema As NamedValue(Of PropertyInfo)() = raw _
                .getObjProperties _
                .Select(AddressOf propertyParserInternal) _
                .IteratesALL _
                .ToArray
            Dim duplicateds = schema _
                .GroupBy(Function(a) a.Name) _
                .Where(Function(a) a.Count > 1) _
                .Keys _
                .ToArray

            If duplicateds.Any Then
                Throw New InvalidProgramException(duplicateds.JoinBy(", "))
            End If

            Return schema
        End Function

        Private Shared Iterator Function propertyParserInternal(p As PropertyInfo) As IEnumerable(Of NamedValue(Of PropertyInfo))
            Dim attrs = p.CustomAttributes.ToArray
            Dim name As String
            Dim typeAttr As CustomAttributeTypedArgument
#If NETCOREAPP Then
            Dim nameAttr As CustomAttributeNamedArgument
#End If

            ' Column
            Yield New NamedValue(Of PropertyInfo)(p.Name, p)

            For Each a As CustomAttributeData In attrs
                name = Nothing

#If NETCOREAPP Then
                ' try get name
                If a.AttributeType Is GetType(ColumnAttribute) Then
                    nameAttr = a.NamedArguments.Where(Function(pa) pa.MemberName = "Name").FirstOrDefault
                    name = nameAttr.TypedValue.Value
                End If
#End If
                If name Is Nothing Then
                    If a.AttributeType.Name = "ColumnAttribute" Then
                        typeAttr = a.ConstructorArguments.FirstOrDefault
                        name = typeAttr.Value
                    Else
                        name = Nothing
                    End If
                End If

                If Not name Is Nothing AndAlso name <> p.Name Then
                    Yield New NamedValue(Of PropertyInfo)(name, p)
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            If m_type.haveDynamicsProperty Then
                Return m_type.getNames + DirectCast(target, IDynamicsObject).GetNames.AsList
            Else
                Return m_type.getNames
            End If
        End Function

        Public Function existsName(name As String) As Boolean
            If properties.ContainsKey(name) Then
                Return True
            ElseIf methods.ContainsKey(name) Then
                Return True
            ElseIf m_type.haveDynamicsProperty Then
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
            ElseIf m_type.haveDynamicsProperty Then
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
                    Dim targetType As Type = properties(name).PropertyType

                    If targetType.IsArray Then
                        value = asVector(value, targetType.GetElementType, envir)
                    Else
                        value = RCType.CTypeDynamic(value, targetType, envir)
                    End If

                    Call properties(name).SetValue(target, value)
                Else
                    Return Internal.debug.stop($"Target property '{name}' is not writeable!", envir)
                End If
            ElseIf m_type.haveDynamicsProperty Then
                Call DirectCast(target, IDynamicsObject).SetValue(name, value)
            Else
                Return Internal.debug.stop($"Missing property '{name}'", envir)
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
                Return Internal.debug.stop(New InvalidProgramException("You can not set multiple property for one VisualBasic.NET class object at once!"), envir)
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateInstance(type As Type) As vbObject
            Return New vbObject(Activator.CreateInstance(type))
        End Function
    End Class
End Namespace
