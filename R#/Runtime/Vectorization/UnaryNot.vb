Imports System.Runtime.CompilerServices

Namespace Runtime.Vectorization

    Module UnaryNot

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function [Not](x As Boolean()) As Boolean()
            Return (From xi As Boolean In x Select Not xi).ToArray
        End Function
    End Module
End Namespace