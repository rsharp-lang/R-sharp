
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("math")>
Module math

    ''' <summary>
    ''' Get the additive identity element for the type of x (x can also specify the type itself).
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("zero")>
    Public Function zero(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim vec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(x), env)
        Dim type As RType = RType.GetRSharpType(vec.GetType.GetElementType)

    End Function

End Module
