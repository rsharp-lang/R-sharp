#Region "Microsoft.VisualBasic::b1ae65f84be9503719657248690c9d96, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/RType.vb"

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

    '   Total Lines: 408
    '    Code Lines: 249
    ' Comment Lines: 114
    '   Blank Lines: 45
    '     File Size: 15.59 KB


    '     Interface IRType
    ' 
    '         Properties: className, mode, raw
    ' 
    '     Class RType
    ' 
    '         Properties: any, characters, closure, floats, fullName
    '                     getCount, getItem, haveDynamicsProperty, integers, isArray
    '                     isCollection, isEnvironment, isGenericListObject, isPrimitive, list
    '                     logicals, mode, name, raw
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) [GetType], [TypeOf], getNames, GetRawElementType, GetRSharpType
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
Imports Microsoft.VisualBasic.Scripting
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    ''' <summary>
    ''' the common model of the .NET clr type and R# runtime type
    ''' </summary>
    Public Interface IRType : Inherits IReflector

        ''' <summary>
        ''' the class name for display and inspect
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property className As String
        ReadOnly Property mode As TypeCodes
        ReadOnly Property raw As Type

    End Interface

    ''' <summary>
    ''' The type wrapper for .NET type to R# language runtime
    ''' </summary>
    Public Class RType : Implements IReflector, IRType

        ''' <summary>
        ''' the raw CLR <see cref="Type.FullName"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property fullName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return raw.FullName
            End Get
        End Property

        ''' <summary>
        ''' the raw CLR type name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements IRType.className
            Get
                Return raw.Name
            End Get
        End Property

        ''' <summary>
        ''' The mapped R# data type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property mode As TypeCodes Implements IRType.mode
        Public ReadOnly Property isArray As Boolean
        Public ReadOnly Property isCollection As Boolean

        ''' <summary>
        ''' is R# runtime type?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isPrimitive As Boolean
            Get
                Select Case mode
                    Case TypeCodes.boolean,
                         TypeCodes.dataframe,
                         TypeCodes.double,
                         TypeCodes.environment,
                         TypeCodes.formula,
                         TypeCodes.integer,
                         TypeCodes.list,
                         TypeCodes.NA,
                         TypeCodes.raw,
                         TypeCodes.string

                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

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
        ''' based .NET CLR type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property raw As Type Implements IRType.raw
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

        ''' <summary>
        ''' include single element and array element, example as:
        ''' 
        ''' integer, integer()
        ''' </summary>
        ''' <returns></returns>
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

        ''' <summary>
        ''' boolean and boolean()
        ''' </summary>
        ''' <returns></returns>
        Friend Shared ReadOnly Property logicals As Index(Of Type) = {
            GetType(Boolean),
            GetType(Boolean())
        }

        Friend Shared ReadOnly Property characters As Index(Of Type) = {
            GetType(String), GetType(String()),
            GetType(Char), GetType(Char())
        }

        Public Shared ReadOnly Property list As RType = RType.GetRSharpType(GetType(list))

        ''' <summary>
        ''' <see cref="Object"/>
        ''' </summary>
        ''' <returns></returns>
        Friend Shared ReadOnly Property any As RType = GetRSharpType(GetType(Object))

        Friend Shared ReadOnly Property closure As RType = GetRSharpType(GetType(RFunction))

        Private Sub New(raw As Type)
            Me.raw = GetUnderlyingType(raw)
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

        Public Shared Function GetUnderlyingType(raw As Type) As Type
            If raw.IsValueType AndAlso
               raw.Namespace = "System" AndAlso
               raw.GenericTypeArguments.Length = 1 AndAlso
               raw.Name = GetType(Nullable(Of )).Name Then

                raw = raw.GenericTypeArguments.First
            End If

            Return raw
        End Function

        ''' <summary>
        ''' gets the array element type from current r# type <see cref="raw"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRawElementType() As Type
            If raw Is GetType(Array) OrElse raw.GetElementType Is Nothing Then
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
            ElseIf raw Is GetType(Void) OrElse raw.FullName = "System.RuntimeType" OrElse raw.FullName = "System.Object" Then
                Return "any"
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
        ''' Get VB.NET clr type to R# type wrapper <see cref="RType"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function GetRSharpType(type As Type) As RType
            Static cache As New Dictionary(Of Type, RType)

            If type Is Nothing Then
                Return Nothing
            Else
                Return cache.ComputeIfAbsent(type, Function(t) New RType(t))
            End If
        End Function

        ''' <summary>
        ''' create <see cref="RType"/> based on the .NET clr <see cref="TypeCode"/> enumeration value
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function [GetType](code As TypeCode) As RType
            Return GetRSharpType(Runtime.GetType(code))
        End Function

        ''' <summary>
        ''' create <see cref="RType"/> based on the R# <see cref="TypeCodes"/> enumeration value
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function [GetType](code As TypeCodes) As RType
            Return GetRSharpType(Runtime.GetType(code))
        End Function

        ''' <summary>
        ''' Convert the type information model to R# type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Overloads Shared Function [GetType](type As MetaData.TypeInfo, runtime As GlobalEnvironment) As RType
            Dim key As String = type.ToString

            Static cache As New Dictionary(Of String, RType)

            SyncLock cache
                If cache.ContainsKey(key) Then
                    Return cache(key)
                End If
            End SyncLock

            Dim dirs As String() = runtime.attachedNamespace.GetDllDirectories.Distinct.ToArray
            Dim model As Type = type.GetType(knownFirst:=True, searchPath:=dirs)
            Dim rtype As RType = GetRSharpType(model)

            SyncLock cache
                cache(key) = rtype
            End SyncLock

            Return rtype
        End Function

        ''' <summary>
        ''' get R# type value of the given VB.NET object value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 对于在``R#``语言之中的基础类型，例如vector,list等，这个
        ''' 函数会返回元素基础类型值：<see cref="RsharpDataObject.elementType"/>.
        ''' </returns>
        Public Shared Function [TypeOf](x As Object) As RType
            If x Is Nothing Then
                Return any
            ElseIf TypeOf x Is RsharpDataObject Then
                Dim type As RType = DirectCast(x, RsharpDataObject).elementType

                If type Is Nothing Then
                    If TypeOf x Is vector Then
                        Return DirectCast(x, vector).data _
                            .DoCall(AddressOf MeasureRealElementType) _
                            .DoCall(AddressOf RType.GetRSharpType)
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

        ''' <summary>
        ''' Enable conversion from R# type data to CLR type information
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Narrowing Operator CType(type As RType) As Type
            If type Is Nothing Then
                Return Nothing
            Else
                Return type.raw
            End If
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
            ElseIf type.mode <> TypeCodes.generic AndAlso type.mode = baseType.mode Then
                Return True
            ElseIf type.raw.IsArray AndAlso Not type.raw.GetElementType Is Nothing Then
#Disable Warning BC42004 ' 表达式递归调用包含运算符
                Return GetRSharpType(type.raw.GetElementType) Like baseType
#Enable Warning BC42004 ' 表达式递归调用包含运算符
            Else
                Return type.raw.IsInheritsFrom(baseType.raw)
            End If
        End Operator
    End Class
End Namespace
