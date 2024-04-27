#Region "Microsoft.VisualBasic::36383d53bbdcc3d6ed4f3c37d047c1bc, E:/GCModeller/src/R-sharp/R#//Language/Syntax/Helper.vb"

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

    '   Total Lines: 80
    '    Code Lines: 55
    ' Comment Lines: 8
    '   Blank Lines: 17
    '     File Size: 3.02 KB


    '     Module Helper
    ' 
    '         Function: EndWithIdentifier, IsStringOpen, TryRequirePackage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
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

        ReadOnly likelySymbolName As Index(Of TokenType) = {TokenType.identifier, TokenType.keyword}

        Public Function EndWithIdentifier(line As String, <Out> ByRef symbol As String) As Boolean
            Dim tokens As Token() = Rscript.GetTokens(line)

            If tokens.Length > 0 AndAlso tokens.Last.name Like likelySymbolName Then
                Dim last As Token = tokens.Last
                Dim s As String = last.text

                If Not s.StringEmpty Then
                    symbol = s
                    Return True
                End If
            End If

            Return False
        End Function

        Public Function TryRequirePackage(line As String, <Out> ByRef package_prefix As String) As Boolean
            Dim tokens As Token() = Rscript.GetTokens(line)

            If tokens.Length >= 2 Then
                Dim last = tokens.Last

                If last.name Like likelySymbolName AndAlso tokens.Length >= 3 Then
                    Dim func = tokens(tokens.Length - 3)

                    If func.name Like likelySymbolName AndAlso (func.text = "require" OrElse func.text = "library") Then
                        package_prefix = last.text
                        Return True
                    End If
                ElseIf last.name = TokenType.open Then
                    Dim func = tokens(tokens.Length - 2)

                    If func.name Like likelySymbolName AndAlso (func.text = "require" OrElse func.text = "library") Then
                        package_prefix = ""
                        Return True
                    End If
                End If
            End If

            Return False
        End Function
    End Module
End Namespace
