Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' the ``R#`` cloud lambda feature.(RPC)
''' </summary>
<Package("lambda")>
Module Lambda

    ''' <summary>
    ''' start the lambda cloud services engine.
    ''' </summary>
    ''' <param name="port"></param>
    ''' <returns></returns>
    Public Function start(port As Integer) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' run ``R#`` expression on remote lambda services engine
    ''' </summary>
    ''' <param name="lambda"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function runLambda(<RLazyExpression> lambda As Expression, Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function
End Module
