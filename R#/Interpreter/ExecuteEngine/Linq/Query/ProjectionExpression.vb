Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' from ... select ...
    ''' </summary>
    Public Class ProjectionExpression : Inherits QueryExpression

        Dim opt As Options
        Dim project As OutputProjection

        Public Overrides ReadOnly Property name As String
            Get
                Return "from ... [select ...]"
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression), proj As OutputProjection, opt As Options)
            Call MyBase.New(symbol, sequence, exec)

            Me.opt = opt
            Me.project = proj

            If proj Is Nothing Then
                ' 当不存在投影表达式的时候
                ' 默认返回所有的symbol
                If symbol.isTuple Then
                    Dim fields = DirectCast(symbol.symbol, VectorLiteral).elements _
                        .Select(Function(a)
                                    Return New NamedValue(Of Expression)(a.ToString, a)
                                End Function) _
                        .ToArray

                    project = New OutputProjection(fields)
                Else
                    project = New OutputProjection({New NamedValue(Of Expression)("*", symbol.symbol)})
                End If
            End If
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
            Dim closure As New ExecutableContext(New Environment(context, context.stackFrame, isInherits:=False))
            Dim skipVal As Boolean
            Dim dataset As DataSet = GetDataSet(context)
            Dim err As Message = symbol.Exec(closure)

            If Not err Is Nothing Then
                Return err
            End If

            For Each item As Object In dataset.PopulatesData()
                err = symbol.SetValue(item, closure)

                If Not err Is Nothing Then
                    Return err
                End If

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