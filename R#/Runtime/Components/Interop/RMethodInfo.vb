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
            Dim result As Object
            Dim parameters As New List(Of Object)
            Dim arguments As Dictionary(Of String, Object) = InvokeParameter.CreateArguments(envir, params)
            Dim arg As RMethodArgument

            For i As Integer = 0 To Me.parameters.Length - 1
                arg = Me.parameters(i)

                If arguments.ContainsKey(arg.name) Then
                    parameters.Add(getValue(arg, arguments(arg.name)))
                ElseIf i >= params.Length Then
                    ' default value
                    If arg.type.raw Is GetType(Environment) Then
                        parameters.Add(envir)
                    ElseIf Not Me.parameters(i).isOptional Then
                        Return Internal.stop({$"Missing parameter value for '{Me.parameters(i).name}'!", "function: " & name, "environment: " & envir.ToString}, envir)
                    Else
                        parameters.Add(Me.parameters(i).default)
                    End If
                Else
                    parameters.Add(getValue(arg, arguments("$" & i)))
                End If
            Next

            result = api.Method.Invoke(api.Target, parameters.ToArray)

            Return result
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