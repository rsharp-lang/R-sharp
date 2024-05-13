#Region "Microsoft.VisualBasic::f6db8d2ff882eb8ac3aa78e5eae5e6e8, R#\Runtime\Interop\RsharpApi\MethodInvoke.vb"

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

    '   Total Lines: 48
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.65 KB


    '     Class MethodInvoke
    ' 
    '         Properties: isStatic, moduleNamespace
    ' 
    '         Function: Invoke, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Development.Package
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Interop

    Friend Class MethodInvoke

        Public method As MethodInfo
        Public target As Object

        Public ReadOnly Property isStatic As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return target Is Nothing
            End Get
        End Property

        Public ReadOnly Property moduleNamespace As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return method.DeclaringType.NamespaceEntry(nullWrapper:=True).Namespace
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Invoke(parameters As Object()) As Object
            Return method.Invoke(target, parameters)
        End Function

        <DebuggerStepThrough>
        Public Overrides Function ToString() As String
            Dim obj As String = If(isStatic, moduleNamespace, any.ToString(target, "NULL"))
            Dim exportName As String = ImportsPackage.GetExportName(method, strict:=False)
            Dim params = method _
                .GetParameters _
                .Select(Function(p) If(p.IsOptional, $"[{p.Name}]", p.Name)) _
                .ToArray
            Dim ftoString As String = $"{exportName}({params.JoinBy(", ")})"

            Return $"{obj}::{ftoString}"
        End Function

    End Class

End Namespace
