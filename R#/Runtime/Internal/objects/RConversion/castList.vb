Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Runtime.Internal.Object.Converts

    ''' <summary>
    ''' cast ``R#`` <see cref="list"/> to <see cref="Dictionary(Of String, TValue)"/>
    ''' </summary>
    Module castList

        <Extension>
        Public Function CTypeList(list As list, type As Type, env As Environment) As Object
            Dim table As IDictionary = Activator.CreateInstance(type)
            Dim keyType As Type = type.GenericTypeArguments(Scan0)
            Dim valType As Type = type.GenericTypeArguments(1)
            Dim key As Object
            Dim val As Object

            For Each item In list.slots
                key = Scripting.CTypeDynamic(item.Key, keyType)
                val = RConversion.CTypeDynamic(item.Value, valType, env)
                table.Add(key, val)
            Next

            Return table
        End Function

        Friend Function listInternal(obj As Object, args As list) As list
            Dim type As Type = obj.GetType

            Select Case type
                Case GetType(Dictionary(Of String, Object))
                    Return New list With {.slots = obj}
                Case GetType(list)
                    Return obj
                Case GetType(vbObject)
                    ' object property as list data
                    Return DirectCast(obj, vbObject).toList
                Case GetType(dataframe)
                    Dim byRow As Boolean = Runtime.asLogical(args!byrow)(Scan0)

                    If byRow Then
                        Return DirectCast(obj, dataframe).listByRows
                    Else
                        Return DirectCast(obj, dataframe).listByColumns
                    End If
                Case Else
                    If type.ImplementInterface(GetType(IDictionary)) Then
                        Dim objList As New Dictionary(Of String, Object)

                        With DirectCast(obj, IDictionary)
                            For Each key As Object In .Keys
                                Call objList.Add(Scripting.ToString(key), .Item(key))
                            Next
                        End With

                        Return New list With {.slots = objList}
                    Else
                        Throw New NotImplementedException
                    End If
            End Select
        End Function
    End Module
End Namespace