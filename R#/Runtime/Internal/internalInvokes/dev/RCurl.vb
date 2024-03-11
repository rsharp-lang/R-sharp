#Region "Microsoft.VisualBasic::037f2b2aa79e842b00aa0655c4e1a6c9, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/dev/RCurl.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 48
    '    Code Lines: 24
    ' Comment Lines: 19
    '   Blank Lines: 5
    '     File Size: 2.06 KB


    '     Module RCurl
    ' 
    '         Function: guessMIMEType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    <Package("RCurl")>
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
        ''' <param name="default">
        ''' the value to use if no MIME type is found in the table for 
        ''' the given file name/extension.
        ''' </param>
        ''' <returns></returns>
        ''' <example>
        ''' guessMIMEType(["file.json" "data.dat" "image.png" "page.html"])
        ''' </example>
        <ExportAPI("guessMIMEType")>
        Public Function guessMIMEType(<RRawVectorArgument>
                                      name As Object,
                                      Optional [default] As String = "application/octet-stream",
                                      Optional env As Environment = Nothing) As Object

            Return env.EvaluateFramework(Of String, String)(
                name,
                eval:=Function(filename)
                          Dim mime As ContentType = filename.FileMimeType(defaultUnknown:=False)

                          If mime Is Nothing Then
                              Return [default]
                          Else
                              Return mime.MIMEType
                          End If
                      End Function)
        End Function
    End Module
End Namespace
