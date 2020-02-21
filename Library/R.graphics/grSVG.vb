Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("grDevices.SVG")>
Module grSVG

    ''' <summary>
    ''' Add css style by css selector
    ''' </summary>
    ''' <param name="svg"></param>
    ''' <param name="selector$"></param>
    ''' <param name="styles"></param>
    ''' <returns></returns>
    <ExportAPI("styles")>
    Public Function styleByclass(svg As SVGData, selector$, <RListObjectArgument> styles As Object, Optional env As Environment = Nothing) As SVGData
        Dim args = DirectCast(styles, InvokeParameter()).Skip(1).ToArray
        Dim stylesVal As list

        If args.Length = 1 AndAlso Not args(Scan0).haveSymbolName AndAlso TypeOf args(Scan0).value Is FunctionInvoke Then
            Dim list As FunctionInvoke = args(Scan0).value

            If TypeOf list.funcName Is Literal AndAlso DirectCast(list.funcName, Literal).Evaluate(env) = "list" Then
                stylesVal = DirectCast(list.Evaluate(env), list)
            Else
                Throw New NotImplementedException
            End If
        Else
            Throw New NotImplementedException
        End If

        Dim cssVal$ = $"{selector} {{
{stylesVal.slots.Select(Function(t) $"{t.Key}: {t.Value};").JoinBy(vbCrLf)}
}}"

        svg.SVG.styles.Add(cssVal)

        Return svg
    End Function
End Module
