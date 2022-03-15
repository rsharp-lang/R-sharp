#Region "Microsoft.VisualBasic::83f0a1efb99e823925b3af5a262a2255, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Regexp.vb"

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

    '   Total Lines: 68
    '    Code Lines: 53
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 2.26 KB


    '     Class Regexp
    ' 
    '         Properties: expressionName, pattern, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Matches, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Development.Package.File
Imports r = System.Text.RegularExpressions.Regex

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' a regular expression literal syntax
    ''' </summary>
    Public Class Regexp : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolRegexp
            End Get
        End Property

        Public ReadOnly Property pattern As String

        Sub New(regexp As String)
            pattern = regexp
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return New r(pattern, RegexICSng)
        End Function

        Public Overrides Function ToString() As String
            Return $"/{pattern}/"
        End Function

        Public Shared Function Matches(r As Regex, text As Expression, env As Environment) As Object
            Dim strData As Object = text.Evaluate(env)

            If Program.isException(strData) Then
                Return strData
            ElseIf strData Is Nothing Then
                Return Nothing
            End If

            Dim inputs As String() = Runtime.asVector(Of String)(strData)

            If inputs.Length = 1 Then
                Return r.Matches(inputs(Scan0)).ToArray
            Else
                Return inputs _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i + 1}]]",
                                  Function(str)
                                      Return CObj(r.Matches(str.value).ToArray)
                                  End Function) _
                    .DoCall(Function(data)
                                Return New list With {.slots = data}
                            End Function)
            End If
        End Function
    End Class
End Namespace
