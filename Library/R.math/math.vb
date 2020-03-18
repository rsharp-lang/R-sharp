#Region "Microsoft.VisualBasic::3320c39dc6a558f22ebc712061e327e4, Library\R.math\math.vb"

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

' Module math
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: create_deSolve_DataFrame, Hist, (+2 Overloads) RK4
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports baseMath = Microsoft.VisualBasic.Math
Imports REnv = SMRUCC.Rsharp.Runtime
Imports stdVec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

''' <summary>
''' the R# math module
''' </summary>
<Package("math", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@live.com")>
Module math

    Sub New()
        REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(ODEsOut), AddressOf create_deSolve_DataFrame)
    End Sub

    Private Function create_deSolve_DataFrame(x As ODEsOut, args As list, env As Environment) As dataframe
        Dim data As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }
        Dim dx As String() = x.x _
            .Select(Function(d) CStr(d)) _
            .ToArray

        If args.hasName("x.lab") Then
            data.columns.Add(args("x.lab"), dx)
        End If

        For Each v In x.y
            data.columns.Add(v.Key, v.Value.ToArray)
        Next

        data.rownames = dx

        Return data
    End Function

    <ExportAPI("solve.RK4")>
    <RApiReturn(GetType(ODEOutput))>
    Public Function RK4(df As Object,
                        Optional y0# = 0,
                        Optional min# = -100,
                        Optional max# = 100,
                        Optional resolution% = 10000,
                        Optional env As Environment = Nothing) As Object
        Dim y As df

        If df Is Nothing Then
            Return REnv.Internal.debug.stop("Missing ``dy/dt``!", env)
        End If

        If TypeOf df Is df Then
            y = DirectCast(df, df)
        ElseIf TypeOf df Is DeclareLambdaFunction Then
            With DirectCast(df, DeclareLambdaFunction).CreateLambda(Of Double, Double)(env)
                y = Function(x, x1) .Invoke(x)
            End With
        Else
            Return REnv.Internal.debug.stop(New NotSupportedException, env)
        End If

        Return New ODE With {
            .df = y,
            .y0 = y0,
            .ID = df.ToString
        }.RK4(resolution, min, max)
    End Function

    <ExportAPI("deSolve")>
    <RApiReturn(GetType(ODEsOut))>
    Public Function RK4(system As DeclareLambdaFunction(), y0 As list, a#, b#,
                        Optional resolution% = 10000,
                        Optional tick As Action(Of Environment) = Nothing,
                        Optional env As Environment = Nothing) As Object

        Dim vector As var() = New var(system.Length - 1) {}
        Dim i As i32 = Scan0
        Dim solve As Func(Of Double)
        Dim names As New Dictionary(Of String, Symbol)

        For Each v As NamedValue(Of Object) In y0.namedValues
            Call env.Push(v.Name, REnv.asVector(Of Double)(v.Value), [readonly]:=False, mode:=TypeCodes.double)
            Call names.Add(v.Name, env.FindSymbol(v.Name, [inherits]:=False))
        Next

        For Each formula As DeclareLambdaFunction In system
            Dim lambda As Func(Of Double, Double) = formula.CreateLambda(Of Double, Double)(env)
            Dim name As String = formula.parameterNames(Scan0)
            Dim ref As Symbol = names(name)

            ref.SetValue(REnv.getFirst(y0.getByName(name)), env)
            solve = Function() lambda(CDbl(ref.value))
            vector(++i) = New var(solve) With {
                .Name = name,
                .Value = REnv.getFirst(y0.getByName(.Name))
            }
        Next

        Dim df = Sub(dx#, ByRef dy As stdVec)
                     For Each x As var In vector
                         dy(x) = x.Evaluate()
                         names(x.Name).SetValue(x.Value, env)
                     Next
                 End Sub
        Dim ODEs As New GenericODEs(vector, df)
        Dim result As Object

        If tick Is Nothing Then
            result = ODEs.Solve(n:=resolution, a:=a, b:=b)
        Else
            result = New SolverIterator(New RungeKutta4(ODEs)) _
                .Config(ODEs.GetY0(False), resolution, a, b) _
                .Bind(Sub()
                          Call tick(env)
                      End Sub)
        End If

        Return result
    End Function

    ''' <summary>
    ''' Do fixed width cut bins
    ''' </summary>
    ''' <param name="data">A numeric vector data sequence</param>
    ''' <param name="step">The width of the bin box.</param>
    ''' <returns></returns>
    <ExportAPI("hist")>
    Public Function Hist(<RRawVectorArgument> data As Object, Optional step! = 1) As DataBinBox(Of Double)()
        Return DirectCast(REnv.asVector(Of Double)(data), Double()) _
            .Hist([step]) _
            .ToArray
    End Function

    ''' <summary>
    ''' do linear modelling
    ''' </summary>
    ''' <param name="formula"></param>
    ''' <param name="data">A dataframe for provides the data source for doing the linear modelling.</param>
    ''' <param name="weights">A numeric vector for provides weight value for the points in the linear modelling processing.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("lm")>
    Public Function lm(formula As Object, data As Object,
                       <RRawVectorArgument>
                       Optional weights As Object = Nothing,
                       Optional env As Environment = Nothing) As Object

        If Not base.isEmpty(weights) Then

        End If

        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' Evaluate cos similarity of two vector
    ''' 
    ''' the given vector x and y should be contains the elements in the same length.
    ''' </summary>
    ''' <param name="x">a numeric data sequence</param>
    ''' <param name="y">another numeric data sequence</param>
    ''' <returns></returns>
    <ExportAPI("ssm")>
    Public Function ssm(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Double
        Dim vx As Double() = DirectCast(REnv.asVector(Of Double)(x), Double())
        Dim vy As Double() = DirectCast(REnv.asVector(Of Double)(y), Double())

        Return baseMath.SSM(vx.AsVector, vy.AsVector)
    End Function
End Module

