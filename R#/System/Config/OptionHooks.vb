Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.FrameworkInternal

Namespace Development.Configuration

    ''' <summary>
    ''' hooks for set options
    ''' </summary>
    Public Module OptionHooks

        ReadOnly hooks As New Dictionary(Of String, Action(Of String))

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="opt">
        ''' the option name
        ''' </param>
        ''' <param name="callback"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(opt As String, callback As Action(Of String))
            hooks(opt) = callback
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CheckHook(opt As String) As Boolean
            Return hooks.ContainsKey(opt)
        End Function

        Public Sub UpdateConfigurationCallback(opt As String, configVal As String)
            If hooks.ContainsKey(opt) Then
                Call hooks(opt)(configVal)
            Else
                ' do nothing
            End If
        End Sub

        Public Sub AddDefaultHooks()
            hooks("memory.load") = New Action(Of String)(AddressOf configMemoryLoads)
            hooks("memory.loads") = New Action(Of String)(AddressOf configMemoryLoads)
            hooks("avx_simd") = New Action(Of String)(AddressOf configAvxSIMD)
        End Sub

        Private Sub configMemoryLoads(optVal As String)
            If optVal = "max" Then
                Call FrameworkInternal.ConfigMemory(MemoryLoads.Heavy)
            Else
                Call FrameworkInternal.ConfigMemory(MemoryLoads.Light)
            End If
        End Sub

        Private Sub configAvxSIMD(optVal As String)
            If optVal.ParseBoolean Then
                SIMDEnvironment.config = SIMDConfiguration.auto
            Else
                SIMDEnvironment.config = SIMDConfiguration.disable
            End If
        End Sub

    End Module
End Namespace