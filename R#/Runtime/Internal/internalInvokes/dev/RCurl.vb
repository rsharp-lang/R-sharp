﻿Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module RCurl

        ''' <summary>
        ''' ### Infer the MIME type from a file name
        ''' 
        ''' This function returns the MIME type, i.e. part of the value 
        ''' used in the Content-Type for an HTTP request/response or in
        ''' email to identify the nature of the content. This is a string
        ''' such as "text/plain" or "text/xml" or "image/png".
        ''' 
        ''' The Function consults an R Object constructed by reading a 
        ''' Web site Of known MIME types (Not necessarily all) And 
        ''' matching the extension Of the file name To the names Of that 
        ''' table.
        ''' </summary>
        ''' <param name="name">character vector of file names</param>
        ''' <param name="[default]">
        ''' the value to use if no MIME type is found in the table for 
        ''' the given file name/extension.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("guessMIMEType")>
        Public Function guessMIMEType(<RRawVectorArgument>
                                      name As Object,
                                      Optional [default] As String = "application/octet-stream",
                                      Optional env As Environment = Nothing) As Object

            Return env.EvaluateFramework(Of String, String)(
                name,
                eval:=Function(filename)
                          Dim mime As ContentType = filename.FileMimeType(defaultUnknown:=False)

                          If mime.IsEmpty Then
                              Return [default]
                          Else
                              Return mime.MIMEType
                          End If
                      End Function)
        End Function
    End Module
End Namespace