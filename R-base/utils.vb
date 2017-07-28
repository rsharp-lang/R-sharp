Imports System.ComponentModel
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

<Package("utils")>
<Description("")>
Public Module utils

    ''' <summary>
    ''' Reads a file in table format and creates a data frame from it, with cases corresponding to lines and variables to fields in the file.
    ''' </summary>
    ''' <param name="file$">
    ''' the name of the file which the data are to be read from. Each row of the table appears as one line of the file. If it does not contain an absolute path, the file name is relative to the current working directory, getwd(). Tilde-expansion is performed where supported. This can be a compressed file (see file).
    ''' Alternatively, file can be a readable text-mode connection (which will be opened for reading if necessary, And if so closed (And hence destroyed) at the end of the function call). (If stdin() Is used, the prompts for lines may be somewhat confusing. Terminate input with a blank line Or an EOF signal, Ctrl-D on Unix And Ctrl-Z on Windows. Any pushback on stdin() will be cleared before return.)
    ''' file can also be a complete URL. (For the supported URL schemes, see the 'URLs’ section of the help for url.)
    ''' </param>
    ''' <param name="encoding$"></param>
    ''' <returns></returns>
    <ExportAPI("read.csv")>
    Public Function readcsv(file$, Optional encoding$ = "utf8") As DataFrame
        Dim codepage As Encoding = encoding _
            .ParseEncodingsName(Encodings.UTF8) _
            .CodePage
        Return DataFrame.CreateObject(csv.Load(file, encoding:=codepage))
    End Function
End Module
