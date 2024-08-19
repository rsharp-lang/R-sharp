﻿#Region "Microsoft.VisualBasic::cf08fcffcf37dc4c7ae8157e7e13600a, snowFall\Parallel.vb"

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

    '   Total Lines: 397
    '    Code Lines: 256 (64.48%)
    ' Comment Lines: 88 (22.17%)
    '    - Xml Docs: 73.86%
    ' 
    '   Blank Lines: 53 (13.35%)
    '     File Size: 15.81 KB


    ' Module Parallel
    ' 
    '     Function: detectCores, makeCluster, parallel, produceTask, runSlaveNode
    '               snowFall, SnowflakeIdGenerator_func, testParseSymbol, worker
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Darwinism.HPC.Parallel
Imports Darwinism.HPC.Parallel.ThreadTask
Imports Darwinism.IPC.Networking.Tcp
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Repository
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports snowFall.Context
Imports snowFall.Context.RPC
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' # Support for Parallel computation in ``R#`` 
''' 
''' this package module implements two kinds of the parallel client:
''' 
''' 1. ``Parallel::snowFall(port)`` works for the clr assembly function parallel
''' 2. ``Parallel::slave(port)`` works for the R# expression parallel
''' </summary>
<Package("Parallel")>
Public Module Parallel

    ''' <summary>
    ''' run parallel client node task
    ''' </summary>
    ''' <param name="port"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' A client of the clr assembly function slave parallel task
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("snowFall")>
    Public Function snowFall(port As Integer,
                             Optional master As String = "localhost",
                             Optional env As Environment = Nothing) As Object

        Return New TaskBuilder(port, master, verbose:=env.verboseOption).Run
    End Function

    ''' <summary>
    ''' ### detectCores: Detect the Number of CPU Cores
    ''' 
    ''' Attempt to detect the number of CPU cores on the current host.
    ''' </summary>
    ''' <param name="all_tests">
    ''' Logical: if true apply all known tests.
    ''' </param>
    ''' <param name="logical">
    ''' Logical: if possible, use the number of physical CPUs/cores (if FALSE) or logical CPUs (if TRUE). 
    ''' Currently this is honoured only on macOS, Solaris and Windows.
    ''' </param>
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
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("detectCores")>
    Public Function detectCores(Optional all_tests As Boolean = False, Optional logical As Boolean = True) As Integer
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("worker")>
    Public Function worker(host$, port%, outfile$) As workerList
        Return New workerList With {
            .host = host,
            .outfile = outfile,
            .port = port
        }
    End Function

    <ExportAPI("parseSymbolPayload")>
    Public Function testParseSymbol(<RRawVectorArgument> payload As Object, Optional env As Environment = Nothing) As Object
#Disable Warning
        Dim bytes As Byte() = REnv.asVector(Of Byte)(payload)
        Dim value As Symbol

        Using buffer As New MemoryStream(bytes)
            value = Serialization.GetValue(buffer)
        End Using

        Return value.value
