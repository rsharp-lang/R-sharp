#Region "Microsoft.VisualBasic::0643a20bf77851a75ca6a0f6f338f679, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\StringInterpolation.vb"

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

    '     Class StringInterpolation
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.C
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class StringInterpolation : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.StringInterpolation
            End Get
        End Property

        ''' <summary>
        ''' 这些表达式产生的全部都是字符串结果值
        ''' </summary>
        Friend ReadOnly stringParts As Expression()

        Sub New(stringParts As Expression())
            Me.stringParts = stringParts
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim current As Array = REnv.asVector(Of String)(stringParts(Scan0).Evaluate(envir))
            Dim [next] As Object

            For Each part As Expression In stringParts.Skip(1)
                [next] = part.Evaluate(envir)

                If Program.isException([next]) Then
                    Return [next]
                End If

                With REnv.asVector(Of Object)([next])
                    If .Length = 1 Then
                        [next] = .GetValue(Scan0)
                    End If
                End With

                current = StringBinaryExpression.DoStringBinary(Of String)(
                    a:=current,
                    b:=[next],
                    op:=Function(x, y) x & y
                )
            Next

            Dim currentStrings As String() = DirectCast(REnv.asVector(Of String)(current), String())
            ' .Select(Function(str) sprintf(str)) _
            ' .ToArray
            Dim strVec As New vector(currentStrings, RType.GetRSharpType(GetType(String)))

            Return strVec
        End Function

        Public Overrides Function ToString() As String
            Return stringParts.JoinBy(" & ")
        End Function
    End Class
End Namespace
