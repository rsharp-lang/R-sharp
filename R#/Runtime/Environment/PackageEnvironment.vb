Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Class PackageEnvironment : Inherits Environment

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(globalEnv As GlobalEnvironment, packageName As String)
            Call MyBase.New(globalEnv, pkgFrame(packageName), isInherits:=False)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Private Shared Function pkgFrame(pkgName As String) As StackFrame
            Return New StackFrame With {
                .File = pkgName,
                .Line = "n/a",
                .Method = New Method With {
                   .Method = "loadEnvironment",
                   .[Module] = "package",
                   .[Namespace] = pkgName
                }
            }
        End Function

        Public Function SetPackage() As PackageEnvironment

        End Function
    End Class
End Namespace