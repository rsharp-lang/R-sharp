Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Class SymbolLanguageProcessor

        ReadOnly env As GlobalEnvironment
        ReadOnly languages As New Dictionary(Of RSymbolLanguageMaskAttribute, (parse As ISymbolLanguageParser, test As ITestSymbolTarget))
        ReadOnly cache As New Dictionary(Of String, Object)

        Sub New(env As GlobalEnvironment)
            Me.env = env
        End Sub

        Public Function Evaluate(symbol As String, ByRef success As Boolean, env As Environment) As Object
            Dim result As Object

            If cache.ContainsKey(symbol) Then
                Return cache(symbol)
            End If

            For Each lang In languages
                If lang.Key.IsCurrentPattern(symbol) Then
                    Try
                        result = lang.Value.parse(symbol, env)
                    Catch ex As Exception
                        result = New Message
                    End Try

                    If Not TypeOf result Is Message Then
                        If Not lang.Value.test Is Nothing AndAlso Not lang.Value.test.Assert(result) Then
                            success = False
                            Continue For
                        Else
                            success = True
                        End If

                        If lang.Key.CanBeCached Then
                            cache.Add(symbol, result)
                        End If

                        Return result
                    End If
                End If
            Next

            success = False

            Return Nothing
        End Function

        Public Function AddSymbolLanguage(tag As RSymbolLanguageMaskAttribute, api As MethodInfo) As Message
            Dim params As ParameterInfo() = api.GetParameters
            Dim parse As ISymbolLanguageParser
            Dim test As ITestSymbolTarget = Nothing

            If Not tag.Test Is Nothing Then
                test = Activator.CreateInstance(tag.Test)
            End If

            If params.Length = 1 Then
                If Not params(Scan0).ParameterType Is GetType(String) Then
                    Return Nothing
                Else
                    parse = Function(symbol, env) api.Invoke(Nothing, {symbol})
                    languages(tag) = (parse, test)
                End If
            ElseIf params.Length > 2 Then
                Return Nothing
            ElseIf params(1).ParameterType Is GetType(Environment) Then
                parse = Function(symbol, env) api.Invoke(Nothing, {symbol, env})
                languages(tag) = (parse, test)
            End If

            Return Nothing
        End Function

    End Class
End Namespace