#Enable Warning
    End Function

    <ExportAPI("snowflake_id_generator")>
    Public Function SnowflakeIdGenerator_func(machine_id As Long,
                                              Optional epoch As Long = SnowflakeIdGenerator.DefaultEpoch,
                                              Optional sequence_id As Long = 0L) As SnowflakeIdFunction

        Return New SnowflakeIdFunction(config:=New SnowflakeIdGenerator(machine_id, epoch, sequence_id))
    End Function

    ''' <summary>
    ''' run slave pipeline task on this new folked sub-process
    ''' </summary>
    ''' <param name="port"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' A client of the R# expression slave parallel task.
    ''' 
    ''' this function will break the rscript process when its job done!
    ''' </remarks>
    <ExportAPI("slave")>
    Public Function runSlaveNode(port As Integer, Optional env As Environment = Nothing) As Object
        Dim req As New RequestStream(MasterContext.Protocol, RPC.Protocols.Initialize)
        Dim localMaster As String = env.globalEnvironment.options.getOption("localMaster", LANTools.GetIPAddress.ToString, env)
        Dim resp As RequestStream
        Dim current_taskName As String = CLRVector.asCharacter(env.globalEnvironment.GetValue("@task")).FirstOrDefault

        If Not current_taskName.StringEmpty Then
            current_taskName = $"<{current_taskName}>: "
        End If

        Call VBDebugger.EchoLine($"{current_taskName}[slave_task] master_host={localMaster}")
        Call VBDebugger.EchoLine($"{current_taskName}[bootstrapping] bootstrap_port={port}!")

        resp = New TcpRequest(hostName:=localMaster, remotePort:=CInt(port)).SetVerbose(False).SendMessage(req)

        Dim uuid As Integer = BitConverter.ToInt32(resp.ChunkBuffer, Scan0)
        Dim masterPort As Integer = BitConverter.ToInt32(resp.ChunkBuffer, 4)
        Dim size As Integer = BitConverter.ToInt32(resp.ChunkBuffer, 8)

        Call VBDebugger.EchoLine($"{current_taskName}uuid={uuid}")
        Call VBDebugger.EchoLine($"{current_taskName}remote_environment={masterPort}")
        Call VBDebugger.EchoLine($"{current_taskName}task_body_size={size}")

        Dim buffer As Byte() = New Byte(size - 1) {}
        Dim closure As Expression = Nothing
        Dim result As ResultPayload = Nothing
        Dim root As New RemoteEnvironment(
            uuid:=uuid,
            master:=IPEndPoint.CreateLocal(masterPort, host:=localMaster),
            parent:=env
        )

        Call VBDebugger.EchoLine($"{current_taskName}create root environment:")
        Call VBDebugger.EchoLine(root.ToString)

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

        Call VBDebugger.EchoLine($"{current_taskName}get task:")
        Call VBDebugger.EchoLine("::")
        Call VBDebugger.EchoLine(closure.ToString)
        Call VBDebugger.EchoLine("::")
        Call VBDebugger.EchoLine($"{current_taskName} --> run!")
        Call VBDebugger.EchoLine("")

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

        Call VBDebugger.EchoLine("")
        Call VBDebugger.EchoLine($"{current_taskName}~job done!")

        If TypeOf result.value Is Message Then
            Call VBDebugger.EchoLine("")
            Call VBDebugger.EchoLine("exception:")
            Call VBDebugger.EchoLine(result.value.ToString)
            Call VBDebugger.EchoLine("")
        End If

        ' sync work on tcp request
        Call New TcpRequest(localMaster, masterPort).SetVerbose(False).SendMessage(req)
        Call New TcpRequest(localMaster, port).SetVerbose(False).SendMessage(New RequestStream(MasterContext.Protocol, RPC.Protocols.Stop))

        If TypeOf result.value Is Message Then
            Call VBDebugger.EchoLine($"{current_taskName}exit with task error.")
        Else
            Call VBDebugger.EchoLine($"{current_taskName}exec success.")
        End If

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
    ''' 5. ``log_tmp``: set the temp directory for log the getsymbol request data payloads
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

        If ___argvSet_____.hasName("log_tmp") Then
            Dim log_tmp As String = ___argvSet_____.getValue(Of String)("log_tmp", env, [default]:=Nothing)

            Call host.master.SetSymbolLogTempDir(log_tmp)
            Call host.task.FlushStream($"{log_tmp}/slave_task.dump")
        End If

        If verbose Then
            Call println("start host services!")
            Call println(host.master.ToString)
        End If

        Call RunTask(Sub() host.master.Run(AddressOf host.getSymbol), "snowFall_host_master")

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

            ' task i throw an exception
            If TypeOf i Is Message Then
                Call output.Add(Nothing)
                Call errors.Add((j, DirectCast(i, Message)))
            Else
                Call output.Add(i)
            End If
        Next

        If verbose Then
            Call println("close master node services!")
        End If

        ' Call App.Pause()
        Call host.master.Dispose()

        If errors.Any Then
            Call println("error was found during the parallel work process...")

            ' log errors to files at first!
            If ___argvSet_____.hasName("log_tmp") Then
                Dim log_tmp As String = ___argvSet_____.getValue(Of String)("log_tmp", env, [default]:=Nothing)

                For Each err As (i As Integer, ex As Message) In errors
                    ' index offset start from base 1 in Rscript
                    ' needs -1 for matched with the input argument
                    ' folder
                    Using file As StreamWriter = $"{log_tmp}/{err.i - 1}/slave_message.err".OpenWriter
                        Call Internal.debug.writeErrMessage(err.ex, stdout:=file, redirectError2stdout:=True)
                        Call file.Flush()
                    End Using
                Next
            End If

            If Not ignoreError Then
                ' returns the first error message
                ' and the caller environment will throw
                ' this error
                Return errors.First.ex
            Else
                ' treated the errors as the wraning message when
                ' do ignores of the errors
                For Each taskResult In errors
                    Call env.AddMessage($"[task_{taskResult.i}] {taskResult.ex.ToString}")
                Next
            End If
        End If

        ' finally returns the output result dataset
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
