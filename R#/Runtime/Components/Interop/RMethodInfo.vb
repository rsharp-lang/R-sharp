Imports System.Reflection
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Components

    Public Class RMethodInfo : Implements RFunction

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property api As [Delegate]
        Public ReadOnly Property returns As RType
        Public ReadOnly Property parameters As RMethodArgument()

        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.api = closure
            Me.returns = New RType(closure.Method.ReturnType)
            Me.parameters = closure.Method.DoCall(AddressOf parseParameters)
        End Sub

        Sub New(name$, closure As MethodInfo, target As Object)
            Me.name = name
            Me.api = Function(params As Object())
                         Return closure.Invoke(target, params)
                     End Function
            Me.returns = New RType(closure.ReturnType)
            Me.parameters = closure.DoCall(AddressOf parseParameters)
        End Sub

        Private Shared Function parseParameters(method As MethodInfo) As RMethodArgument()
            Return method _
                .GetParameters _
                .Select(AddressOf RMethodArgument.ParseArgument) _
                .ToArray
        End Function

        Public Function Invoke(envir As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim arguments As Dictionary(Of String, Object) = InvokeParameter.CreateArguments(envir, params)
            Dim parameters As Object()

            If Me.parameters.Any(Function(a) a.isObjectList) Then
                Throw New NotImplementedException
            Else
                parameters = createNormalArguments(envir, arguments).ToArray
            End If

            For Each arg In parameters
                If Not arg Is Nothing AndAlso arg.GetType Is GetType(Message) Then
                    Return arg
                End If
            Next

            Dim result As Object = api.Method.Invoke(api.Target, parameters.ToArray)

            Return result
        End Function

        Private Iterator Function createNormalArguments(envir As Environment, arguments As Dictionary(Of String, Object)) As IEnumerable(Of Object)
            Dim arg As RMethodArgument

            For i As Integer = 0 To Me.parameters.Length - 1
                arg = Me.parameters(i)

                If arguments.ContainsKey(arg.name) Then
                    Yield getValue(arg, arguments(arg.name))
                ElseIf i >= arguments.Count Then
                    ' default value
                    If arg.type.raw Is GetType(Environment) Then
                        Yield envir
                    ElseIf Not arg.isOptional Then
                        Yield Internal.stop({$"Missing parameter value for '{arg.name}'!", "function: " & name, "environment: " & envir.ToString}, envir)
                    Else
                        Yield arg.default
                    End If
                Else
                    Yield getValue(arg, arguments("$" & i))
                End If
            Next
        End Function

        Private Shared Function getValue(arg As RMethodArgument, value As Object) As Object
            If arg.type.isArray Then
                Return CObj(Runtime.asVector(value, arg.type.raw.GetElementType))
            Else
                Return Runtime.getFirst(value)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {api.ToString}"
        End Function
    End Class
End Namespace