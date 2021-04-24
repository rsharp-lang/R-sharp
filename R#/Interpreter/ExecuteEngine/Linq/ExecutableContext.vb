Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ExecutableContext

        ReadOnly environment As Environment

        Public ReadOnly Property stackFrame As StackFrame
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return environment.stackFrame
            End Get
        End Property

        Public ReadOnly Property symbolNames As String()
            Get
                Return environment.symbols.Keys.ToArray
            End Get
        End Property

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(env As Environment)
            environment = env
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddSymbol(symbolName$, type As TypeCodes)
            Call environment.Push(symbolName, Nothing, [readonly]:=False, mode:=type)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindSymbol(symbolName As String) As Symbol
            Return environment.FindSymbol(symbolName)
        End Function

        ''' <summary>
        ''' set <paramref name="value"/> to target symbol <paramref name="name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetSymbol(name As String, value As Object)
            Call environment.FindSymbol(name).SetValue(value, Me)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return environment.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(context As ExecutableContext) As Environment
            Return context.environment
        End Operator
    End Class
End Namespace