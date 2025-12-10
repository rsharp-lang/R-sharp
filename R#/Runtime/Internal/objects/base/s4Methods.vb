Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object.baseOp

    Public Module s4Methods

        <Extension>
        Public Function defineObject(type As S4Object, env As GlobalEnvironment) As Type
            Dim def As New DynamicType(GetType(s4Reflector))
            Dim valueType As Type

            For Each slot As KeyValuePair(Of String, String) In type.slots
                Select Case Microsoft.VisualBasic.Strings.Trim(slot.Value).ToLower
                    Case "numeric", "double" : valueType = GetType(Double())
                    Case "integer" : valueType = GetType(Integer())
                    Case "character" : valueType = GetType(String())
                    Case "raw" : valueType = GetType(Byte())
                    Case "list" : valueType = GetType(list)
                    Case "float" : valueType = GetType(Single())
                    Case "long" : valueType = GetType(Long())
                    Case Else
                        valueType = env.types(slot.Value).raw
                End Select

                Call def.Add(slot.Key, valueType)
            Next

            Return def.Create(type.class_name, asm_module:="r_sharp_s4object").GeneratedType
        End Function

        <Extension>
        Public Function createObject(type As RType, values As list, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(type.raw)
            Dim val As Object
            Dim reflection = DataFramework.Schema(type.raw, PropertyAccess.Writeable, PublicProperty, nonIndex:=True)
            Dim writer As PropertyInfo

            For Each slot As KeyValuePair(Of String, Object) In values.slots
                writer = reflection(slot.Key)
                val = slot.Value
                val = RCType.CTypeDynamic(val, writer.PropertyType, env)

                If TypeOf val Is Message Then
                    Return val
                End If

                Call writer.SetValue(obj, val)
            Next

            Return obj
        End Function

        <Extension>
        Public Function createObject(type As S4Object, values As list, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(type.raw)
            Dim val As Object

            For Each slot As KeyValuePair(Of String, String) In type.slots
                If values.hasName(slot.Key) Then
                    val = values.getByName(slot.Key)
                Else
                    ' use the default value
                    If type.prototype.ContainsKey(slot.Key) Then
                        val = type.prototype(slot.Key)
                    Else
                        Continue For
                    End If
                End If

                val = s4Methods.conversionValue(val, slot.Value, env)
                type.reflection(slot.Key).SetValue(obj, val)
            Next

            Return obj
        End Function

        Public Function conversionValue(val As Object, type As String, env As Environment) As Object
            Select Case Microsoft.VisualBasic.Strings.Trim(type).ToLower
                Case "numeric", "double" : val = CLRVector.asNumeric(val)
                Case "integer" : val = CLRVector.asInteger(val)
                Case "character" : val = CLRVector.asCharacter(val)
                Case "raw" : val = CLRVector.asRawByte(val)
                Case "list" : val = base.Rlist(val, env)
                Case "float" : val = CLRVector.asFloat(val)
                Case "long" : val = CLRVector.asLong(val)
                Case Else
                    ' no conversion?
            End Select

            Return val
        End Function
    End Module
End Namespace