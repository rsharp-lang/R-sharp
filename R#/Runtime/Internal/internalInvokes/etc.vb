#Region "Microsoft.VisualBasic::d1fc2044b06206e91c590abd9f7ca56f, R#\Runtime\Internal\internalInvokes\etc.vb"

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

'     Module etc
' 
'         Function: contributors, license
' 
'         Sub: demo
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module etc

        ''' <summary>
        ''' # The R# License Terms
        ''' 
        ''' The license terms under which R# is distributed.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("license")>
        Public Function license() As <RSuppressPrint> Object
            Call Console.WriteLine(Rsharp.LICENSE.GPL3)
            Return Nothing
        End Function

        ''' <summary>
        ''' # ``R#`` Project Contributors
        ''' 
        ''' The R# Who-is-who, describing who made significant contributions to the development of R#.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("contributors")>
        Public Function contributors() As <RSuppressPrint> Object
            Call Console.WriteLine(My.Resources.contributions)
            Return Nothing
        End Function

        <ExportAPI("demo")>
        Public Sub demo()

        End Sub

        ''' <summary>
        ''' ### Extract System and User Information
        ''' 
        ''' Reports system and user information.
        ''' </summary>
        ''' <returns>
        ''' A character vector with fields
        '''
        ''' + ``sysname`` The operating system name.
        ''' + ``release`` The OS release.
        ''' + ``version`` The OS version.
        ''' + ``nodename`` A name by which the machine Is known On 
        '''     the network (If any).
        ''' + ``machine`` A concise description Of the hardware, 
        '''     often the CPU type.
        ''' + ``login`` The user 's login name, or "unknown" if it 
        '''     cannot be ascertained.
        ''' + ``user`` The name Of the real user ID, Or "unknown" If 
        '''     it cannot be ascertained.
        ''' + ``effective_user`` The name Of the effective user ID, Or 
        '''     "unknown" If it cannot be ascertained. This may differ 
        '''     from the real user In 'set-user-ID’ processes.
        '''
        ''' The last three fields give the same value On Windows.
        ''' </returns>
        <ExportAPI("Sys.info")>
        Public Function Sys_info() As list
            Return New list With {
                .slots = New Dictionary(Of String, Object)
            }
        End Function
    End Module
End Namespace
