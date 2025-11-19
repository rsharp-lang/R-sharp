Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object.baseOp

    Module s4Methods

        <Extension>
        Public Function defineObject(type As S4Object, env As GlobalEnvironment) As Type
            Dim def As New DynamicType
            Dim valueType As Type

            For Each slot As KeyValuePair(Of String, String) In type.slots
                Select Case Microsoft.VisualBasic.Strings.Trim(slot.Value).ToLower
                    Case "numeric" : valueType = GetType(Double())
                    Case "integer" : valueType = GetType(Integer())
                    Case "character" : valueType = GetType(String())
                    Case "raw" : valueType = GetType(Byte())
                    Case "list" : valueType = GetType(list)
                    Case Else
                        valueType = env.types(slot.Value).raw
                End Select

                def.Add(slot.Key, valueType)
            Next

            Return def.Create.GeneratedType
        End Function

        <Extension>
        Public Function createObject(type As S4Object, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(type.raw)

            For Each slot As KeyValuePair(Of String, String) In type.slots
                If Not type.prototype.ContainsKey(slot.Key) Then
                    Continue For
                End If

                Dim val As Object = slot.Value

                Select Case Microsoft.VisualBasic.Strings.Trim(slot.Value).ToLower
                    Case "numeric" : val = CLRVector.asNumeric(val)
                    Case "integer" : val = CLRVector.asInteger(val)
                    Case "character" : val = CLRVector.asCharacter(val)
                    Case "raw" : val = CLRVector.asRawByte(val)
                    Case "list" : base.Rlist(val, env)
                    Case Else
                        ' no conversion?
                End Select


            Next

            Return obj
        End Function
    End Module
End Namespace