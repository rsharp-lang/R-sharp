Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal

    ''' <summary>
    ''' Proxy for VB.NET class <see cref="Object"/>
    ''' </summary>
    Public Class vbObject : Implements RNameIndex

        Public ReadOnly Property target As Object
        Public ReadOnly Property type As RType

        Dim properties As Dictionary(Of String, PropertyInfo)
        Dim methods As Dictionary(Of String, RMethodInfo)

        Sub New(obj As Object)
            target = obj
            type = New RType(obj.GetType)
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
            Return type.getNames
        End Function

        ''' <summary>
        ''' Get property value/method reference by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If properties.ContainsKey(name) Then
                Return properties(name).GetValue(target)
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
        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If properties.ContainsKey(name) Then
                If properties(name).CanWrite Then
                    properties(name).SetValue(target, value)
                Else
                    Return Internal.stop($"Target property '{name}' is not writeable!", envir)
                End If
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
            Return Internal.stop(New InvalidProgramException, envir)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return target.ToString
        End Function
    End Class
End Namespace