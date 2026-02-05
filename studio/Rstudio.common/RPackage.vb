Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module RPackage

    ''' <summary>
    ''' Common method for compile a R# source directory to binary package
    ''' </summary>
    ''' <param name="meta"></param>
    ''' <param name="src"></param>
    ''' <param name="save"></param>
    ''' <param name="skipSourceBuild"></param>
    ''' <param name="r_syntax"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Compile(meta As DESCRIPTION, src As String, save As String,
                            Optional skipSourceBuild As Boolean = True,
                            Optional r_syntax As String = "../../_assets/R_syntax.js",
                            Optional doc_template As String = Nothing,
                            Optional clr_template As String = Nothing,
                            Optional enableDebugSymbols As Boolean = False) As Integer

        If meta.isEmpty Then
            Call Console.WriteLine($"Missing 'DESCRIPTION' meta data file at: {src.GetDirectoryFullPath}, check of your commandline input please!")
            Return 500
        Else
            ' config for publish document files on github page
            Call App.JoinVariable("r_syntax.js", r_syntax)
            Call App.JoinVariable("doc_template", doc_template)
            Call App.JoinVariable("clr_template", clr_template)
        End If

        ' build .net5 assembly via dotnet msbuild command?
#If NETCOREAPP Then
        If Not skipSourceBuild Then
            Call runMSBuild(src)
        Else
            Call Console.WriteLine($"Skip MSBuild for .NET core runtime...")
        End If
#End If

        Using outputfile As FileStream = save.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Return meta.Compile(src, outputfile, enableDebugSymbols:=enableDebugSymbols)
        End Using
    End Function

    ''' <summary>
    ''' compile the source dir as a package file
    ''' </summary>
    ''' <param name="meta"></param>
    ''' <param name="src"></param>
    ''' <param name="outputFile"></param>
    ''' <param name="auto_fileclose"></param>
    ''' <param name="enableDebugSymbols"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Compile(meta As DESCRIPTION, src As String, outputFile As Stream,
                            Optional auto_fileclose As Boolean = True,
                            Optional enableDebugSymbols As Boolean = False) As Integer

        ' framework dll module ignores
        Dim assemblyFilters As Index(Of String) = {
            "Rscript.exe", "R#.exe", "Rscript.dll", "R#.dll", "REnv.dll", "RData.dll",
            "Microsoft.VisualBasic.Runtime.dll"
        }
        Dim err As Message = meta.Build(src, outputFile, assemblyFilters,
                                        file_close:=auto_fileclose,
                                        enableDebugSymbols:=enableDebugSymbols)
        Dim save_file As String

        If TypeOf outputFile Is FileStream Then
            save_file = DirectCast(outputFile, FileStream).Name
        Else
            save_file = $"in_memory://&H_0x<{outputFile.GetHashCode.ToHexString}>"
        End If

        If RProgram.isException(err) Then
            Return CInt(debug.PrintMessageInternal(err, Nothing))
        Else
            Call Console.WriteLine()
            Call Console.WriteLine($"  Source package written to {save_file}")
            Call Console.WriteLine()
        End If

        Return 0
    End Function

#If NETCOREAPP Then
    Private Function runMSBuild(src As String) As Boolean
        If MSBuild.version Is Nothing Then
            Return False
        End If

        For Each sln As String In ls - l - "*.sln" <= src
            Call MSBuild.BuildVsSolution(sln, rebuild:=True)
        Next

        Return True
    End Function
#End If

    Public Function sourceHelper(src As String) As String
        src = Strings.Trim(src)

        If src.StringEmpty Then
            Return Nothing
        End If

        If src.DirectoryExists Then
            Return src
        ElseIf src.ExtensionSuffix("Rproj") Then
            Return src.ParentPath
        Else
            Throw New InvalidProgramException($"invalid project target ""{src}""!")
        End If
    End Function
End Module
