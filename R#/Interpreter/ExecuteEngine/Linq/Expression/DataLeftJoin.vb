#Region "Microsoft.VisualBasic::985f29d0e5936046a9edbf19eaef4965, R-sharp\R#\Interpreter\ExecuteEngine\Linq\Expression\DataLeftJoin.vb"

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

    '   Total Lines: 59
    '    Code Lines: 43
    ' Comment Lines: 3
    '   Blank Lines: 13
    '     File Size: 1.96 KB


    '     Class DataLeftJoin
    ' 
    '         Properties: keyword
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateFields, Exec, FindKeySymbol, (+2 Overloads) SetKeyBinary, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class DataLeftJoin : Inherits LinqKeywordExpression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "JOIN"
            End Get
        End Property

        ''' <summary>
        ''' join xxx in xxx
        ''' </summary>
        Friend ReadOnly anotherData As QuerySource

        Dim key1 As MemberReference
        Dim key2 As MemberReference

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            anotherData = New QuerySource(symbol, sequence)
        End Sub

        Public Function FindKeySymbol(side As String) As String
            If QuerySource.getSymbolName(key1.symbol) = side Then
                Return key1.memberName
            Else
                Return key2.memberName
            End If
        End Function

        Public Function SetKeyBinary(left As MemberReference, right As MemberReference) As DataLeftJoin
            key1 = left
            key2 = right

            Return Me
        End Function

        Public Function EnumerateFields() As IEnumerable(Of NamedValue(Of Expression))
            Return anotherData.EnumerateFields(addSymbol:=True)
        End Function

        Public Function SetKeyBinary(equivalent As BinaryExpression) As DataLeftJoin
            Return SetKeyBinary(equivalent.left, equivalent.right)
        End Function

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return anotherData.sequence.Exec(context)
        End Function

        Public Overrides Function ToString() As String
            Return New String() {
                $"JOIN {anotherData.symbol} IN {anotherData.sequence}",
                $"ON ({key1} == {key2})"
            }.JoinBy(vbCrLf)
        End Function
    End Class
End Namespace
