Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object.baseOp

    Public MustInherit Class s4Reflector : Inherits RsharpDataObject
        Implements RNameIndex

        Friend ReadOnly properties As Dictionary(Of String, PropertyInfo)
        Friend ReadOnly methods As Dictionary(Of String, RMethodInfo)

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

        ''' <summary>
        ''' processing of myself
        ''' </summary>
        Public Sub New()
        End Sub

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

        Public Shared Function MakeReflection(obj As Object,
                                              <Out> ByRef properties As Dictionary(Of String, PropertyInfo),
                                              <Out> ByRef methods As Dictionary(Of String, RMethodInfo)) As RType

            Dim type As RType = RType.GetRSharpType(obj.GetType)

            Static propertySchemaCache As New Dictionary(Of RType, Dictionary(Of String, PropertyInfo))
            Static methodSchemaCache As New Dictionary(Of RType, Dictionary(Of String, RMethodInfo))

            properties = propertySchemaCache.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function()
                               Return getObjProperties(type.raw) _
                                   .ToDictionary(Function(p) p.Name,
                                                 Function(p)
                                                     Return p.Value
                                                 End Function)
                           End Function)
            methods = methodSchemaCache.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function()
                               Return getObjMethods(type.raw) _
                                   .ToDictionary(Function(m) m.Name,
                                                 Function(m)
                                                     Return New RMethodInfo(m.Name, m.Value, obj)
                                                 End Function)
                           End Function)

            Return type
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
        Public Overridable Function getByName(name As String) As Object Implements RNameIndex.getByName
            If properties.ContainsKey(name) Then
                Return properties(name).GetValue(Me)
            ElseIf methods.ContainsKey(name) Then
                Return methods(name)
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
        Public Overridable Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If properties.ContainsKey(name) Then
                If properties(name).CanWrite Then
                    Dim targetType As Type = properties(name).PropertyType

                    If targetType.IsArray Then
                        value = asVector(value, targetType.GetElementType, envir)
                    Else
                        value = RCType.CTypeDynamic(value, targetType, envir)
                    End If

                    Call properties(name).SetValue(Me, value)
                Else
                    Return Internal.debug.stop($"Target property '{name}' is not writeable!", envir)
                End If
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
        Public Overridable Function getNames() As String() Implements IReflector.getNames
            Return m_type.getNames
        End Function
    End Class
End Namespace