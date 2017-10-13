Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.PrimitiveTypes

    Module Core

        Friend NotInheritable Class TypeDefine(Of T)

            Private Sub New()
            End Sub

            Shared singleType, collectionType As Type

            Public Shared Function GetSingleType() As Type
                If singleType Is Nothing Then
                    singleType = GetType(T)
                End If
                Return singleType
            End Function

            Public Shared Function GetCollectionType() As Type
                If collectionType Is Nothing Then
                    collectionType = GetType(IEnumerable(Of T))
                End If
                Return collectionType
            End Function
        End Class

        ''' <summary>
        ''' The ``In`` operator
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        Public Function op_In(Of T)(x As Object, collection As IEnumerable(Of T)) As IEnumerable(Of Boolean)
            With collection.AsList
                If x Is Nothing Then
                    Return {}
                Else
                    Dim type As Type = x.GetType

                    If type Is TypeDefine(Of T).GetSingleType Then
                        Return { .IndexOf(DirectCast(x, T)) > -1}
                    ElseIf type.ImplementsInterface(TypeDefine(Of T).GetCollectionType) Then
                        Return DirectCast(x, IEnumerable(Of T)).Select(Function(n) .IndexOf(n) > -1)
                    Else
                        Throw New InvalidOperationException(type.FullName)
                    End If
                End If
            End With
        End Function

        ''' <summary>
        ''' Generic ``+`` operator for numeric type
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function Add(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            ' add with 0
            If x Is Nothing Then
                Return y
            ElseIf y Is Nothing Then
                Return x
            End If

            Dim xtype As Type = x.GetType
            Dim ytype As Type = y.GetType

            If xtype Is TypeDefine(Of TX).GetSingleType Then

                If ytype Is TypeDefine(Of TY).GetSingleType Then
                    Return {DirectCast(x + y, TOut)}
                ElseIf ytype.ImplementsInterface(TypeDefine(Of TY).GetCollectionType) Then
                    Return DirectCast(y, IEnumerable(Of TY)).Select(Function(yi) DirectCast(x + yi, TOut))
                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            ElseIf xtype.ImplementsInterface(TypeDefine(Of TX).GetCollectionType) Then

                If ytype Is TypeDefine(Of TY).GetSingleType Then
                    Return DirectCast(x, IEnumerable(Of TX)).Select(Function(xi) DirectCast(xi + y, TOut))
                ElseIf ytype.ImplementsInterface(TypeDefine(Of TY).GetCollectionType) Then

                    Dim xlist = DirectCast(x, IEnumerable(Of TX)).ToArray
                    Dim ylist = DirectCast(y, IEnumerable(Of TY)).ToArray

                    If xlist.Length <> ylist.Length Then
                        Throw New InvalidOperationException("Vector length between the X and Y should be equals!")
                    Else
                        Return xlist _
                            .SeqIterator _
                            .Select(Function(xi)
                                        Return DirectCast(CObj(xi.value) + CObj(ylist(xi)), TOut)
                                    End Function)
                    End If
                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            Else
                Throw New InvalidCastException(xtype.FullName)
            End If
        End Function
    End Module
End Namespace