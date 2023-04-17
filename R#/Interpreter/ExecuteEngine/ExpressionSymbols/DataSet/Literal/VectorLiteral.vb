#Region "Microsoft.VisualBasic::8b98dbe5da8aba62bd6a1f3eb275551c, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/Literal/VectorLiteral.vb"

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

    '   Total Lines: 92
    '    Code Lines: 69
    ' Comment Lines: 5
    '   Blank Lines: 18
    '     File Size: 3.13 KB


    '     Class VectorLiteral
    ' 
    '         Properties: expressionName, length, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, FromArray, GetEnumerator, IEnumerable_GetEnumerator, ToArray
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ```
    ''' [x,y,z,...]
    ''' ```
    ''' </summary>
    Public Class VectorLiteral : Inherits Expression
        Implements IEnumerable(Of Expression)

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.VectorLiteral
            End Get
        End Property

        Public ReadOnly Property length As Integer
            Get
                Return values.Length
            End Get
        End Property

        Friend ReadOnly values As Expression()

        Sub New(values As Expression(), type As TypeCodes)
            Me.values = values
            Me.type = type
        End Sub

        Sub New(values As IEnumerable(Of Expression))
            Me.values = values.ToArray
            Me.type = Me.values.DoCall(AddressOf SyntaxImplements.TypeCodeOf)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim vector As Array = Array.CreateInstance(GetType(Object), values.Length)
            Dim val As Object
            Dim expr As Expression

            For i As Integer = 0 To values.Length - 1
                expr = values(i)
                val = REnv.single(expr.Evaluate(envir))

                If Program.isException(val) Then
                    Return val
                Else
                    vector(i) = val
                End If
            Next

            Dim type As Type = MeasureRealElementType(vector)

            If Not type Is GetType(Void) AndAlso Not type Is GetType(Object) Then
                Return New vector(type, vector, envir)
            Else
                Return Environment.asRVector(Me.type, vector)
            End If
        End Function

        Public Shared Function FromArray(ParamArray array As String()) As VectorLiteral
            Return New VectorLiteral(array.Select(Function(str) New Literal(str)))
        End Function

        Public Function ToArray() As Expression()
            Return values.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"[{values.JoinBy(", ")}]"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Expression) Implements IEnumerable(Of Expression).GetEnumerator
            For Each value As Expression In Me.values
                Yield value
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
