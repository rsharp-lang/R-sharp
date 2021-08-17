#Region "Microsoft.VisualBasic::330088f022e23255507af5dc737a74dc, snowFall\Parallel.vb"

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

' Module Parallel
' 
'     Function: detectCores, makeCluster, snowFall, worker
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Parallel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

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
    Public Function snowFall(port As Integer) As Object
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
    ''' run parallel of R# expression
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="argv"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parallel")>
    Public Function parallel(task As Expression, <RListObjectArgument> Optional argv As list = Nothing, Optional env As Environment = Nothing) As Object

    End Function
End Module
