Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class DotNetObject : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolIndex
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