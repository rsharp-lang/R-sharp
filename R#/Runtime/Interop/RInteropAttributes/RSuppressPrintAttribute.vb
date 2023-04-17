#Region "Microsoft.VisualBasic::bbdd522c3de0d9731ad2697bbae3119d, D:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RSuppressPrintAttribute.vb"

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

    '   Total Lines: 99
    '    Code Lines: 18
    ' Comment Lines: 75
    '   Blank Lines: 6
    '     File Size: 10.04 KB


    '     Class RSuppressPrintAttribute
    ' 
    '         Function: IsPrintInvisible
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    ''' <summary>
    ''' The return value will not be print on the console.
    ''' (函数的返回值不会被自动打印出来)
    ''' </summary>
    ''' <remarks>
    ''' this custom attribute will create an <see cref="invisible"/>
    ''' wrapper of the object value.
    ''' </remarks>
    <AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Public Class RSuppressPrintAttribute : Inherits RInteropAttribute

        ' 20191227
        '
        ' The bug code use for get custom attribute:
        ' Return Not api.ReturnParameter.GetCustomAttribute(Of RSuppressPrintAttribute) Is Nothing
        '
        ' may be throw exception like:
        ' 
        ' System.Exception: apiInvoke 
        ' ---> System.Exception: D:\MassSpectrum-toolkits\Rscript\demo\mrm_quantify.R 
        ' ---> System.Exception: Execute file failure! 
        ' ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. 
        ' ---> System.IndexOutOfRangeException: Index was outside the bounds of the array.
        '
        '   at System.Attribute.GetParentDefinition(ParameterInfo param)
        '   at System.Attribute.InternalParamGetCustomAttributes(ParameterInfo param, Type type, Boolean inherit)
        '   at System.Attribute.GetCustomAttribute(ParameterInfo element, Type attributeType, Boolean inherit)
        '   at System.Reflection.CustomAttributeExtensions.GetCustomAttribute[T](ParameterInfo element)
        '   at SMRUCC.Rsharp.Runtime.Interop.RSuppressPrintAttribute.IsPrintInvisible(MethodInfo api) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RInteropAttribute.vb:line 87
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo..ctor(String name, MethodInfo closure, Object target) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 113
        '   at SMRUCC.Rsharp.Runtime.Internal.vbObject._Lambda$__8-5(MethodInfo m) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\objects\vbObject.vb:line 80
        '   at System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement](IEnumerable`1 source, Func`2 keySelector, Func`2 elementSelector, IEqualityComparer`1 comparer)
        '   at System.Linq.Enumerable.ToDictionary[TSource,TKey,TElement](IEnumerable`1 source, Func`2 keySelector, Func`2 elementSelector)
        '   at SMRUCC.Rsharp.Runtime.Internal.vbObject..ctor(Object obj) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\objects\vbObject.vb:line 70
        '   at SMRUCC.Rsharp.Runtime.Internal.RConversion.asObject(Object obj) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\objects\conversion.vb:line 77
        '   --- End of inner exception stack trace ---
        '   at System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)
        '   at System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)
        '   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
        '   at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
        '   at SMRUCC.Rsharp.Runtime.Interop.MethodInvoke.Invoke(Object[] parameters) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\MethodInvoke.vb:line 65
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.Invoke(Object[] parameters) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 164
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.Invoke(Environment envir, InvokeParameter[] params) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 196
        '   at SMRUCC.Rsharp.Runtime.Internal.invoke.invokeInternals(Environment envir, String funcName, InvokeParameter[] paramVals) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\invoke.vb:line 153
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.invokeRInternal(String funcName, Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 263
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.doInvokeFuncVar(RFunction funcVar, Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 218
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 159
        '   at SMRUCC.Rsharp.Runtime.Components.InvokeParameter.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Runtime\System\InvokeParameter.vb:line 111
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo._Closure$__22-0._Lambda$__1(RMethodArgument a) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 221
        '   at Microsoft.VisualBasic.Linq.PipelineExtensions.DoCall[T,Tout](T input, Func`2 apply) in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\Extensions\Collection\Linq\Pipeline.vb:line 63
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.createObjectListArguments(Environment envir, InvokeParameter[] params) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 217
        '   at SMRUCC.Rsharp.Runtime.Interop.RMethodInfo.Invoke(Environment envir, InvokeParameter[] params) in D:\GCModeller\src\R-sharp\R#\Runtime\Interop\RMethodInfo.vb:line 186
        '   at SMRUCC.Rsharp.Runtime.Internal.invoke.invokeInternals(Environment envir, String funcName, InvokeParameter[] paramVals) in D:\GCModeller\src\R-sharp\R#\Runtime\Internal\invoke.vb:line 153
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.invokeRInternal(String funcName, Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 263
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.doInvokeFuncVar(RFunction funcVar, Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 218
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.FunctionInvoke.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\FunctionInvoke.vb:line 159
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.DeclareNewVariable.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DeclareNewVariable.vb:line 139
        '   at SMRUCC.Rsharp.Interpreter.Program.ExecuteCodeLine(Expression expression, Environment envir, Boolean& breakLoop, Boolean debug) in D:\GCModeller\src\R-sharp\R#\Interpreter\Program.vb:line 123
        '   at SMRUCC.Rsharp.Interpreter.Program.Execute(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\Program.vb:line 94
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ClosureExpression.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\ClosureExpression.vb:line 90
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.DeclareNewFunction.Invoke(Environment parent, InvokeParameter[] params) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DeclareNewFunction.vb:line 198
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ForLoop.RunLoop(Object value, String loopTag, Environment env) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb:line 177
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ForLoop.VB$StateMachine_12_exec.MoveNext() in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb:line 186
        '   at SMRUCC.Rsharp.Interpreter.ExecuteEngine.ForLoop.Evaluate(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ForLoop.vb:line 145
        '   at SMRUCC.Rsharp.Interpreter.Program.ExecuteCodeLine(Expression expression, Environment envir, Boolean& breakLoop, Boolean debug) in D:\GCModeller\src\R-sharp\R#\Interpreter\Program.vb:line 123
        '   at SMRUCC.Rsharp.Interpreter.Program.Execute(Environment envir) in D:\GCModeller\src\R-sharp\R#\Interpreter\Program.vb:line 94
        '   at SMRUCC.Rsharp.Interpreter.RInterpreter.RunInternal(Rscript Rscript, NamedValue`1[] arguments) in D:\GCModeller\src\R-sharp\R#\Interpreter\RInterpreter.vb:line 252
        '   at SMRUCC.Rsharp.Interpreter.RInterpreter.Source(String filepath, NamedValue`1[] arguments) in D:\GCModeller\src\R-sharp\R#\Interpreter\RInterpreter.vb:line 283
        '   at Rterm.Program.RunScript(String filepath, CommandLine args) in D:\GCModeller\src\R-sharp\R-terminal\Program.vb:line 78
        '   at Microsoft.VisualBasic.CommandLine.Interpreter.apiInvoke(String commandName, Object[] argvs, String[] help_argvs) in D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\CommandLine\Interpreters\Interpreter.vb:line 236
        '   --- End of inner exception stack trace ---
        '   --- End of inner exception stack trace ---
        '   --- End of inner exception stack trace ---

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="api"></param>
        ''' <returns></returns>
        Public Shared Function IsPrintInvisible(api As MethodInfo) As Boolean
            Dim returns As ParameterInfo = api.ReturnParameter
            Dim attrs As CustomAttributeData() = returns.CustomAttributes.ToArray

            If attrs.Length = 0 Then
                Return False
            End If

            Return attrs _
                .Any(Function(a)
                         Return a.AttributeType Is GetType(RSuppressPrintAttribute)
                     End Function)
        End Function
    End Class
End Namespace
