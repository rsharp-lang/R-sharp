#Region "Microsoft.VisualBasic::9d7fee23043498a5dd1a07bf88bdf47c, R-sharp\R#\System\Document\ShellScript\DefaultFormatter.vb"

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


     Code Statistics:

        Total Lines:   46
        Code Lines:    38
        Comment Lines: 0
        Blank Lines:   8
        File Size:     1.85 KB


    '     Module DefaultFormatter
    ' 
    '         Function: (+3 Overloads) FormatDefaultString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports any = Microsoft.VisualBasic.Scripting

Namespace Development.CommandLine

    Module DefaultFormatter

        Public Function FormatDefaultString(expr As Expression) As String
            If TypeOf expr Is StringInterpolation Then
                Return FormatDefaultString(DirectCast(expr, StringInterpolation))
            ElseIf TypeOf expr Is Literal Then
                Return any.ToString(DirectCast(expr, Literal).value)
            ElseIf TypeOf expr Is FunctionInvoke Then
                Return FormatDefaultString(DirectCast(expr, FunctionInvoke))
            ElseIf TypeOf expr Is SymbolReference Then
                Return expr.ToString
            Else
                Return expr.ToString
            End If
        End Function

        Private Function FormatDefaultString(expr As FunctionInvoke) As String
            Dim args As String() = expr.parameters.Select(AddressOf FormatDefaultString).ToArray
            Dim name As String = FormatDefaultString(expr.funcName)

            Return $"{name}({args.JoinBy(", ")})"
        End Function

        Private Function FormatDefaultString(expr As StringInterpolation) As String
            Dim sb As New StringBuilder

            For Each part As Expression In expr.stringParts
                If TypeOf part Is Literal Then
                    sb.Append(any.ToString(DirectCast(part, Literal).value))
                Else
                    sb.Append($"${{{FormatDefaultString(part)}}}")
                End If
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
