#Region "Microsoft.VisualBasic::a12abb5478db43ff901319269d4c1738, R#\Interpreter\ExecuteEngine\ExpressionSymbols\MemberValueAssign.vb"

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

    '     Class MemberValueAssign
    ' 
    '         Properties: memberReference, type, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine

    Public Class MemberValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public ReadOnly Property memberReference As SymbolIndexer
        Public ReadOnly Property value As Expression

        Sub New(member As SymbolIndexer, value As Expression)
            Me.memberReference = member
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As String() = Runtime.asVector(Of String)(memberReference.index.Evaluate(envir))
            Dim list As RNameIndex = Runtime.getFirst(memberReference.symbol.Evaluate(envir))
            Dim value As Array = Runtime.asVector(Of Object)(Me.value.Evaluate(envir))

            If TypeOf list Is list AndAlso names.Length = 1 Then
                Return list.setByName(names(Scan0), value, envir)
            Else
                Return list.setByName(names, value, envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{memberReference} <- {value}"
        End Function
    End Class
End Namespace
