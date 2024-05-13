#Region "Microsoft.VisualBasic::a1a3cbada2760de1172c84e53d657340, R#\Runtime\Environment\SymbolLanguageProcessor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 82
    '    Code Lines: 65
    ' Comment Lines: 0
    '   Blank Lines: 17
    '     File Size: 2.91 KB


    '     Class SymbolLanguageProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddSymbolLanguage, Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
                success = True
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
