Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime

    Module EnvironmentExtensions

        Public ReadOnly Property globalStackFrame As StackFrame
            Get
                Return New StackFrame With {
                    .File = "<globalEnvironment>",
                    .Line = "n/a",
                    .Method = New Method With {
                        .Method = "<globalEnvironment>",
                        .[Module] = "global",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }
            End Get
        End Property

        <Extension>
        Public Function GetEnvironmentStackTraceString(env As Environment) As String
            Return env.parent?.ToString & " :> " & env.stackFrame.Method.ToString
        End Function

        Public Function CreateSpecialScriptReference(filepath As String) As list
            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"dir", filepath.ParentPath},
                    {"file", filepath.FileName},
                    {"fullName", filepath.GetFullPath}
                }
            }
        End Function
    End Module
End Namespace