#Region "Microsoft.VisualBasic::15e267a0ce268f1c2ed32894bb0998ce, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Regexp.vb"

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

    '   Total Lines: 133
    '    Code Lines: 82 (61.65%)
    ' Comment Lines: 36 (27.07%)
    '    - Xml Docs: 72.22%
    ' 
    '   Blank Lines: 15 (11.28%)
    '     File Size: 5.85 KB


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
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports r = System.Text.RegularExpressions.Regex
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

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

        ' 20250410
        ' $"regexp"(...) # drop is default = FALSE, which means for multiple string will returns a tuple list for all matches
        ' $"regexp"(..., drop=TRUE) # drop is TRUE, which means regexp matches will returns a character vector for each
        '                           # element with first matched, empty string will be returns if corresponding element
        '                           # has no regexp matches.

        Public ReadOnly Property pattern As String

        Sub New(regexp As String)
            pattern = regexp
        End Sub

        ''' <summary>
        ''' just create a new regexp object for run text match
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return New r(pattern, RegexICSng)
        End Function

        Public Overrides Function ToString() As String
            Return $"/{pattern}/ig"
        End Function

        ''' <summary>
        ''' do regular expression pattern matches
        ''' </summary>
        ''' <param name="r"></param>
        ''' <param name="text"></param>
        ''' <param name="drop">controls of the function evaluation result</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' this function returns different result value based on the <paramref name="drop"/> option:
        ''' 
        ''' 1. drop = FALSE, for scalar string value, will returns a character vector that contains all matches result, 
        '''                  for multiple string input, this function will returns a list that contains all regexp 
        '''                  matches result for each elements string.
        '''                  
        ''' 2. drop = TRUE, for scalar string value, will returns a scalar character string of the first regexp matches
        '''                 result. for multiple string input, this function will returns a character vector which is 
        '''                 consist with the every first matches result of the multiple string input, empty string will 
        '''                 be placed in the result vector if the corresponding input string element has no matches 
        '''                 result.
        ''' </returns>
        Public Shared Function Matches(r As Regex, text As Expression, drop As Boolean, env As Environment) As Object
            Dim strData As Object = text.Evaluate(env)

            If Program.isException(strData) Then
                Return strData
            ElseIf strData Is Nothing Then
                Return Nothing
            ElseIf strData.GetType.ImplementInterface(Of RFunction) Then
                Return RInternal.debug.stop({
                    $"the given symbols is a function symbol, can not be apply for regular expression pattern matches!",
                    $"symbol expression: {text.ToString}",
                    $"symbol value: {any.ToString(strData)}"
                }, env)
            End If

            Dim inputs As String()

            Try
                inputs = CLRVector.asCharacter(strData)
            Catch ex As Exception
                Return RInternal.debug.stop(ex, env)
            End Try

            If inputs.Length = 1 Then
                Dim all As String() = r.Matches(inputs(Scan0)).ToArray

                If drop Then
                    ' just returns the first matche result if drop option is true
                    Return all.ElementAtOrDefault(Scan0)
                Else
                    Return all
                End If
            ElseIf drop Then
                ' product a character vector for first element hits
                Return inputs _
                    .Select(Function(str)
                                Dim all = r.Matches(str).ToArray
                                Dim first = all.ElementAtOrDefault(Scan0, [default]:="")
                                Return first
                            End Function) _
                    .ToArray
            Else
                ' product a list for all matches
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
