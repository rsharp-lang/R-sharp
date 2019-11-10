Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Package

    Public Class Package

        Public Property info As PackageAttribute
        Public Property package As Type

        Public ReadOnly Property [namespace] As String
            Get
                Return info.Namespace
            End Get
        End Property

        Sub New(info As PackageAttribute, package As Type)
            Me.info = info
            Me.package = package
        End Sub

        Public Overrides Function ToString() As String
            Return $"{info.Namespace}: {info.Description}"
        End Function
    End Class
End Namespace