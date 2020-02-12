Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime

    Module EnvironmentExtensions

        Public ReadOnly Property globalStackFrame As StackFrame
            Get
                Return New StackFrame With {
                    .File = "<globalEnvironment>",
                    .Line = "n/a",
                    .Method = New Method With {
                        .Method = "%global%",
                        .[Module] = "global",
                        .[Namespace] = "R#"
                    }
                }
            End Get
        End Property

        <Extension>
        Public Function GetEnvironmentStackTraceString(env As Environment) As String
            Return env.parent?.ToString & " :> " & env.stackFrame.Method.ToString
        End Function

    End Module
End Namespace