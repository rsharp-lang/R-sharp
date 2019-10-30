Imports System.Runtime.CompilerServices

Namespace Runtime

    ''' <summary>
    ''' Helper methods for add .NET function into <see cref="Environment"/> target
    ''' </summary>
    Public Module ImportsPackage

        <Extension>
        Public Sub ImportsStatic(envir As Environment, package As Type)

        End Sub

        Public Sub ImportsInstance(envir As Environment, target As Object)

        End Sub
    End Module
End Namespace
