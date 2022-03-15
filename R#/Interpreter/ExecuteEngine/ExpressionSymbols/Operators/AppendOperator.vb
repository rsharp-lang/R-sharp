#Region "Microsoft.VisualBasic::9995d2ec4d5022a0cba83bdfaea01cf1, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\AppendOperator.vb"

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

        Total Lines:   78
        Code Lines:    57
        Comment Lines: 9
        Blank Lines:   12
        File Size:     2.60 KB


    '     Class AppendOperator
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' append, not modify the source vector
    ''' 
    ''' ```
    ''' a &lt;&lt; b
    ''' ```
    ''' </summary>
    Public Class AppendOperator : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return target.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        Friend ReadOnly target, appendData As Expression

        Sub New(a As Expression, b As Expression)
            Me.target = a
            Me.appendData = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim x As Object = target.Evaluate(envir)
            Dim y As Object = appendData.Evaluate(envir)

            If x Is Nothing Then
                Return y
            ElseIf y Is Nothing Then
                Return x
            ElseIf x Is Nothing AndAlso y Is Nothing Then
                Return Nothing
            End If

            Dim type1 As Type = x.GetType
            Dim type2 As Type = y.GetType

            If type1.IsArray OrElse type1 Is GetType(vector) Then
                ' y should be vector
                ' execute the append
                Return Runtime.asVector(Of Object)(x) _
                    .AsObjectEnumerator _
                    .JoinIterates(Runtime.asVector(Of Object)(y).AsObjectEnumerator) _
                    .ToArray
            ElseIf type1 Is GetType(list) Then
                If type2 Is GetType(list) Then
                ElseIf type2.IsArray Then
                Else
                    Return Internal.debug.stop(New InvalidProgramException(type2.FullName), envir)
                End If
                Return Internal.debug.stop(New NotImplementedException, envir)
            End If

            Return Runtime.asVector(Of Object)(x) _
                .AsObjectEnumerator _
                .JoinIterates(Runtime.asVector(Of Object)(y).AsObjectEnumerator) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"Call base::vec::append({target}, {appendData})"
        End Function
    End Class
End Namespace
