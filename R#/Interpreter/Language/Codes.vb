#Region "Microsoft.VisualBasic::6d8bc7b5d20b22a3c3d4ebbbe8767a6d, R#\Interpreter\Language\Codes.vb"

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

    '     Class Codes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: RunProgram, TryParse
    ' 
    '     Class Token
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.CodeDOM
Imports RSharpLang = SMRUCC.Rsharp.Interpreter.Language.Tokens

Namespace Interpreter.Language

    ''' <summary>
    ''' The R# script code model
    ''' </summary>
    Public Class Codes : Inherits Main(Of Tokens)

        Sub New(main As Main(Of Tokens))
            Me.program = main.program
        End Sub

        Sub New(script As IEnumerable(Of Statement(Of Tokens)))
            Me.program = script.ToArray.Trim
        End Sub

        ''' <summary>
        ''' Run this R# script code model in a specific runtime environment.
        ''' (运行当前的这个解析出来的脚本程序)
        ''' </summary>
        ''' <param name="environment"></param>
        ''' <returns></returns>
        Public Function RunProgram(environment As Environment) As Object
            Dim last As Object = Nothing
            Dim statement As PrimitiveExpression

            For Each line As Statement(Of Tokens) In program
                statement = line.Parse
                last = statement _
                    .Evaluate(environment) _
                    .value
            Next

            environment(".Last").Value = last

            Return last
        End Function

        ''' <summary>
        ''' Parsing the R# script text as the script model.
        ''' </summary>
        ''' <param name="script$"></param>
        ''' <returns></returns>
        Public Shared Function TryParse(script$) As Codes
            Return New Codes(TokenIcer.Parse(script))
        End Function
    End Class

    Public Class Token : Inherits Token(Of RSharpLang)

        Sub New()
        End Sub

        Sub New(type As RSharpLang, text$)
            Call MyBase.New(type, text)
        End Sub

        Sub New(type As RSharpLang)
            Call MyBase.New(type)
        End Sub
    End Class
End Namespace
