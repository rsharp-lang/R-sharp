#Region "Microsoft.VisualBasic::10891bae292822cd619fb720db780686, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RGenericOverloadsAttribute.vb"

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

    '   Total Lines: 94
    '    Code Lines: 54
    ' Comment Lines: 27
    '   Blank Lines: 13
    '     File Size: 3.53 KB


    '     Class RGenericOverloadsAttribute
    ' 
    '         Properties: [Overloads], FunctionName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class RGenericOverloads
    ' 
    '         Properties: [overloads], func, name
    ' 
    '         Function: GetOverloads
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Runtime.Interop

    ''' <summary>
    ''' construct a flag that indicates that target clr function will 
    ''' be overloads in R# environment, the function overloads is 
    ''' determined based on the data type of the first parameter. 
    ''' 
    ''' the function overloads in R# usually be ``plot(...)``, ``as.data.frame(...)``,
    ''' typically.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class RGenericOverloadsAttribute : Inherits RInteropAttribute

        Public ReadOnly Property FunctionName As String
        Public ReadOnly Property [Overloads] As Type()

        ''' <summary>
        ''' construct a flag that indicates that target clr function will 
        ''' be overloads in R# environment, the function overloads is 
        ''' determined based on the data type of the first parameter. 
        ''' 
        ''' the function overloads in R# usually be ``plot(...)``, ``as.data.frame(...)``,
        ''' typically.
        ''' </summary>
        ''' <param name="func">
        ''' The name of the target function for overloads
        ''' </param>
        Sub New(func As String, ParamArray [overloads] As Type())
            _FunctionName = func
            _Overloads = [overloads]
        End Sub

        Public Overrides Function ToString() As String
            Return $"{FunctionName}(...)"
        End Function
    End Class

    Public Class RGenericOverloads

        ''' <summary>
        ''' the name of the target function
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        ''' <summary>
        ''' the clr overloads type
        ''' </summary>
        ''' <returns></returns>
        Public Property [overloads] As Type()
        Public Property func As RMethodInfo

        Public Shared Iterator Function GetOverloads(pkg As Type) As IEnumerable(Of RGenericOverloads)
            Dim methods As MethodInfo() = pkg.GetMethods(bindingAttr:=BindingFlags.Static)
            Dim flag As RGenericOverloadsAttribute
            Dim clr_overloads As Type()

            For Each f As MethodInfo In methods
                flag = f.GetCustomAttribute(Of RGenericOverloadsAttribute)

                If flag Is Nothing Then
                    Continue For
                Else
                    clr_overloads = flag.Overloads

                    If clr_overloads.IsNullOrEmpty Then
                        clr_overloads = {
                            f.GetParameters _
                                .First _
                                .ParameterType
                        }
                    End If

                    clr_overloads = clr_overloads _
                        .Select(Function(t)
                                    If t.IsArray Then
                                        Return t.GetElementType
                                    Else
                                        Return t
                                    End If
                                End Function) _
                        .ToArray
                End If

                Yield New RGenericOverloads With {
                   .name = flag.FunctionName,
                   .[overloads] = clr_overloads,
                   .func = New RMethodInfo(f)
                }
            Next
        End Function
    End Class
End Namespace
