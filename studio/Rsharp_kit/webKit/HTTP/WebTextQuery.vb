Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Net.Http

Public Class WebTextQuery : Inherits WebQueryModule(Of String)
    Implements IHttpGet

    Public ReadOnly Property fs As IFileSystemEnvironment
        Get
            Return cache
        End Get
    End Property

    Sub New(dir As String)
        Call MyBase.New(dir)
    End Sub

    Sub New(fs As IFileSystemEnvironment)
        Call MyBase.New(fs)
    End Sub

    Public Function GetText(url As String) As String Implements IHttpGet.GetText
        Return QueryCacheText(url, cacheType:=".txt")
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="context">The query context is the url string</param>
    ''' <returns></returns>
    Protected Overrides Function doParseUrl(context As String) As String
        Return context
    End Function

    ''' <summary>
    ''' a general method just used for get html text
    ''' </summary>
    ''' <param name="html"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    Protected Overrides Function doParseObject(html As String, schema As Type) As Object
        Return html
    End Function

    Protected Overrides Function doParseGuid(context As String) As String
        Return MD5(context)
    End Function

    Protected Overrides Function contextPrefix(guid As String) As String
        If TypeOf cache Is Directory Then
            Return guid.Substring(2, 2)
        Else
            Return $"/.cache/${guid.Substring(2, 2)}"
        End If
    End Function
End Class
