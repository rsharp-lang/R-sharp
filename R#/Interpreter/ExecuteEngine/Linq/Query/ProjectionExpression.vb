Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' from ... select ...
    ''' </summary>
    Public Class ProjectionExpression : Inherits QueryExpression

        Dim opt As Options
        Dim project As OutputProjection

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression), proj As OutputProjection, opt As Options)
            Call MyBase.New(symbol, sequence, exec)

            Me.opt = opt
            Me.project = proj
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>
        ''' array of <see cref="JavaScriptObject"/>
        ''' </returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim projections As New List(Of JavaScriptObject)
            Dim env As Environment = context.environment
            Dim closure As New ExecutableContext With {
                .env = New Environment(parent:=env),
                .throwError = context.throwError
            }
            Dim skipVal As Boolean
            Dim dataset As DataSet = GetDataSet(context)

            Call closure.AddSymbol(symbol.symbolName, symbol.type)

            For Each item As Object In dataset.PopulatesData()
                closure.env.FindSymbol(symbol.symbolName).value = item

                For Each line As Expression In executeQueue
                    If TypeOf line Is WhereFilter Then
                        skipVal = Not DirectCast(line.Exec(closure), Boolean)

                        If skipVal Then
                            Exit For
                        End If
                    End If
                Next

                If Not skipVal Then
                    projections.Add(project.Exec(closure))
                End If
            Next

            Return opt.RunOptionPipeline(projections, context).ToArray
        End Function
    End Class
End Namespace