#Region "Microsoft.VisualBasic::87ef9cf628211d6d02a26f69b67ced70, R#\Runtime\Internal\debug.vb"

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

    '     Class debug
    ' 
    '         Properties: verbose
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [stop]
    ' 
    '         Sub: write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Public NotInheritable Class debug

        ''' <summary>
        ''' 啰嗦模式下会输出一些调试信息
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property verbose As Boolean = False

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 只在<see cref="verbose"/>啰嗦模式下才会工作
        ''' </summary>
        ''' <param name="message$"></param>
        ''' <param name="color"></param>
        Public Shared Sub write(message$, Optional color As ConsoleColor = ConsoleColor.White)
            If verbose Then
                Call VBDebugger.WaitOutput()
                Call Log4VB.Print(message & ASCII.LF, color)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
        End Function
    End Class
End Namespace
