#Region "Microsoft.VisualBasic::3142407d4affe424a53b3dabadbe6462, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryInExpression.vb"

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

    '   Total Lines: 169
    '    Code Lines: 129
    ' Comment Lines: 15
    '   Blank Lines: 25
    '     File Size: 6.90 KB


    '     Class BinaryInExpression
    ' 
    '         Properties: [operator], expressionName, left, right, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, findTest, getIndex, testListIndex, testVectorIndexOf
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.Operator
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' + 如果右边参数为序列，则是进行对值的indexOf操作
    ''' + 如果右边参数为列表，则是对key进行查找操作
    ''' </summary>
    Public Class BinaryInExpression : Inherits Expression
        Implements IBinaryExpression

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

        Public Property left As Expression Implements IBinaryExpression.left
        Public ReadOnly Property right As Expression Implements IBinaryExpression.right

        Public ReadOnly Property [operator] As String Implements IBinaryExpression.operator
            Get
                Return "in"
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">left</param>
        ''' <param name="b">right</param>
        Sub New(a As Expression, b As Expression)
            Me.left = a
            Me.right = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim sequence As Object = right.Evaluate(envir)
            Dim testLeft As Array = getIndex(left.Evaluate(envir))

            If sequence Is Nothing Then
                Return {}
            ElseIf RProgram.isException(sequence) Then
                Return sequence
            Else
                testLeft = REnv.TryCastGenericArray(testLeft, env:=envir)
            End If

            Dim flags As Boolean()
            Dim seqtype As Type = sequence.GetType
            ' 20220913 vector type implements the rnames
            ' interface but we usually needs to found data 
            ' in its values
            Dim isNameList As Boolean = seqtype IsNot GetType(vector) AndAlso
                seqtype.ImplementInterface(Of RNames) AndAlso
                REnv.MeasureRealElementType(testLeft) Is GetType(String)

            ' test string key in list index name?
            If isNameList Then
                flags = testListIndex(DirectCast(sequence, RNames), testLeft)
            Else
                ' try custom operator at first
                Dim op = BinaryOperatorEngine.getOperator("in", envir, suppress:=True)
                Dim left As RType = RType.GetRSharpType(testLeft.GetType)
                Dim right As RType = RType.GetRSharpType(sequence.GetType)

                If Not op Like GetType(Message) AndAlso op.TryCast(Of BinaryIndex).hasOperator(left, right) Then
                    Return op.TryCast(Of BinaryIndex).Evaluate(testLeft, sequence, envir)
                Else
                    ' and then try index hash
                    flags = testVectorIndexOf(getIndex(sequence).AsObjectEnumerator.Indexing, testLeft)
                End If
            End If

            Return flags
        End Function

        ''' <summary>
        ''' check keys in left is exists in the namelist of right?
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <param name="testLeft"></param>
        ''' <returns></returns>
        Private Shared Function testListIndex(sequence As RNames, testLeft As String()) As Boolean()
            Return testLeft _
                .Select(Function(a)
                            If a Is Nothing Then
                                Return False
                            Else
                                Return sequence.hasName(a)
                            End If
                        End Function) _
                .ToArray
        End Function

        Private Shared Function testVectorIndexOf(index As Index(Of Object), testLeft As Array) As Boolean()
            Dim rawIndexObjects As Object() = index.Objects
            Dim typeLeft = REnv.MeasureRealElementType(testLeft)
            Dim typeRight = REnv.MeasureRealElementType(rawIndexObjects)

            If typeLeft Is typeRight Then
                Select Case typeLeft
                    Case GetType(String)
                        Dim left As String() = testLeft.AsObjectEnumerator.Select(Function(o) CStr(o)).ToArray
                        Dim indexStr As Index(Of String) = rawIndexObjects.Select(Function(o) CStr(o)).Indexing

                        Return left.Select(Function(str) str Like indexStr).ToArray
                    Case Else
                        ' GoTo Generic
                End Select
            End If

Generic:
            Dim isComparable As Boolean = rawIndexObjects.All(Function(a) a.GetType.ImplementInterface(GetType(IComparable)))
            Dim findTest As Boolean() = testLeft _
                .AsObjectEnumerator _
                .Select(Function(x)
                            Return BinaryInExpression.findTest(x, isComparable, index, rawIndexObjects)
                        End Function) _
                .ToArray

            Return findTest
        End Function

        Private Shared Function findTest(x As Object, isComparable As Boolean, index As Index(Of Object), rawIndexObjects As Object()) As Boolean
            If x Like index Then
                Return True
            ElseIf isComparable AndAlso x.GetType.ImplementInterface(GetType(IComparable)) Then
                For Each y As Object In rawIndexObjects
                    Dim test = BinaryBetweenExpression.compareOf(x, y)

                    If test Like GetType(Exception) Then
                        ' can not compare between different type!
                        ' ignore
                    ElseIf test.TryCast(Of Integer) = 0 Then
                        Return True
                    End If
                Next

                Return False
            Else
                Return False
            End If
        End Function

        Private Shared Function getIndex(src As Object) As Array
            Dim isList As Boolean = False
            Dim seq = LinqQuery.produceSequenceVector(src, isList)

            If isList Then
                Return DirectCast(seq, KeyValuePair(Of String, Object)()) _
                    .Select(Function(t) t.Value) _
                    .ToArray
            Else
                Return REnv.asVector(Of Object)(seq)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} %in% index<{right}>)"
        End Function
    End Class
End Namespace
