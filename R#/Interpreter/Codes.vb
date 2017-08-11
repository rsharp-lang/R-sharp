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