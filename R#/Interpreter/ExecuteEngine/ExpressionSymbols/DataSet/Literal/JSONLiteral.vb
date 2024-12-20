﻿#Region "Microsoft.VisualBasic::f6454cab64e0729881de57dd5e0d2b7d, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Literal\JSONLiteral.vb"

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

    '   Total Lines: 69
    '    Code Lines: 50 (72.46%)
    ' Comment Lines: 6 (8.70%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 13 (18.84%)
    '     File Size: 2.15 KB


    '     Class JSONLiteral
    ' 
    '         Properties: expressionName, members, size, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 这个数据对象模型只兼容json object的申明
    ''' </remarks>
    Public Class JSONLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.list
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.JSONLiteral
            End Get
        End Property

        Public Property members As NamedValue(Of Expression)()

        Public ReadOnly Property size As Integer
            Get
                Return members.Length
            End Get
        End Property

        Sub New(members As IEnumerable(Of NamedValue(Of Expression)))
            Me.members = members.ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim json As New list With {.slots = New Dictionary(Of String, Object)}
            Dim value As Object

            For Each member As NamedValue(Of Expression) In members
                value = member.Value.Evaluate(envir)

                If TypeOf value Is Message Then
                    Return value
                Else
                    json.slots(member.Name) = value
                End If
            Next

            Return json
        End Function

        Public Overrides Function ToString() As String
            Dim memberStrings As String() = members _
                .Select(Function(m) $"  ""{m.Name}"": {m.Value.Indent}") _
                .ToArray

            Return $"{{
    {memberStrings.JoinBy("," & vbCrLf)}
}}"
        End Function
    End Class
End Namespace
