
Imports System.Runtime.InteropServices
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax

    Public Module Helper

        ''' <summary>
        ''' check current line input is string stack open?
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="string">get last open string part value</param>
        ''' <returns></returns>
        Public Function IsStringOpen(line As String, <Out> ByRef [string] As String) As Boolean
            Dim tokens As Token() = Rscript.GetTokens(line)

            If tokens.Length > 0 AndAlso tokens.Last.name = TokenType.invalid Then
                ' string literal no stack close will create invalid syntax token
                ' 
                Dim last As Token = tokens.Last
                Dim s As String = last.text

                If Not s.StringEmpty Then
                    If s.StartsWith("""") OrElse s.StartsWith("'") Then
                        [string] = s.Substring(1)
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        Public Function EndWithIdentifier(line As String, <Out> ByRef symbol As String) As Boolean
            Dim tokens As Token() = Rscript.GetTokens(line)

            If tokens.Length > 0 AndAlso (tokens.Last.name = TokenType.identifier OrElse tokens.Last.name = TokenType.keyword) Then
                Dim last As Token = tokens.Last
                Dim s As String = last.text

                If Not s.StringEmpty Then
                    symbol = s
                    Return True
                End If
            End If

            Return False
        End Function
    End Module
End Namespace