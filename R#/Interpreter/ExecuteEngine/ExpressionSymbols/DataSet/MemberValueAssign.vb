#Region "Microsoft.VisualBasic::9c5e728703ec04a3731a6bf806870f8f, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\MemberValueAssign.vb"

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

        Total Lines:   45
        Code Lines:    36
        Comment Lines: 0
        Blank Lines:   9
        File Size:     1.74 KB


    '     Class MemberValueAssign
    ' 
    '         Properties: expressionName, memberReference, type, value
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
Imports SMRUCC.Rsharp.Development.Package.File
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class MemberValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public ReadOnly Property memberReference As SymbolIndexer
        Public ReadOnly Property value As Expression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolMemberAssign
            End Get
        End Property

        Sub New(member As SymbolIndexer, value As Expression)
            Me.memberReference = member
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As String() = REnv.asVector(Of String)(memberReference.index.Evaluate(envir))
            Dim target As Object = memberReference.symbol.Evaluate(envir)
            Dim list As RNameIndex = REnv.getFirst(target)
            Dim result As Object = Me.value.Evaluate(envir)

            If TypeOf list Is list AndAlso names.Length = 1 Then
                Return list.setByName(names(Scan0), result, envir)
            Else
                Return list.setByName(names, REnv.asVector(Of Object)(result), envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{memberReference} <- {value}"
        End Function
    End Class
End Namespace
