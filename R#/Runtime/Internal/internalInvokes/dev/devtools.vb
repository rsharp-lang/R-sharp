#Region "Microsoft.VisualBasic::a3a2b61054dc7066a946e1e47cf7e140, R#\Runtime\Internal\internalInvokes\dev\dev.vb"

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

    '     Module dev
    ' 
    '         Function: flash_load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module devtools

        ''' <summary>
        ''' Load R script in directory
        ''' 
        ''' Load all of the R script in a given working directory,
        ''' by default is load all script in current directory.
        ''' </summary>
        ''' <param name="dir">The script source directory, by default is current workspace.</param>
        <ExportAPI("flash_load")>
        Public Function flash_load(<RDefaultExpression> Optional dir As String = "~getwd()", Optional env As GlobalEnvironment = Nothing) As Object
            Dim Rlist As String() = dir _
                .EnumerateFiles("*.r", "*.R") _
                .Select(Function(path) path.GetFullPath) _
                .Distinct _
                .ToArray

            For Each script As String In Rlist
                Try
                    Dim err As Object = env.Rscript.Source(script)

                    If Program.isException(err) Then
                        Call debug.PrintMessageInternal(DirectCast(err, Message), env)
                    End If
                Catch ex As Exception
                    Call base.print($"Error while loading script: {script}", , env)
                    Call App.LogException(ex)
                    Call base.print(ex, , env)
                End Try
            Next

            Dim zzz As String = $"{dir}/zzz.R"

            If zzz.FileExists Then
                Call env.doCall(".onLoad")
            End If

            Return New invisible()
        End Function
    End Module
End Namespace
