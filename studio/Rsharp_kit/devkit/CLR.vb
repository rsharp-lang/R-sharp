#Region "Microsoft.VisualBasic::a6ce542a6f9913e718b300ba0732cbb5, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/devkit//CLR.vb"

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

    '   Total Lines: 85
    '    Code Lines: 51
    ' Comment Lines: 22
    '   Blank Lines: 12
    '     File Size: 2.97 KB


    ' Module CLRTool
    ' 
    '     Function: call_clr, LoadAssembly, open
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' .NET CLR tools
''' </summary>
<Package("clr")>
Public Module CLRTool

    ' clr::assembly("xxx.dll");
    ' lib = clr::open("xxx.xxx.xxx");
    ' result = call_clr_func(lib, "xxx", par1= xxx, par2 = xxx);

    ''' <summary>
    ''' load assembly from a given dll file
    ''' </summary>
    ''' <param name="pstr">the dll file path or the assembly name string</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("assembly")>
    Public Function LoadAssembly(pstr As String, Optional env As Environment = Nothing) As Object
        If pstr.FileExists Then
            Return Assembly.LoadFile(pstr.GetFullPath)
        ElseIf pstr.isFilePath(includeWindowsFs:=True) Then
            Return Internal.debug.stop({
                $".NET assembly is not exists on the given file location: '{pstr}'",
                $"Full_path: {pstr.GetFullPath}"
            }, env)
        Else
            Return Assembly.Load(pstr)
        End If
    End Function

    ''' <summary>
    ''' get .NET clr type via the type <paramref name="fullName"/>
    ''' </summary>
    ''' <param name="fullName"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("open")>
    Public Function open(fullName As String, Optional env As Environment = Nothing) As Object
        Return Microsoft.VisualBasic.Scripting.GetType(fullName)
    End Function

    <ExportAPI("call_clr")>
    Public Function call_clr([lib] As Type, name As String,
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

        Dim methods = [lib].GetMethods _
            .Where(Function(f) f.Name = name) _
            .ToArray

        If methods.Length = 0 Then
            Return Internal.debug.stop("method could not be found!", env)
        End If

        ' get given methods list may contains multiple
        ' signature function overloads
        Dim target As MethodInfo = methods(0)

        If methods.Length = 1 Then
            GoTo EXEC
        End If

        ' find the best function?

EXEC:
        ' finally, invoke the target clr function
        Dim rsharp_func As New RMethodInfo(target.Name, target, Nothing)
        Dim params As InvokeParameter() = args _
            .namedValues _
            .Select(Function(par, i) New InvokeParameter(par.Name, par.Value, i)) _
            .ToArray
        Dim result As Object = rsharp_func.Invoke(env, params)

        Return result
    End Function
End Module
