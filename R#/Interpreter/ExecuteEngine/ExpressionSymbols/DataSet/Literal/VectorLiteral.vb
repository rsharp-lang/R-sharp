﻿#Region "Microsoft.VisualBasic::9b339de01109efe7f4a5263dff050ab0, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Literal\VectorLiteral.vb"

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

    '   Total Lines: 132
    '    Code Lines: 90 (68.18%)
    ' Comment Lines: 18 (13.64%)
    '    - Xml Docs: 72.22%
    ' 
    '   Blank Lines: 24 (18.18%)
    '     File Size: 4.64 KB


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
    ''' 1. vector literal
    ''' 
    ''' ```
    ''' [x,y,z,...]
    ''' ```
    ''' 
    ''' 2. vector append
    ''' 
    ''' ```
    ''' [x, ...y, ...z]
    ''' ```
    ''' </summary>
    ''' <remarks>
    ''' this object model implements the interface of a collection of the value <see cref="Expression"/>.
    ''' </remarks>
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
            Dim vector As New List(Of Object)
            Dim val As Object
            Dim expr As Expression

            For i As Integer = 0 To values.Length - 1
                expr = values(i)

                If TypeOf expr Is SymbolReference AndAlso DirectCast(expr, SymbolReference).symbol.StartsWith("...") Then
                    Dim [try] = expr.Evaluate(envir)

                    If TypeOf [try] Is Message Then
                        ' is vector append syntax
                        Dim srcSymbol As String = Mid(DirectCast(expr, SymbolReference).symbol, 4)

                        [try] = SymbolReference.GetReferenceObject(srcSymbol, env:=envir)

                        If TypeOf [try] Is Message Then
                            Return [try]
                        Else
                            Dim data As Array = REnv.asVector(Of Object)([try])

                            For index As Integer = 0 To data.Length - 1
                                Call vector.Add(data(index))
                            Next
                        End If
                    Else
                        ' isnot, add to current vector
                        vector.Add(REnv.single([try]))
                    End If
                Else
                    val = REnv.single(expr.Evaluate(envir))

                    If Program.isException(val) Then
                        Return val
                    Else
                        vector.Add(val)
                    End If
                End If
            Next

            Dim type As Type = MeasureRealElementType(vector.ToArray)

            If vector.Any(Function(a) a Is GetType(Void)) Then
                type = GetType(Object)
            End If

            If Not type Is GetType(Void) AndAlso Not type Is GetType(Object) Then
                Return New vector(type, vector, envir)
            Else
                Return Environment.asRVector(Me.type, vector.ToArray)
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
