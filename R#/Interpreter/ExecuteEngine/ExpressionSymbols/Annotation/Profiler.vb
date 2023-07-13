#Region "Microsoft.VisualBasic::5009f5a938c1a04a4171ab5753a3f4de, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Annotation/Profiler.vb"

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

    '   Total Lines: 70
    '    Code Lines: 46
    ' Comment Lines: 14
    '   Blank Lines: 10
    '     File Size: 2.42 KB


    '     Class Profiler
    ' 
    '         Properties: expressionName, stackFrame, target, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@profile``
    ''' 
    ''' 开始进行性能计数
    ''' </summary>
    ''' <remarks>It is not recommended that open the profiler 
    ''' session in the production mode, due to the reason of
    ''' profiler sampling will take too much time to generate 
    ''' sample data.
    ''' </remarks>
    Public Class Profiler : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        ''' <summary>
        ''' target expression for run profiler
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property target As Expression
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(evaluate As Expression, sourceMap As StackFrame)
            target = evaluate
            stackFrame = sourceMap
        End Sub

        Public Overrides Function ToString() As String
            Return $"@profile -> ( {target.ToString} )"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim openProfiler As New Environment(
                parent:=envir,
                stackFrame:=stackFrame,
                isInherits:=False,
                openProfiler:=True
            )
            Dim result As Object = target.Evaluate(openProfiler)
            Dim frames As New ProfilerFrames With {
                .profiles = openProfiler.profiler.PopAll,
                .timestamp = App.UnixTimeStamp,
                .traceback = stackFrame
            }

            Call envir.globalEnvironment.profiler2.Push(frames)

            Return result
        End Function
    End Class
End Namespace
