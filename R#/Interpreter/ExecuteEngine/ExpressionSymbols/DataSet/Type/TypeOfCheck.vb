Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``typeof x is "export_typename"``
    ''' </summary>
    Public Class TypeOfCheck : Inherits ModeOf

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

        ''' <summary>
        ''' value of this property is usually comes from <see cref="RTypeExportAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property typeName As Expression

        Public Sub New(keyword As String, target As Expression, typeName As Expression)
            MyBase.New(keyword, target)

            Me.typeName = typeName
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim type = MyBase.Evaluate(envir)

            If Program.isException(type) Then
                Return type
            End If

            Dim typeRight As Object = typeName.Evaluate(envir)

            If Program.isException(typeRight) Then
                Return typeRight
            End If

            If TypeOf type Is RType Then
                If TypeOf typeRight Is RType Then
                    Return type Is typeRight
                ElseIf TypeOf typeRight Is String Then
                    Dim type2 = envir.globalEnvironment.types.TryGetValue(typeRight.ToString)

                    If type2 Is Nothing Then
                        Return Internal.debug.stop({
                            $"we are not able to find any information about the data type: '{typeRight}', please imports underlying package namespace at first!",
                            $"type name: {typeRight}"
                        }, envir)
                    Else
                        Return type Is type2
                    End If
                Else
                    Return Message.InCompatibleType(GetType(String), typeRight.GetType, envir)
                End If
            Else
                Return Message.InCompatibleType(GetType(String), typeRight.GetType, envir)
            End If
        End Function
    End Class
End Namespace