#Region "Microsoft.VisualBasic::b3b1c89d657af9cc1c6ddc11904200e5, D:/GCModeller/src/R-sharp/R#//Runtime/Interop/RsharpApi/ApiDocumentHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 76
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.87 KB


    '     Module ApiDocumentHelper
    ' 
    '         Function: (+2 Overloads) markdown
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Interop

    Module ApiDocumentHelper

        <Extension>
        Public Function markdown(api As RMethodInfo) As String
            Dim raw As Type = api.GetNetCoreCLRDeclaration().DeclaringType
            Dim rawDeclare$ = raw.FullName
            Dim packageName$ = raw.NamespaceEntry(True).Namespace
            Dim params$
            Dim returns As IRType() = RApiReturnAttribute _
                .GetActualReturnType(api.GetNetCoreCLRDeclaration) _
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
    return call ``R#.interop_[{raw.Name}::{api.GetNetCoreCLRDeclaration().Name}]``(...);
}}"
        End Function

        <Extension>
        Public Function markdown(arg As RMethodArgument) As String
            Dim defaultValue As String = "``<NULL>``"
            Dim arg_default As Object = arg.default

            If arg_default Is Nothing Then
                defaultValue = "``<NULL>``"
            ElseIf arg.isOptional Then
                If arg_default.GetType Is GetType(String) Then
                    defaultValue = $"""{arg_default}"""
                ElseIf arg.type.raw.IsEnum Then
                    defaultValue = enumPrinter.defaultValueToString(arg_default, arg.type)
                ElseIf Not arg.defaultScriptValue Is Nothing Then
                    defaultValue = $"'{arg.defaultScriptValue.defaultValue}'"
                ElseIf arg_default.GetType.IsArray Then
                    defaultValue = JSON.GetObjectJson(arg_default.GetType, arg_default, indent:=False)
                Else
                    defaultValue = arg_default.ToString.ToUpper
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
