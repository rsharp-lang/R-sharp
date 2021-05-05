Imports Microsoft.VisualBasic.My.JavaScript

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data set result of left join
    ''' </summary>
    Public Class JointDataSet : Inherits DataSet

        ReadOnly main As DataSet
        ReadOnly mainSymbol As String
        ReadOnly joins As New List(Of Dictionary(Of String, JavaScriptObject))

        Sub New(symbol As String, main As DataSet)
            Me.main = main
            Me.mainSymbol = symbol
        End Sub

        Public Function Join(data As DataLeftJoin, context As ExecutableContext) As ErrorDataSet
            Dim right As DataSet = DataSet.CreateRawDataSet(data.Exec(context), context)

            If TypeOf right Is ErrorDataSet Then
                Return right
            End If

            Dim mainKey As String = data.FindKeySymbol(mainSymbol)
            Dim rightKey As String = data.FindKeySymbol(data.anotherData.symbolName)

            Return Nothing
        End Function

        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
            For Each obj As Object In main.PopulatesData

            Next
        End Function
    End Class
End Namespace