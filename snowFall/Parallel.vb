#Region "Microsoft.VisualBasic::7289286f15ce4b40e2bc88d54916d94e, R-sharp\snowFall\Parallel.vb"

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

    '   Total Lines: 319
    '    Code Lines: 213
    ' Comment Lines: 65
    '   Blank Lines: 41
    '     File Size: 11.60 KB


    ' Module Parallel
    ' 
    '     Function: detectCores, makeCluster, parallel, produceTask, runSlaveNode
    '               snowFall, worker
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Parallel
Imports Parallel.ThreadTask
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports snowFall.Context
Imports snowFall.Context.RPC
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' # Support for Parallel computation in ``R#`` 
''' </summary>
<Package("Parallel")>
Public Module Parallel

    ''' <summary>
    ''' run parallel client node task
    ''' </summary>
    ''' <param name="port"></param>
    ''' <returns></returns>
    <ExportAPI("snowFall")>
    Public Function snowFall(port As Integer, Optional env As Environment = Nothing) As Object
        Return New TaskBuilder(port).Run
    End Function

    ''' <summary>
    ''' ### detectCores: Detect the Number of CPU Cores
    ''' 
    ''' Attempt to detect the number of CPU cores on the current host.
    ''' </summary>
    ''' <returns>An integer, NA if the answer is unknown.
    ''' Exactly what this represents Is OS-dependent: where possible by 
    ''' Default it counts logical (e.g., hyperthreaded) CPUs And Not 
    ''' physical cores Or packages.</returns>
    ''' <remarks>
    ''' This attempts to detect the number of available CPU cores.
    ''' It has methods To Do so For Linux, macOS, FreeBSD, OpenBSD, Solaris, 
    ''' Irix And Windows. detectCores(True) could be tried On other Unix-alike 
    ''' systems.
    ''' </remarks>
    <ExportAPI("detectCores")>
    Public Function detectCores() As Integer
        Return App.CPUCoreNumbers
    End Function

    ''' <summary>
    ''' ### makeCluster: Create a Parallel Socket Cluster
    ''' 
    ''' Creates a set of copies of R running in parallel and communicating over sockets.
    ''' </summary>
    ''' <param name="spec">
    ''' A specification appropriate To the type Of cluster.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("makeCluster")>
    Public Function makeCluster(<RRawVectorArgument> spec As Object, Optional env As Environment = Nothing)
        Throw New NotImplementedException
    End Function

    <ExportAPI("worker")>
    Public Function worker(host$, port%, outfile$) As workerList
        Return New workerList With {
            .host = host,
            .outfile = outfile,
            .port = port
        }
    End Function

    ''' <summary>
    ''' run slave pipeline task on this new folked sub-process
    ''' </summary>
    ''' <param name="port"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function will break the rscript process when its job done!
    ''' </remarks>
    <ExportAPI("slave")>
    Public Function runSlaveNode(port As Integer, Optional env As Environment = Nothing) As Object
        Dim req As New RequestStream(MasterContext.Protocol, RPC.Protocols.Initialize)
        Dim localMaster As String = env.globalEnvironment.options.getOption("localMaster", LANTools.GetIPAddress.ToString, env)
        Dim resp As RequestStream

        Call Console.WriteLine($"[slave_task] master_host={localMaster}")
        Call Console.WriteLine($"[bootstrapping] bootstrap_port={port}!")

        resp = New TcpRequest(hostName:=localMaster, remotePort:=port).SendMessage(req)

        Dim uuid As Integer = BitConverter.ToInt32(resp.ChunkBuffer, Scan0)
        Dim masterPort As Integer = BitConverter.ToInt32(resp.ChunkBuffer, 4)
        Dim size As Integer = BitConverter.ToInt32(resp.ChunkBuffer, 8)

        Call Console.WriteLine($"uuid={uuid}")
        Call Console.WriteLine($"remote_environment={masterPort}")
        Call Console.WriteLine($"task_body_size={size}")

        Dim buffer As Byte() = New Byte(size - 1) {}
        Dim closure As Expression = Nothing
        Dim result As ResultPayload = Nothing
        Dim root As New RemoteEnvironment(
            uuid:=uuid,
            master:=IPEndPoint.CreateLocal(masterPort, host:=localMaster),
            parent:=env
        )

        Call Console.WriteLine("create root environment:")
        Call Console.WriteLine(root.ToString)

        Dim fake As New DESCRIPTION With {
            .Author = "xieguigang",
            .[Date] = Now.ToString,
            .Maintainer = .Author,
            .License = "MIT",
            .Package = NameOf(runSlaveNode),
            .Title = .Package,
            .Type = "runtime",
            .Version = App.Version,
            .Description = .Package
        }

        Call Array.ConstrainedCopy(resp.ChunkBuffer, 12, buffer, Scan0, size)

        Using file As New MemoryStream(buffer), reader As New BinaryReader(file)
            Call BlockReader.Read(reader).Parse(fake, expr:=closure)
        End Using

        Call Console.WriteLine("get task:")
        Call Console.WriteLine("::")
        Call Console.WriteLine(closure.ToString)
        Call Console.WriteLine("::")
        Call Console.WriteLine(" --> run!")
        Call Console.WriteLine()

        Try
            ' run task closure at here
            result = New ResultPayload(env) With {
                .uuid = uuid,
                .value = closure.Evaluate(root)
            }
        Catch ex As Exception
            ' 20221024 capture the unexpected error
            result = New ResultPayload(env) With {
                .uuid = uuid,
                .value = Internal.debug.stop(ex, env, suppress:=True)
            }
        End Try

        req = New RequestStream(MasterContext.Protocol, RPC.Protocols.PushResult, result)

        ' Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine("~job done!")

        If TypeOf result.value Is Message Then
            Call Console.WriteLine()
            Call Console.WriteLine("exception:")
            Call Console.WriteLine(result.value.ToString)
            Call Console.WriteLine()
        End If

        ' sync work on tcp request
        Call New TcpRequest(localMaster, masterPort).SendMessage(req)
        Call New TcpRequest(localMaster, port).SendMessage(New RequestStream(MasterContext.Protocol, RPC.Protocols.Stop))
        Call Console.WriteLine("exit!")

        Return App.Exit(0)
    End Function

    ''' <summary>
    ''' run parallel of R# expression
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="___argvSet_____">
    ''' there are some additional parameter in this object list that can be config:
    ''' 
    ''' 1. ``debug``: set true for open debug mode
    ''' 2. ``master``: set the tcp port of the master node
    ''' 3. ``bootstrap``: set the bootstrap tcp port of the slave node
    ''' 4. ``slaveDebug``: set this option to pause will make the master node pause when run a new salve node for run debug
    ''' 
    ''' due to the reason of some short parameter name may 
    ''' conflict with the symbol name in script code, so 
    ''' make a such long name in weird and strange string 
    ''' pattern to avoid such bug.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parallel")>
    Public Function parallel(task As Expression,
                             Optional n_threads As Integer = -1,
                             Optional debug As Boolean? = Nothing,
                             Optional ignoreError As Boolean? = Nothing,
                             Optional verbose As Boolean? = Nothing,
                             <RListObjectArgument>
                             Optional ___argvSet_____ As list = Nothing,
                             Optional env As Environment = Nothing) As Object

        If debug Is Nothing Then
            If ___argvSet_____.hasName("debug") Then
                debug = ___argvSet_____.getValue(Of Boolean)("debug", env, [default]:=False)
            Else
                debug = False
            End If
        End If
        If ignoreError Is Nothing Then
            If ___argvSet_____.hasName("ignoreError") Then
                ignoreError = ___argvSet_____.getValue(Of Boolean)("ignoreError", env, [default]:=False)
            Else
                ignoreError = False
            End If
        End If
        If verbose Is Nothing Then
            If ___argvSet_____.hasName("verbose") Then
                verbose = ___argvSet_____.getValue(Of Boolean)("verbose", env, [default]:=False)
            Else
                verbose = False
            End If
        End If

        Dim host As RunParallel = RunParallel.Initialize(task, ___argvSet_____, debug, verbose, env)

        If host.error IsNot Nothing Then
            Return host.error
        End If

        Dim println = env.WriteLineHandler
        Dim taskList As IEnumerable(Of Func(Of SeqValue(Of Object))) = host.produceTask
        Dim engine As New ThreadTask(Of SeqValue(Of Object))(
            task:=taskList,
            debugMode:=debug,
            verbose:=env.globalEnvironment.debugMode
        )

        If verbose Then
            Call println("start host services!")
            Call println(host.master.ToString)
        End If

        Call New Thread(Sub() host.master.Run(AddressOf host.getSymbol)).Start()

        If verbose Then
            Call println("run parallel!")
        End If

        Dim result As Object() = engine _
            .WithDegreeOfParallelism(n_threads) _
            .RunParallel _
            .OrderBy(Function(a) a.i) _
            .Select(Function(a) REnv.single(a.value)) _
            .ToArray
        Dim output As New List(Of Object)
        Dim errors As New List(Of (i As Integer, ex As Message))
        Dim j As Integer = 0

        If verbose Then
            Call println("all parallel job done!")
        End If

        For Each i As Object In result
            j += 1

            If Program.isException(i) Then
                If Not ignoreError Then
                    Call host.master.Dispose()
                    Return i
                Else
                    Call output.Add(Nothing)
                    Call errors.Add((j, DirectCast(i, Message)))
                End If
            Else
                Call output.Add(i)
            End If
        Next

        If verbose Then
            Call println("close master node services!")
        End If

        Call host.master.Dispose()

        If errors.Any Then
            Call println("error was found during the parallel work process...")

            For Each taskResult In errors
                Call env.AddMessage($"[task_{taskResult.i}] {taskResult.ex.ToString}")
            Next
        End If

        Return REnv.TryCastGenericArray(output.ToArray, env)
    End Function

    <Extension>
    Private Iterator Function produceTask(run As RunParallel) As IEnumerable(Of Func(Of SeqValue(Of Object)))
        For i As Integer = 0 To run.size - 1
            Dim index As Integer = i
            Dim x As Func(Of SeqValue(Of Object)) =
                Function()
                    Return New SeqValue(Of Object)(index, run.taskFactory(index))
                End Function

            Yield x
        Next
    End Function
End Module
