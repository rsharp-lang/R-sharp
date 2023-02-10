#Region "Microsoft.VisualBasic::b9ace5070035c3a4a12c85cf19ca74f3, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SymbolIndexer\VectorLoop.vb"

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

    '   Total Lines: 147
    '    Code Lines: 102
    ' Comment Lines: 22
    '   Blank Lines: 23
    '     File Size: 5.47 KB


    '     Class VectorLoop
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, getListVector, getVectorList, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' create a new vector data which is introduced via 
    ''' the perl syntax:
    ''' 
    ''' ``list@name`` means ``sapply(list, x -> x$name);``
    ''' ``list@{name}`` means ``sapply(list, x -> x[[name]]);``
    ''' </summary>
    Public Class VectorLoop : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' the target member name
        ''' 
        ''' should be string literal or a symbol reference 
        ''' expression produce the variable name
        ''' </summary>
        Friend ReadOnly index As Expression
        ''' <summary>
        ''' target list symbol that will extract vector data
        ''' from it
        ''' </summary>
        Friend symbol As Expression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.VectorLoop
            End Get
        End Property

        Sub New(symbol As Expression, index As Expression)
            Me.symbol = symbol
            Me.index = index
        End Sub

        Public Overrides Function ToString() As String
            If TypeOf index Is Literal Then
                Return $"{symbol}@{index}"
            Else
                Return $"{symbol}@{{{index}}}"
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim data As Object = symbol.Evaluate(envir)
            Dim member As Object = index.Evaluate(envir)

            If Program.isException(data) Then
                Return data
            ElseIf Program.isException(member) Then
                Return member
            End If

            Dim memberName As String = any.ToString(REnv.single(member))

            If TypeOf data Is list Then
                Return getListVector(DirectCast(data, list), memberName, envir)
            ElseIf TypeOf data Is vector Then
                data = DirectCast(data, vector).data
            End If

            If data Is Nothing Then
                Return Nothing
            ElseIf data.GetType.IsArray Then
                Return getVectorList(data, memberName, envir)
            End If

            Return Message.InCompatibleType(GetType(list), data.GetType, envir)
        End Function

        Private Shared Function getVectorList(data As Array, memberName As String, envir As Environment)
            Dim vec As Object() = New Object(data.Length - 1) {}
            Dim item As Object

            For i As Integer = 0 To vec.Length - 1
                item = data(i)

                If item Is Nothing Then
                    vec(i) = Nothing
                ElseIf TypeOf item Is list Then
                    vec(i) = DirectCast(item, list).getByName(memberName)
                ElseIf TypeOf item Is dataframe Then
                    vec(i) = DirectCast(item, dataframe).getColumnVector(memberName)
                Else
                    Return Message.InCompatibleType(GetType(list), item.GetType, envir)
                End If
            Next

            Return REnv.TryCastGenericArray(vec, env:=envir)
        End Function

        Private Shared Function getListVector(datalist As list, memberName As String, envir As Environment)
            Dim vec As Object() = New Object(datalist.length - 1) {}
            Dim source As Object() = datalist.data.ToArray
            Dim item As Object

            ' for each element item in the source data list
            For i As Integer = 0 To vec.Length - 1
                item = source(i)

                If item Is Nothing Then
                    vec(i) = Nothing
                ElseIf TypeOf item Is list Then
                    vec(i) = DirectCast(source(i), list).getByName(memberName)
                ElseIf TypeOf item Is dataframe Then
                    vec(i) = DirectCast(item, dataframe).getColumnVector(memberName)
                Else
                    Return Message.InCompatibleType(GetType(list), item.GetType, envir)
                End If
            Next

            ' 20230206
            ' handling of the vector bugs
            If vec.All(Function(vi)
                           If vi Is Nothing Then
                               Return True
                           Else
                               ' vector length = 0: means nothing
                               ' vector length = 1: means scalar
                               Return TypeOf vi Is vector AndAlso DirectCast(vi, vector).length <= 1
                           End If
                       End Function) Then

                vec = vec _
                    .Select(Function(vi)
                                If vi Is Nothing Then
                                    Return Nothing
                                Else
                                    Return DirectCast(vi, vector).getByIndex(1)
                                End If
                            End Function) _
                    .ToArray
            End If

            Return REnv.TryCastGenericArray(vec, env:=envir)
        End Function
    End Class
End Namespace
