Namespace Runtime.Interop

    ''' <summary>
    ''' 这个主要是针对plot泛型函数，因为泛型函数没有办法正常展示与绘图样式相关的函数参数，所以会需要使用这个参数来进行展示
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RPlotStyleCSSAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' the background color or texture image path
        ''' </summary>
        ''' <returns></returns>
        Public Property bg As String
        ''' <summary>
        ''' the main title
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String
        Public Property size As String
        Public Property padding As String
        Public Property mainTitleFont As String
        Public Property legendTitle As String
        Public Property legendTitleFont As String

    End Class
End Namespace