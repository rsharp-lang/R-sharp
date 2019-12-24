Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Interop

<HideModuleName>
Public Module Extensions

    <DebuggerStepThrough>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsRReturn(Of T)(x As T) As RReturn
        Return New RReturn(x)
    End Function
End Module
