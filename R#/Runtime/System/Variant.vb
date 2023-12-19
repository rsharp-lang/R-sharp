Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language

Namespace Runtime.Components

    Public Class [Variant](Of T) : Inherits Value(Of Object)

        Public ReadOnly Property CanbeCastTo As Boolean
            Get
                Return GetUnderlyingType() Is GetType(T) OrElse
                    GetUnderlyingType.IsInheritsFrom(GetType(T)) OrElse
                    (GetType(T).IsInterface AndAlso GetUnderlyingType.ImplementInterface(GetType(T)))
            End Get
        End Property

        Public Function [TryCast]() As T
            If CanbeCastTo Then
                Return DirectCast(Value, T)
            Else
                Return Nothing
            End If
        End Function

    End Class
End Namespace