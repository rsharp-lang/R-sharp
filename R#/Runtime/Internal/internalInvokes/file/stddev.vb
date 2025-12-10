#Region "Microsoft.VisualBasic::5530e58965c75f590a088610222f4f98, R#\Runtime\Internal\internalInvokes\file\stddev.vb"

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

    '   Total Lines: 142
    '    Code Lines: 22 (15.49%)
    ' Comment Lines: 114 (80.28%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 6 (4.23%)
    '     File Size: 9.61 KB


    '     Module stddev
    ' 
    '         Function: nullfile, stderr, stdin, stdout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Runtime.Internal.Invokes

    Module stddev

        ''' <summary>
        ''' ### Display Connections
        ''' 
        ''' Display aspects of connections.
        ''' </summary>
        ''' <returns>
        ''' stdin(), stdout() and stderr() return connection objects.
        ''' showConnections returns a character matrix of information with a row for each connection,
        ''' by default only for open non-standard connections.
        ''' getConnection returns a connection object, or NULL.
        ''' </returns>
        ''' <remarks>
        ''' stdin(), stdout() and stderr() are standard connections corresponding to input, output and 
        ''' error on the console respectively (and not necessarily to file streams). They are text-mode 
        ''' connections of class "terminal" which cannot be opened or closed, and are read-only, write-only 
        ''' and write-only respectively. The stdout() and stderr() connections can be re-directed by sink 
        ''' (and in some circumstances the output from stdout() can be split: see the help page).
        ''' The encoding for stdin() when redirected can be set by the command-line flag --encoding.
        ''' nullfile() returns filename of the null device ("/dev/null" on Unix, "nul:" on Windows).
        ''' showConnections returns a matrix of information. If a connection object has been lost or forgotten,
        ''' getConnection will take a row number from the table and return a connection object for that connection,
        ''' which can be used to close the connection, for example. However, if there is no R level object 
        ''' referring to the connection it will be closed automatically at the next garbage collection 
        ''' (except for gzcon connections).
        ''' closeAllConnections closes (and destroys) all user connections, restoring all sink diversions as it does so.
        ''' isatty returns true if the connection is one of the class "terminal" connections and it is apparently 
        ''' connected to a terminal, otherwise false. This may not be reliable in embedded applications, 
        ''' including GUI consoles.
        ''' getAllConnections returns a sequence of integer connection descriptors for use with getConnection,
        ''' corresponding to the row names of the table returned by showConnections(all = TRUE).
        ''' 
        ''' stdin() refers to the ‘console’ and not to the C-level ‘stdin’ of the process. The distinction matters 
        ''' in GUI consoles (which may not have an active ‘stdin’, and if they do it may not be connected to 
        ''' console input), and also in embedded applications. If you want access to the C-level file stream 
        ''' ‘stdin’, use file("stdin").
        ''' When R is reading a script from a file, the file is the ‘console’: this is traditional usage to allow 
        ''' in-line data (see ‘An Introduction to R’ for an example).
        ''' </remarks>
        <ExportAPI("stderr")>
        Public Function stderr() As Stream
            Return Console.OpenStandardError
        End Function

        ''' <summary>
        ''' ### Display Connections
        ''' 
        ''' Display aspects of connections.
        ''' </summary>
        ''' <returns>
        ''' stdin(), stdout() and stderr() return connection objects.
        ''' showConnections returns a character matrix of information with a row for each connection,
        ''' by default only for open non-standard connections.
        ''' getConnection returns a connection object, or NULL.
        ''' </returns>
        ''' <remarks>
        ''' stdin(), stdout() and stderr() are standard connections corresponding to input, output and 
        ''' error on the console respectively (and not necessarily to file streams). They are text-mode 
        ''' connections of class "terminal" which cannot be opened or closed, and are read-only, write-only 
        ''' and write-only respectively. The stdout() and stderr() connections can be re-directed by sink 
        ''' (and in some circumstances the output from stdout() can be split: see the help page).
        ''' The encoding for stdin() when redirected can be set by the command-line flag --encoding.
        ''' nullfile() returns filename of the null device ("/dev/null" on Unix, "nul:" on Windows).
        ''' showConnections returns a matrix of information. If a connection object has been lost or forgotten,
        ''' getConnection will take a row number from the table and return a connection object for that connection,
        ''' which can be used to close the connection, for example. However, if there is no R level object 
        ''' referring to the connection it will be closed automatically at the next garbage collection 
        ''' (except for gzcon connections).
        ''' closeAllConnections closes (and destroys) all user connections, restoring all sink diversions as it does so.
        ''' isatty returns true if the connection is one of the class "terminal" connections and it is apparently 
        ''' connected to a terminal, otherwise false. This may not be reliable in embedded applications, 
        ''' including GUI consoles.
        ''' getAllConnections returns a sequence of integer connection descriptors for use with getConnection,
        ''' corresponding to the row names of the table returned by showConnections(all = TRUE).
        ''' 
        ''' stdin() refers to the ‘console’ and not to the C-level ‘stdin’ of the process. The distinction matters 
        ''' in GUI consoles (which may not have an active ‘stdin’, and if they do it may not be connected to 
        ''' console input), and also in embedded applications. If you want access to the C-level file stream 
        ''' ‘stdin’, use file("stdin").
        ''' When R is reading a script from a file, the file is the ‘console’: this is traditional usage to allow 
        ''' in-line data (see ‘An Introduction to R’ for an example).
        ''' </remarks>
        <ExportAPI("stdout")>
        Public Function stdout() As Stream
            Return Console.OpenStandardOutput
        End Function

        ''' <summary>
        ''' ### Display Connections
        ''' 
        ''' Display aspects of connections.
        ''' </summary>
        ''' <returns>
        ''' stdin(), stdout() and stderr() return connection objects.
        ''' showConnections returns a character matrix of information with a row for each connection,
        ''' by default only for open non-standard connections.
        ''' getConnection returns a connection object, or NULL.
        ''' </returns>
        ''' <remarks>
        ''' stdin(), stdout() and stderr() are standard connections corresponding to input, output and 
        ''' error on the console respectively (and not necessarily to file streams). They are text-mode 
        ''' connections of class "terminal" which cannot be opened or closed, and are read-only, write-only 
        ''' and write-only respectively. The stdout() and stderr() connections can be re-directed by sink 
        ''' (and in some circumstances the output from stdout() can be split: see the help page).
        ''' The encoding for stdin() when redirected can be set by the command-line flag --encoding.
        ''' nullfile() returns filename of the null device ("/dev/null" on Unix, "nul:" on Windows).
        ''' showConnections returns a matrix of information. If a connection object has been lost or forgotten,
        ''' getConnection will take a row number from the table and return a connection object for that connection,
        ''' which can be used to close the connection, for example. However, if there is no R level object 
        ''' referring to the connection it will be closed automatically at the next garbage collection 
        ''' (except for gzcon connections).
        ''' closeAllConnections closes (and destroys) all user connections, restoring all sink diversions as it does so.
        ''' isatty returns true if the connection is one of the class "terminal" connections and it is apparently 
        ''' connected to a terminal, otherwise false. This may not be reliable in embedded applications, 
        ''' including GUI consoles.
        ''' getAllConnections returns a sequence of integer connection descriptors for use with getConnection,
        ''' corresponding to the row names of the table returned by showConnections(all = TRUE).
        ''' 
        ''' stdin() refers to the ‘console’ and not to the C-level ‘stdin’ of the process. The distinction matters 
        ''' in GUI consoles (which may not have an active ‘stdin’, and if they do it may not be connected to 
        ''' console input), and also in embedded applications. If you want access to the C-level file stream 
        ''' ‘stdin’, use file("stdin").
        ''' When R is reading a script from a file, the file is the ‘console’: this is traditional usage to allow 
        ''' in-line data (see ‘An Introduction to R’ for an example).
        ''' </remarks>
        <ExportAPI("stdin")>
        Public Function stdin() As Stream
            Return Console.OpenStandardInput
        End Function

        <ExportAPI("nullfile")>
        Public Function nullfile() As Stream
            Return NullStream.Instance
        End Function
    End Module
End Namespace
