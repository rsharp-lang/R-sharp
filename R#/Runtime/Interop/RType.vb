#Region "Microsoft.VisualBasic::e3b2f73648157489b2e13db8a2caa238, R#\Runtime\Interop\RType.vb"

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

    '     Class RType
    ' 
    '         Properties: any, characters, floats, fullName, getCount
    '                     getItem, haveDynamicsProperty, integers, isArray, isCollection
    '                     isEnvironment, isGenericListObject, logicals, mode, name
    '                     raw
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [GetType], [TypeOf], getNames, GetRawElementType, GetRSharpType
    '                   populateNames, ToString
    '         Operators: (+4 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    ''' <summary>
    ''' The type wrapper for .NET type to R# language runtime
    ''' </summary>
    Public Class RType : Implements IReflector

        ''' <summary>
        ''' <see cref="Type.FullName"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property fullName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return raw.FullName
            End Get
        End Property

        Public ReadOnly Property name As String
            Get
                Return raw.Name
            End Get
        End Property

        ''' <summary>
        ''' The mapped R# data type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property mode As TypeCodes
        Public ReadOnly Property isArray As Boolean
        Public ReadOnly Property isCollection As Boolean

        ''' <summary>
        ''' Is dictionary of string and value types?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isGenericListObject As Boolean
            Get
                If Not raw.ImplementInterface(GetType(IDictionary)) Then
                    Return False
                End If
                If raw.GenericTypeArguments.Length <> 2 Then
                    Return False
                End If
                If raw.GenericTypeArguments(Scan0) Is GetType(String) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' .NET type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property raw As Type
        ''' <summary>
        ''' implements interface of <see cref="IDynamicsObject"/>?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property haveDynamicsProperty As Boolean
        ''' <summary>
        ''' is an <see cref="Environment"/> object?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEnvironment As Boolean

        Dim names As String()

        Public ReadOnly Property getCount As PropertyInfo
        Public ReadOnly Property getItem As PropertyInfo

        Friend Shared ReadOnly Property integers As Index(Of Type) = {
            GetType(Integer), GetType(Integer()),
            GetType(Long), GetType(Long()),
            GetType(Short), GetType(Short()),
            GetType(Byte), GetType(Byte()),
            GetType(UInteger), GetType(UInteger()),
            GetType(ULong), GetType(ULong())
        }

        Friend Shared ReadOnly Property floats As Index(Of Type) = {
            GetType(Single), GetType(Single()),
            GetType(Double), GetType(Double())
        }

        Friend Shared ReadOnly Property logicals As Index(Of Type) = {
            GetType(Boolean),
            GetType(Boolean())
        }

        Friend Shared ReadOnly Property characters As Index(Of Type) = {
            GetType(String), GetType(String()),
            GetType(Char), GetType(Char())
        }

        ''' <summary>
        ''' <see cref="Object"/>
        ''' </summary>
        ''' <returns></returns>
        Friend Shared ReadOnly Property any As RType = GetRSharpType(GetType(Object))

        Private Sub New(raw As Type)
            If raw.IsValueType AndAlso
               raw.Namespace = "System" AndAlso
               raw.GenericTypeArguments.Length = 1 AndAlso
               raw.Name = GetType(Nullable(Of )).Name Then

                raw = raw.GenericTypeArguments.First
            End If

            Me.raw = raw
            Me.names = populateNames _
                .Distinct _
                .ToArray
            Me.haveDynamicsProperty = raw.ImplementInterface(GetType(IDynamicsObject))
            Me.isArray = raw Is GetType(Array) _
                  OrElse raw.IsInheritsFrom(GetType(Array))
            Me.isCollection = raw.ImplementInterface(GetType(IEnumerable)) AndAlso Not raw Is GetType(String)
            Me.mode = raw.GetRTypeCode
            Me.isEnvironment = raw.IsInheritsFrom(GetType(Environment), strict:=False)

            Static countNames As Index(Of String) = {"Count", "count", "Length", "length", "len"}

            Me.getCount = raw _
                .GetProperties(PublicProperty) _
                .Where(Function(p)
                           Return p.CanRead AndAlso
                                  p.Name Like countNames AndAlso
                                  p.PropertyType Is GetType(Integer) AndAlso
                                  p.GetIndexParameters.IsNullOrEmpty
                       End Function) _
                .FirstOrDefault
            Me.getItem = raw _
                .GetProperties(PublicProperty) _
                .Where(Function(p)
                           Return p.Name = "Item" AndAlso
                                  p.CanRead AndAlso
                                  p.GetIndexParameters.Length = 1 AndAlso
                                  p.GetIndexParameters(Scan0).ParameterType Is GetType(Integer)
                       End Function) _
                .FirstOrDefault
        End Sub

        Public Function GetRawElementType() As Type
            If raw Is GetType(Array) Then
                Return GetType(Object)
            Else
                Return raw.GetElementType
            End If
        End Function

        ''' <summary>
        ''' <see cref="getNames()"/>
        ''' </summary>
        ''' <returns></returns>
        Private Iterator Function populateNames() As IEnumerable(Of String)
            For Each m As MethodInfo In raw.getObjMethods
                Yield m.Name
            Next
            For Each p As PropertyInfo In raw.getObjProperties
                Yield p.Name
            Next
        End Function

        Public Overrides Function ToString() As String
            If mode.IsPrimitive Then
                Return mode.Description
            ElseIf isGenericListObject Then
                Return $"list[{GetRSharpType(raw.GenericTypeArguments(1)).ToString}]"
            ElseIf raw.IsEnum Then
                Return $"<integer> {raw.Name}"
            ElseIf raw.IsInheritsFrom(GetType(ValueTuple), strict:=False) Then
                Return $"({raw.GenericTypeArguments.Select(AddressOf GetRSharpType).JoinBy(", ")})"
            Else
                Return $"<{mode.Description}> {raw.Name}"
            End If
        End Function

        ''' <summary>
        ''' Get method names and property names of target type object instance
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function getNames() As String() Implements IReflector.getNames
            Return names.Clone
        End Function

        ''' <summary>
        ''' Get VB.NET to R# type wrapper
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function GetRSharpType(type As Type) As RType
            Static cache As New Dictionary(Of Type, RType)
            Return cache.ComputeIfAbsent(type, Function(t) New RType(t))
        End Function

        Public Overloads Shared Function [GetType](code As TypeCodes) As RType
            Return GetRSharpType(Runtime.GetType(code))
        End Function

        ''' <summary>
        ''' get R# type value of the given VB.NET object value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function [TypeOf](x As Object) As RType
            If x Is Nothing Then
                Return any
            ElseIf TypeOf x Is RsharpDataObject Then
                Dim type As RType = DirectCast(x, RsharpDataObject).elementType

                If type Is Nothing Then
                    If TypeOf x Is vector Then
                        Return MeasureRealElementType(DirectCast(x, vector).data).DoCall(AddressOf RType.GetRSharpType)
                    Else
                        Return GetRSharpType(x.GetType)
                    End If
                Else
                    Return type
                End If
            Else
                Return GetRSharpType(x.GetType)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Narrowing Operator CType(type As RType) As Type
            Return type.raw
        End Operator

        ''' <summary>
        ''' <see cref="raw"/> type is given target <paramref name="type"/>?
        ''' </summary>
        ''' <param name="rtype"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Operator Like(rtype As RType, type As Type) As Boolean
            Return rtype.raw Is type
        End Operator

        ''' <summary>
        ''' does <paramref name="type"/> can be convert to the <paramref name="baseType"/>?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="baseType"></param>
        ''' <returns></returns>
        Public Shared Operator Like(type As RType, baseType As RType) As Boolean
            If type.raw Is baseType.raw Then
                Return True
            ElseIf type.mode = baseType.mode Then
                Return True
            Else
                Return type.raw.IsInheritsFrom(baseType.raw)
            End If
        End Operator
    End Class
End Namespace
