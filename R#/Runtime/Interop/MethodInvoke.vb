Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Runtime.Interop

    Friend Class MethodInvoke

        Public method As MethodInfo
        Public target As Object

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Invoke(parameters As Object()) As Object
            Return method.Invoke(target, parameters)
        End Function

    End Class
End Namespace