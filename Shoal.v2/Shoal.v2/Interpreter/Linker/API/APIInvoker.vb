Imports Microsoft.VisualBasic.Scripting.ShoalShell.Interpreter.Linker.APIHandler.Alignment.FunctionCalls

Namespace Interpreter.Linker.APIHandler

    Public Module APIInvoker

        ''' <summary>
        ''' 参数的位置适合函数的定义是一一对应的
        ''' </summary>
        ''' <param name="API"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function TryInvoke(API As APIEntryPoint, args As Object()) As Object
            Dim LQuery = (From Method As SignedFuncEntryPoint
                              In API.AsParallel
                          Let ref = __tryInvoke(Method.EntryPoint.EntryPoint, args)
                          Where Not ref Is Nothing
                          Select Method, ref
                          Order By ref.Score Descending).ToArray

            If LQuery.IsNullOrEmpty Then
                Throw New Runtime.Exceptions.MethodNotFoundException(
                    API,
                    (From obj In args Select New KeyValuePair(Of String, Object)(ShoalShell.Interpreter.Linker.APIHandler.Alignment.OrderReference, obj)).ToArray,
                    Nothing,
                    Nothing)
            End If

            Dim APIEntryPoint = LQuery(Scan0)
            Dim value As Object = APIEntryPoint.Method.EntryPoint.DirectInvoke(APIEntryPoint.ref.args)
            Return value
        End Function

        Private Function __tryInvoke(FuncDef As System.Reflection.MethodInfo, args As Object()) As ParamAlignments
            Dim ref = APIHandler.Alignment.FunctionCalls.OrderReferenceAlignment(FuncDef.GetParameters, args)
            Return ref
        End Function
    End Module
End Namespace