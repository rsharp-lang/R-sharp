#Region "Microsoft.VisualBasic::5b9b14fc47e6f0e2036867824de8d004, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\DotNetObject.vb"

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

        Total Lines:   70
        Code Lines:    55
        Comment Lines: 3
        Blank Lines:   12
        File Size:     2.72 KB


    '     Class DotNetObject
    ' 
    '         Properties: [object], expressionName, member, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' [x]::property, this syntax only works for the class property
    ''' </summary>
    Public Class DotNetObject : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.DotNetMemberReference
            End Get
        End Property

        Public ReadOnly Property [object] As Expression
        Public ReadOnly Property member As Expression

        Sub New(obj As Expression, member As Expression)
            Me.object = obj
            Me.member = member
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim objVal As Object = [object].Evaluate(envir)
            Dim memberName As String = ValueAssignExpression.GetSymbol(member)

            If Program.isException(objVal) Then
                Return objVal
            ElseIf TypeOf objVal Is vbObject Then
                objVal = DirectCast(objVal, vbObject).target
            End If

            If TypeOf objVal Is list Then
                Return DirectCast(objVal, list).getByName(memberName)
            Else
                Static schema As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

                Dim schemaTable = schema.ComputeIfAbsent(objVal.GetType, Function(cache) DataFramework.Schema(cache, PropertyAccess.Readable, nonIndex:=True))
                Dim reader As PropertyInfo = schemaTable.TryGetValue(memberName)

                If reader Is Nothing Then
                    If envir.globalEnvironment.options.strict Then
                        Return Internal.debug.stop($"can not found member symbol {memberName} in [{objVal.GetType.FullName}].", envir)
                    Else
                        Return Nothing
                    End If
                Else
                    Return reader.GetValue(objVal, Nothing)
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"[{[object]}]::{member}"
        End Function
    End Class
End Namespace
