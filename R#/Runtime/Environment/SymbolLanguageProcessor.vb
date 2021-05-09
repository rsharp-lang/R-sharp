Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Class SymbolLanguageProcessor

        ReadOnly env As GlobalEnvironment
        ReadOnly languages As New Dictionary(Of RSymbolLanguageMaskAttribute, ISymbolLanguageParser)
        ReadOnly cache As New Dictionary(Of String, Object)

        Sub New(env As GlobalEnvironment)
            Me.env = env
        End Sub

        Public Function Evaluate(symbol As String, ByRef success As Boolean, env As Environment) As Object
            Dim result As Object

            If cache.ContainsKey(symbol) Then
                Return cache(symbol)
            End If

            For Each language As KeyValuePair(Of RSymbolLanguageMaskAttribute, ISymbolLanguageParser) In languages
                If language.Key.IsCurrentPattern(symbol) Then
                    Try
                        result = language.Value(symbol, env)
                    Catch ex As Exception
                        result = New Message
                    End Try

                    If Not TypeOf result Is Message Then
                        success = True

                        If language.Key.CanBeCached Then
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

            If params.Length = 1 Then
                If Not params(Scan0).ParameterType Is GetType(String) Then
                    Return Nothing
                Else
                    languages(tag) = Function(symbol, env) api.Invoke(Nothing, {symbol})
                End If
            ElseIf params.Length > 2 Then
                Return Nothing
            ElseIf params(1).ParameterType Is GetType(Environment) Then
                languages(tag) = Function(symbol, env) api.Invoke(Nothing, {symbol, env})
            End If

            Return Nothing
        End Function

    End Class
End Namespace