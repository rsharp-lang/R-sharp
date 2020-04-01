Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.System.Package

Namespace Runtime.Interop

    Module ApiDocumentHelper

        <Extension>
        Public Function markdown(api As RMethodInfo) As String
            Dim raw As Type = api.GetRawDeclares().DeclaringType
            Dim rawDeclare$ = raw.FullName
            Dim packageName$ = raw.NamespaceEntry(True).Namespace
            Dim params$
            Dim returns As RType() = RApiReturnAttribute _
                .GetActualReturnType(api.GetRawDeclares) _
                .Select(AddressOf RType.GetRSharpType) _
                .ToArray

            If api.parameters.Length > 3 Then
                params = api.parameters.JoinBy(", " & vbCrLf)
            Else
                params = api.parameters.JoinBy(", ")
            End If

            Return $"let ``{api.name}`` as function({params}) -> ``{returns.JoinBy("|")}`` {{
    #
    # .NET API information
    #
    # module: {rawDeclare}
    # LibPath: {raw.Assembly.Location.ParentPath}
    # library: {raw.Assembly.Location.FileName}
    # package: ""{packageName}""
    #
    return call ``R#.interop_[{raw.Name}::{api.GetRawDeclares().Name}]``(...);
}}"
        End Function
    End Module
End Namespace