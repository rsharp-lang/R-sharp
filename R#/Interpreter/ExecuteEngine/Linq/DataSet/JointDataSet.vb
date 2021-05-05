Imports Microsoft.VisualBasic.My.JavaScript
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data set result of left join
    ''' </summary>
    Public Class JointDataSet : Inherits DataSet

        Dim main As DataSet
        Dim mainSymbol As String

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
            Dim raw As Object() = main.PopulatesData.ToArray
            Dim joinSeq As JavaScriptObject() = New JavaScriptObject(raw.Length - 1) {}
            Dim rightSeq As Dictionary(Of String, JavaScriptObject) = right _
                .PopulatesData _
                .Select(Function(a)
                            Return DirectCast(a, JavaScriptObject)
                        End Function) _
                .GroupBy(Function(a) any.ToString(a(rightKey))) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.First
                              End Function)
            Dim leftQuery As String

            For i As Integer = 0 To raw.Length - 1
                joinSeq(i) = DirectCast(raw(i), JavaScriptObject)
                leftQuery = any.ToString(joinSeq(i)(mainKey))

                If rightSeq.ContainsKey(leftQuery) Then
                    ' join two data
                    joinSeq(i) = JavaScriptObject.Join(joinSeq(i), rightSeq(leftQuery))
                End If
            Next

            main = New RuntimeVectorDataSet(joinSeq)

            Return Nothing
        End Function

        Friend Overrides Function PopulatesData() As IEnumerable(Of Object)
            Return main.PopulatesData
        End Function
    End Class
End Namespace