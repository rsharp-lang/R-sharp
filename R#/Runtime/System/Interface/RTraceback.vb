Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Components.Interface

    Public Interface IRuntimeTrace

        ReadOnly Property stackFrame As StackFrame

    End Interface

    Public Interface INamespaceReferenceSymbol

        ReadOnly Property [namespace] As String

    End Interface
End Namespace