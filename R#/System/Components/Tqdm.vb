Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Components

    Public Class tqdmList

        Friend list As list

        Default Public ReadOnly Property Item(name As String) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return list.getByName(name)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getKeys() As IEnumerable(Of String)
            Return Tqdm.Wrap(list.getNames)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getValue(key As Object) As Object
            Return list.getByName(key.ToString)
        End Function

        Public Overrides Function ToString() As String
            Return list.ToString
        End Function

    End Class
End Namespace