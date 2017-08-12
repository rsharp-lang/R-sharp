#Region "Microsoft.VisualBasic::6ec44276dbd8427c1463120ad05ef2cd, ..\R-sharp\R#\Interpreter\Codes.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Runtime.CodeDOM
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' The R# script code model
    ''' </summary>
    Public Class Codes : Inherits Main(Of LanguageTokens)

        Sub New(main As Main(Of LanguageTokens))
            Me.program = main.program
        End Sub

        Sub New(script As IEnumerable(Of Statement(Of LanguageTokens)))
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

            For Each line As Statement(Of LanguageTokens) In program
                statement = line.Parse
                last = statement _
                    .Evaluate(environment) _
                    .value
            Next

            Call environment.Push(".Last", last, TypeCodes.generic)

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
End Namespace
