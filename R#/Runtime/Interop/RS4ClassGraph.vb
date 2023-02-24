Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Interop

    ''' <summary>
    ''' An clr interface liked type define in R#
    ''' </summary>
    Public Class RS4ClassGraph : Implements IReflector

        Public Property className As String
        Public Property members As String()

        Sub New()
        End Sub

        Sub New(name As String, ParamArray [interface] As String())
            Me.className = name
            Me.members = [interface]
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements IReflector.getNames
            Return members.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"{className}: {members.GetJson}"
        End Function

    End Class
End Namespace