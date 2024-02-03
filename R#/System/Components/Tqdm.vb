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

        ''' <summary>
        ''' wrap tqdm from this function
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Iterator Function getKeys() As IEnumerable(Of String)
            Dim bar As Tqdm.ProgressBar = Nothing
            Dim n As Integer = 0
            Dim size As Integer = list.length
            Dim d As Integer = size / 50

            For Each key As String In Tqdm.Wrap(list.slotKeys, bar:=bar)
                If d <= 1 OrElse (n Mod d = 0) Then
                    Call bar.SetLabel(key)
                End If

                n += 1

                Yield key
            Next
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