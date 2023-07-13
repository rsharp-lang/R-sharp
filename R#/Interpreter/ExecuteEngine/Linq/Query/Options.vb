#Region "Microsoft.VisualBasic::108f393953c86664d61d879be97c5aa5, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Query/Options.vb"

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

    '   Total Lines: 48
    '    Code Lines: 36
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 1.60 KB


    '     Class Options
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: RunOptionPipeline, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class Options

        Dim pipeline As PipelineKeyword()

        Friend message As Message

        Sub New(pipeline As IEnumerable(Of Expression))
            Me.pipeline = pipeline _
                .Select(Function(l) DirectCast(l, PipelineKeyword)) _
                .ToArray
        End Sub

        Public Function RunOptionPipeline(output As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)
            Dim raw As JavaScriptObject() = output.ToArray
            Dim allNames As String() = raw(Scan0).GetNames
            Dim env As Environment = context

            For Each name As String In allNames
                If env.FindSymbol(name, [inherits]:=False) Is Nothing Then
                    Call env.Push(name, Nothing, [readonly]:=False, mode:=TypeCodes.generic)
                End If
            Next

            For Each line As PipelineKeyword In pipeline
                raw = line.Exec(raw, context).SafeQuery.ToArray
                message = line.message

                If Not line.message Is Nothing Then
                    Return Nothing
                End If
            Next

            Return raw
        End Function

        Public Overrides Function ToString() As String
            Return pipeline.JoinBy(vbCrLf)
        End Function

    End Class
End Namespace
