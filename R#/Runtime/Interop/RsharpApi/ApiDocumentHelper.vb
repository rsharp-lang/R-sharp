Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Interop

    Module ApiDocumentHelper

        <Extension> Public Function markdown(api As RMethodInfo) As String
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

        <Extension> Public Function markdown(arg As RMethodArgument) As String
            Dim defaultValue As String = "``<NULL>``"

            If arg.[default] Is Nothing Then
                defaultValue = "``<NULL>``"
            ElseIf arg.isOptional Then
                If arg.[default].GetType Is GetType(String) Then
                    defaultValue = $"""{arg.[default]}"""
                ElseIf arg.type.raw.IsEnum Then
                    defaultValue = enumPrinter.defaultValueToString(arg.[default], arg.type)
                ElseIf Not arg.defaultScriptValue Is Nothing Then
                    defaultValue = $"'{arg.defaultScriptValue.defaultValue}'"
                ElseIf arg.[default].GetType.IsArray Then
                    defaultValue = JSON.GetObjectJson(arg.[default].GetType, arg.[default], indent:=False)
                Else
                    defaultValue = arg.[default].ToString.ToUpper
                End If
            End If

            If arg.isObjectList Then
                Return "..."
            End If

            If arg.type.isEnvironment Then
                Return $"[``<Environment>``]"
            End If

            If arg.isOptional Then
                Return $"``{arg.name}`` as {arg.type} = {defaultValue}"
            Else
                Return $"``{arg.name}`` as {arg.type}"
            End If
        End Function
    End Module
End Namespace