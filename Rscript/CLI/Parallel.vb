#Region "Microsoft.VisualBasic::37c1cd1cc9e95667de0a17c4e71f5364, E:/GCModeller/src/R-sharp/Rscript//CLI/Parallel.vb"

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

    '   Total Lines: 89
    '    Code Lines: 64
    ' Comment Lines: 10
    '   Blank Lines: 15
    '     File Size: 3.78 KB


    ' Module CLI
    ' 
    '     Function: parallelMode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Partial Module CLI

    ''' <summary>
    ''' run parallel combine with the snowFall package
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--parallel")>
    <Usage("--parallel --master <master_port> [--host <localhost> --delegate <delegate_name> --task <task_name> --redirect_stdout <logfile.txt>]")>
    <Description("Create a new parallel thread process for running a new parallel task.")>
    <Argument("--master", False, CLITypes.Integer, AcceptTypes:={GetType(Integer)}, Description:="the TCP port of the master node.")>
    <Argument("--task", True, CLITypes.String, Description:="set the task name for current slave process, this option may affects the label tag of the global environment for run debug test.")>
    <Argument("--delegate", True, CLITypes.String, Description:="the delegate function name in clr environment for solve the parallel task.")>
    Public Function parallelMode(args As CommandLine) As Integer
        Dim masterPort As Integer = args <= "--master"
        Dim logfile As String = args <= "--redirect_stdout"
        Dim hostName As String = args("--host")
        Dim taskName As String = args("--task")

        If Not logfile.StringEmpty Then
            Dim stdout As StreamWriter = App.RedirectLogging(logfile)

            Call App.AddExitCleanHook(
                Sub()
                    Call stdout.Flush()
                    Call stdout.Close()
                End Sub)
        End If

        Dim delegateName As String = args("--delegate")
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        Dim REngine As New RInterpreter(env_label:=taskName)
        Dim plugin As String = LibDLL.GetDllFile($"snowFall.dll", REngine.globalEnvir)

        If Not SetDllDirectory.StringEmpty Then
            Call REngine.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If
        If Not hostName.StringEmpty Then
            Call REngine.globalEnvir.options.setOption("localMaster", hostName)
        End If

        If Not plugin.FileExists Then
            Return 500
        End If

        Dim reference As NamedValue(Of String)

        If delegateName.StringEmpty Then
            reference = "Parallel::snowFall".GetTagValue("::")
        Else
            reference = delegateName.GetTagValue("::")
        End If

        Dim parallelFunc As String = reference.Description
        Dim argv = New Object() {masterPort, REngine.globalEnvir}

        ' load primary base libraries
        Call LoadLibrary(REngine, ignoreMissingStartupPackages:=False, "base", "utils", "grDevices", "math", "stats")
        Call Console.WriteLine()

        Call REngine.Add("@task", taskName, TypeCodes.string)
        Call PackageLoader.ParsePackages(plugin) _
            .Where(Function(pkg) pkg.namespace = reference.Name) _
            .FirstOrDefault _
            .DoCall(Sub(pkg)
                        Call REngine.globalEnvir.ImportsStatic(pkg.package)
                    End Sub)

        ' start to run the parallel task
        Call REngine.Invoke(parallelFunc, argv)

        ' 20221103 unsure for the bug that some working thread
        ' is not exit from current parallel slave process?
        ' try to end current process directly!
        End

        Return 0
    End Function
End Module
