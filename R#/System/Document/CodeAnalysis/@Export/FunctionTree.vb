Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

Namespace Development.CodeAnalysis

    Public Class FunctionTree : Inherits TreeNodeBase(Of FunctionTree)

        Public Overrides ReadOnly Property MySelf As FunctionTree
            Get
                Return Me
            End Get
        End Property

        Public Property Symbol1 As SymbolTypeDefine
        Public Property FunctionTrace As New List(Of String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' the return value always not null
        ''' </returns>
        Public Function GetNode(name As String) As FunctionTree
            Dim find = ChildNodes.Where(Function(t) t.Name = name).FirstOrDefault

            If find Is Nothing Then
                find = New FunctionTree(name)
                AddChild(find)
            End If

            Return find
        End Function

    End Class
End Namespace