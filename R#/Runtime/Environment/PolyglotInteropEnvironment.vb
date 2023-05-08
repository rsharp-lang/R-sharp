Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    ''' <summary>
    ''' symbols solver for javascript/python reference to R# object
    ''' </summary>
    Public Class PolyglotInteropEnvironment : Inherits Environment

        Sub New(_global As GlobalEnvironment)
            Call MyBase.New(_global)

            ' set stack frame for current polyglot interop environment
            Call setStackInfo(New StackFrame With {
                .File = "runtime_polyglot_interop.vb",
                .Line = "999",
                .Method = New Method With {
                    .Method = "interop_call",
                    .[Module] = "PolyglotInteropEnvironment",
                    .[Namespace] = "SMRUCC_rsharp"
                }
            })
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddInteropSymbol(symbol As String, libs As Type())
            Call Push(
                name:=symbol,
                value:=hook_jsEnv(libs),
                [readonly]:=True,
                mode:=TypeCodes.list
            )
        End Sub

        Public Const pkg_ref_libs = "$_pkg_ref@-<libs!!!!!>*"

        ''' <summary>
        ''' construct the interop object for javascript/python
        ''' </summary>
        ''' <param name="libs"></param>
        ''' <returns></returns>
        Public Shared Function hook_jsEnv(ParamArray libs As Type()) As list
            Dim env As New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {pkg_ref_libs, libs}
                }
            }

            For Each type As Type In libs
                For Each func As NamedValue(Of MethodInfo) In ImportsPackage.GetAllApi(type)
                    Dim t As String() = func.Name.Split("."c)
                    Dim modObj As list = env

                    For Each ti As String In t
                        If Not modObj.hasName(ti) Then
                            Call modObj.add(ti, New list With {
                               .slots = New Dictionary(Of String, Object)
                            })
                        End If

                        modObj = modObj.getByName(ti)
                    Next

                    modObj.slots(t.Last) = New RMethodInfo(func)
                Next
            Next

            Return env
        End Function
    End Class
End Namespace