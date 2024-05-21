#Region "Microsoft.VisualBasic::27b0d5e7e0250ac58c1affa057a26504, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\StringInterpolation.vb"

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

    '   Total Lines: 127
    '    Code Lines: 92 (72.44%)
    ' Comment Lines: 13 (10.24%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 22 (17.32%)
    '     File Size: 4.69 KB


    '     Class StringInterpolation
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, r_string_concatenate, ToString, UnsafeStringConcatenate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime
Imports vector = SMRUCC.Rsharp.Runtime.Internal.Object.vector

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' A typescript liked string interpolation syntax
    ''' </summary>
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
            Dim current As Array = CLRVector.asCharacter(stringParts(Scan0).Evaluate(envir))
            Dim [next] As Object
            Dim str_concatenate As New op_evaluator(AddressOf r_string_concatenate)

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

                [next] = StringBinaryExpression.DoStringBinary(Of String)(
                    a:=current,
                    b:=[next],
                    op:=str_concatenate,
                    env:=envir
                )

                If TypeOf [next] Is Message Then
                    Return [next]
                Else
                    current = DirectCast([next], String())
                End If
            Next

            Dim currentStrings As String() = CLRVector.asCharacter(current)
            ' .Select(Function(str) sprintf(str)) _
            ' .ToArray
            Dim strVec As New vector(currentStrings, RType.GetRSharpType(GetType(String)))

            Return strVec
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strs">the string collection must be checked for the size before call this function</param>
        ''' <returns></returns>
        Public Shared Function UnsafeStringConcatenate(strs As IEnumerable(Of String()), Optional sep As String = "") As String()
            Dim pullAll As String()() = strs.ToArray
            Dim current As String() = pullAll(Scan0)
            Dim str_concatenate As New op_evaluator(AddressOf r_string_concatenate)
            Dim hasDelimiter As String = Not sep.StringEmpty(whitespaceAsEmpty:=False)

            If pullAll.Length = 1 Then
                Return current
            End If

            For Each [next] As String() In pullAll.Skip(1)
                If hasDelimiter Then
                    current = StringBinaryExpression.DoStringBinary(Of String)(
                        a:=current,
                        b:=sep,
                        op:=str_concatenate,
                        env:=Nothing
                    )
                End If

                current = StringBinaryExpression.DoStringBinary(Of String)(
                    a:=current,
                    b:=[next],
                    op:=str_concatenate,
                    env:=Nothing
                )
            Next

            Return current
        End Function

        Friend Shared Function r_string_concatenate(x As Object, y As Object, env As Environment) As Object
            If x Is Nothing OrElse y Is Nothing Then
                Call env.AddMessage("One of the string value is nothing!", MSG_TYPES.WRN)
            End If

            Return CStr(x) & CStr(y)
        End Function

        Public Overrides Function ToString() As String
            Return stringParts.JoinBy(" & ")
        End Function
    End Class
End Namespace
