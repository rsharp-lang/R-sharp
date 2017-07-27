Namespace Library

    ''' <summary>
    ''' ``R#`` package manager
    ''' </summary>
    Public Class RPM

        ''' <summary>
        ''' Imports dotnet namespace and module API.
        ''' </summary>
        ''' <param name="namespace$"></param>
        ''' <returns></returns>
        Public Function [Imports](namespace$) As Dictionary(Of String, String())

        End Function

        ''' <summary>
        ''' Load R# package
        ''' </summary>
        ''' <param name="package$"></param>
        ''' <returns></returns>
        Public Function LibraryLoad(package$) As String()

        End Function
    End Class
End Namespace