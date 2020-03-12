Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object.Converts

    Public NotInheritable Class RCType

        Private Sub New()
        End Sub

        ''' <summary>
        ''' If target <paramref name="type"/> is <see cref="Object"/>, then this function 
        ''' will stop the narrowing conversion from <see cref="vbObject"/> wrapper to 
        ''' object type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function CTypeDynamic(obj As Object, type As Type, env As Environment) As Object
            If obj Is Nothing Then
                Return Nothing
            ElseIf type Is GetType(vbObject) Then
                Return asObject(obj)
            End If

            Dim objType As Type = obj.GetType

            If objType Is GetType(vbObject) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, vbObject).target

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(RDispose) AndAlso Not type Is GetType(Object) Then
                obj = DirectCast(obj, RDispose).Value

                If Not obj Is Nothing AndAlso obj.GetType Is type Then
                    Return obj
                End If
            ElseIf objType Is GetType(list) AndAlso type.ImplementInterface(GetType(IDictionary)) Then
                ' cast R# list object to any dictionary table object???
                Return DirectCast(obj, list).CTypeList(type, env)
            ElseIf type.IsEnum Then
                Return CastToEnum(obj, type, env)
            ElseIf objType Is GetType(Environment) AndAlso type Is GetType(GlobalEnvironment) Then
                ' fix the type mismatch bugs for passing value to 
                ' a API parameter which its data type is a global 
                ' environment.
                Return DirectCast(obj, Environment).globalEnvironment
            ElseIf makeObject.isObjectConversion(type, obj) Then
                Return makeObject.createObject(type, obj, env)
            ElseIf objType.IsArray AndAlso type.IsArray Then
                Return Runtime.asVector(obj, type.GetElementType, env)
            End If

            Return Conversion.CTypeDynamic(obj, type)
        End Function

        Public Shared Function CastToEnum(obj As Object, type As Type, env As Environment) As Object
            Dim REnum As REnum = REnum.GetEnumList(type)

            If obj.GetType Is GetType(String) Then
                If REnum.hasName(obj) Then
                    Return REnum.GetByName(obj)
                Else
                    Return debug.stop($"Can not convert string '{obj}' to enum type: {REnum.raw.FullName}", env)
                End If
            ElseIf obj.GetType.GetRTypeCode = TypeCodes.integer Then
                Return REnum.getByIntVal(obj)
            Else
                Return debug.stop($"Can not convert type '{obj.GetType.FullName}' to enum type: {REnum.raw.FullName}", env)
            End If
        End Function

    End Class
End Namespace