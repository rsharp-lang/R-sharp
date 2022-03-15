#Region "Microsoft.VisualBasic::939fa4fc77cbab317a72a72130a59441, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryBetweenExpression.vb"

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

    '   Total Lines: 107
    '    Code Lines: 85
    ' Comment Lines: 6
    '   Blank Lines: 16
    '     File Size: 4.41 KB


    '     Class BinaryBetweenExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: compareOf, Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class BinaryBetweenExpression : Inherits Expression

        ''' <summary>
        ''' left
        ''' </summary>
        Friend ReadOnly collectionSet As Expression
        ''' <summary>
        ''' right
        ''' </summary>
        Friend ReadOnly range As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        Sub New(collectionSet As Expression, range As Expression)
            Me.collectionSet = collectionSet
            Me.range = range
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim rangeVal As Object() = REnv.asVector(Of Object)(range.Evaluate(envir))
            Dim min As IComparable = rangeVal(Scan0)
            Dim max As IComparable = rangeVal(1)
            Dim values As Object() = REnv.asVector(Of Object)(collectionSet.Evaluate(envir))
            Dim flags As New List(Of Boolean)

            For Each a As Object In values
                Dim cmin = compareOf(a, min)
                Dim cmax = compareOf(a, max)

                If cmin Like GetType(Integer) AndAlso cmax Like GetType(Integer) Then
                    If cmin.VA >= 0 AndAlso cmax.VA <= 0 Then
                        flags.Add(True)
                    Else
                        flags.Add(False)
                    End If
                ElseIf cmin Like GetType(Exception) Then
                    Return Internal.debug.stop(cmin.TryCast(Of Exception)(), envir)
                Else
                    Return Internal.debug.stop(cmax.TryCast(Of Exception)(), envir)
                End If
            Next

            Return flags.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{collectionSet.ToString} BETWEEN {range}"
        End Function

        Friend Shared Function compareOf(a As IComparable, b As IComparable) As [Variant](Of Integer, Exception)
            If a Is Nothing AndAlso b Is Nothing Then
                Return 0
            ElseIf a Is Nothing Then
                Return -1
            ElseIf b Is Nothing Then
                Return 1
            End If

            If a.GetType Is b.GetType Then
                Return a.CompareTo(b)
            End If

            Dim t1 As Type = a.GetType
            Dim t2 As Type = b.GetType

            If t1 Is GetType(Decimal) OrElse t2 Is GetType(Decimal) Then
                Return CType(a, Decimal).CompareTo(CType(b, Decimal))
            ElseIf t1 Is GetType(Double) OrElse t2 Is GetType(Double) Then
                Return CType(a, Double).CompareTo(CType(b, Double))
            ElseIf t1 Is GetType(Single) OrElse t2 Is GetType(Single) Then
                Return CType(a, Single).CompareTo(CType(b, Single))
            ElseIf t1 Is GetType(Long) OrElse t2 Is GetType(Long) Then
                Return CType(a, Long).CompareTo(CType(b, Long))
            ElseIf t1 Is GetType(Integer) OrElse t2 Is GetType(Integer) Then
                Return CType(a, Integer).CompareTo(CType(b, Integer))
            ElseIf t1 Is GetType(Short) OrElse t2 Is GetType(Short) Then
                Return CType(a, Short).CompareTo(CType(b, Short))
            ElseIf t1 Is GetType(Byte) OrElse t2 Is GetType(Byte) Then
                Return CType(a, Byte).CompareTo(CType(b, Byte))
            ElseIf t1 Is GetType(Date) OrElse t2 Is GetType(Date) Then
                Return CType(a, Date).CompareTo(CType(b, Date))
            ElseIf t1 Is GetType(String) OrElse t2 Is GetType(String) Then
                Return CType(a, String).CompareTo(CType(b, String))
            End If

            Throw New NotImplementedException($"unsure how to compare between {t1.FullName} and {t2.FullName}!")
        End Function
    End Class
End Namespace
