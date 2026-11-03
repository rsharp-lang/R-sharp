Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Report
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.OpenXPS
Imports Microsoft.VisualBasic.MIME.RTF
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("report")>
Public Module report

    ReadOnly Utf8 As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

    ''' <summary>
    ''' 把内容块渲染成目标文档并落盘，返回文件的绝对路径。
    ''' <para>
    ''' 支持的扩展名：<c>.md</c> / <c>.markdown</c> / <c>.txt</c> / <c>.html</c> /
    ''' <c>.docx</c> / <c>.pdf</c> / <c>.rtf</c> / <c>.xps</c> / <c>.oxps</c>。
    ''' </para>
    ''' </summary>
    ''' 
    <ExportAPI("render")>
    <RApiReturn(TypeCodes.boolean)>
    Public Function render(<RRawVectorArgument(GetType(Block))> blockSet As Object, file As String,
                           Optional title As String = Nothing,
                           Optional author As String = Nothing,
                           Optional toc As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        Dim path As String = System.IO.Path.GetFullPath(file)
        Dim content As Block()
        Dim ext As String = path.ExtensionSuffix(dotPrefix:=True)
        Dim list As PipeIterator(Of Block) = pipeline.Stream(Of Block)(blockSet, env)

        If list.isError Then
            Return list.getError
        Else
            content = list.ToArray
        End If

        Call path.ParentPath.MakeDir

        Select Case ext
            Case ".md", ".markdown"
                Return content.ToMarkdown.SaveTo(path, Utf8)
            Case ".txt", ".text"
                Return content.ToPlainText.SaveTo(path, Utf8)
            Case ".html", ".htm"
                Return content.ToHtml(If(title, path.BaseName)).SaveTo(path, Utf8)
            Case ".docx"
                Dim renderer As New WordRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".pdf"
                Dim renderer As New PdfRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".rtf"
                Dim renderer As New RtfRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case ".xps", ".oxps"
                Dim renderer As New XpsRender With {
                    .DocumentTitle = If(title, ""),
                    .Author = If(author, ""),
                    .TableOfContents = toc
                }

                Call renderer.Render(content)
                Call renderer.Save(path)
            Case Else
                Throw New NotSupportedException($"the report format '{ext}' of '{path}' is not supported; use md/html/txt/docx/pdf/rtf/xps")
        End Select

        Return True
    End Function

End Module
