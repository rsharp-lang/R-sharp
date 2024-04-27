#Region "Microsoft.VisualBasic::11138971c48613e1f7825ee2f45ed40d, G:/GCModeller/src/R-sharp/R#//Runtime/Interop/RsharpApi/RMethodArgument.vb"

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

    '   Total Lines: 188
    '    Code Lines: 106
    ' Comment Lines: 65
    '   Blank Lines: 17
    '     File Size: 7.58 KB


    '     Class RMethodArgument
    ' 
    '         Properties: [default], acceptFormula, isByrefValueParameter, isObjectList, isOptional
    '                     isRequireRawVector, name, requireRawExpression, type
    ' 
    '         Function: getDefaultValue, getDefaultVector, GetRawVectorElementType, ParseArgument, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Runtime.Interop

    ''' <summary>
    ''' The R# method argument wrapper
    ''' </summary>
    Public Class RMethodArgument : Implements INamedValue

        ''' <summary>
        ''' The argument name
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' is a reference of <see cref="PropertyInfo.Name"/>
        ''' </remarks>
        Public Property name As String Implements INamedValue.Key
        ''' <summary>
        ''' the type of this parameter that required
        ''' </summary>
        ''' <returns></returns>
        Public Property type As RType
        ''' <summary>
        ''' default value 
        ''' </summary>
        ''' <returns></returns>
        Public Property [default] As Object
        ''' <summary>
        ''' is an optional parameter?
        ''' </summary>
        ''' <returns></returns>
        Public Property isOptional As Boolean
        ''' <summary>
        ''' is a ``...`` list argument?
        ''' </summary>
        ''' <returns></returns>
        Public Property isObjectList As Boolean
        ''' <summary>
        ''' is a parameter that accept the byref value to this function?
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' byref call: 
        ''' func(...) &lt;- value;
        ''' 
        ''' call in normal function invoke style:
        ''' func(value, ...);
        ''' 
        ''' if this property is TRUE, then it meansthe byref assign of the value will
        ''' passing to this parameter.
        ''' </remarks>
        Public Property isByrefValueParameter As Boolean

        ''' <summary>
        ''' Do not apply the <see cref="Runtime.getFirst"/> operation
        ''' </summary>
        ''' <returns></returns>
        Public Property isRequireRawVector As Boolean

        Friend rawVectorFlag As RRawVectorArgumentAttribute
        Friend defaultScriptValue As RDefaultValueAttribute
        Friend islazyeval As Boolean

        Public ReadOnly Property requireRawExpression As Boolean
            Get
                Return type IsNot Nothing AndAlso type.raw.IsInheritsFrom(GetType(Expression), strict:=False)
            End Get
        End Property

        Public ReadOnly Property acceptFormula As Boolean
            Get
                If type Is Nothing Then
                    Return False
                End If

                Select Case type.raw
                    Case GetType(Object), GetType(FormulaExpression), GetType(Expression)
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Get element type of the target raw vector
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRawVectorElementType() As Type
            If Not rawVectorFlag Is Nothing Then
                Return rawVectorFlag.vector
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' show markdown document of this parameter
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return markdown()
        End Function

        Public Shared Function ParseArgument(p As ParameterInfo, is_lambda As Boolean) As RMethodArgument
            ' System.MissingMethodException: .ctor 
            Dim rawVectorFlag = p.GetCustomAttribute(Of RRawVectorArgumentAttribute)
            Dim [default] = p.GetCustomAttribute(Of RDefaultValueAttribute)
            Dim isObj As Boolean = Not p.GetCustomAttribute(Of RListObjectArgumentAttribute) Is Nothing
            Dim isByref As Boolean = Not p.GetCustomAttribute(Of RByRefValueAssignAttribute) Is Nothing
            Dim hasExpression As Boolean = Not p.GetCustomAttribute(Of RDefaultExpressionAttribute) Is Nothing
            Dim islazyeval As Boolean = Not p.GetCustomAttribute(Of RLazyExpressionAttribute) Is Nothing

            If is_lambda AndAlso p.ParameterType Is GetType(Object) Then
                ' 20231204
                ' object type of the clr lambda parameter always be a vector
                If rawVectorFlag Is Nothing Then
                    rawVectorFlag = New RRawVectorArgumentAttribute
                End If
            End If

            Return New RMethodArgument With {
                .name = p.Name,
                .type = RType.GetRSharpType(p.ParameterType),
                .rawVectorFlag = rawVectorFlag,
                .defaultScriptValue = [default],
                .[default] = getDefaultValue(
                    rawVector:= .rawVectorFlag,
                    defaultScript:= .defaultScriptValue,
                    paramType:=p.ParameterType,
                    hasExpression:=hasExpression,
                    [default]:=p.DefaultValue
                ),
                .isOptional = p.HasDefaultValue,
                .isObjectList = isObj,
                .isRequireRawVector = Not .rawVectorFlag Is Nothing,
                .isByrefValueParameter = isByref,
                .islazyeval = islazyeval
            }
        End Function

        ''' <summary>
        ''' get the default value of this parameter
        ''' </summary>
        ''' <param name="rawVector"></param>
        ''' <param name="defaultScript"></param>
        ''' <param name="paramType"></param>
        ''' <param name="hasExpression"></param>
        ''' <param name="[default]"></param>
        ''' <returns></returns>
        Private Shared Function getDefaultValue(rawVector As RRawVectorArgumentAttribute,
                                                defaultScript As RDefaultValueAttribute,
                                                paramType As Type,
                                                hasExpression As Boolean,
                                                [default] As Object) As Object

            If hasExpression AndAlso TypeOf [default] Is String AndAlso DirectCast([default], String).FirstOrDefault = "~"c Then
                Return RDefaultExpressionAttribute.ParseDefaultExpression(CStr([default]))
            ElseIf rawVector Is Nothing Then
                If Not defaultScript Is Nothing Then
                    Return defaultScript.ParseDefaultValue(paramType)
                Else
                    Return [default]
                End If
            Else
                Return getDefaultVector(rawVector, [default])
            End If
        End Function

        Private Shared Function getDefaultVector(flag As RRawVectorArgumentAttribute, [default] As Object) As Object
            If flag Is Nothing OrElse [default] Is Nothing Then
                Return [default]
            ElseIf Not flag.containsLiteral Then
                Return [default]
            End If

            If [default].GetType Is GetType(String) Then
                ' parser works for string expression only
                Return flag.GetVector(DirectCast([default], String))
            Else
                Return [default]
            End If
        End Function
    End Class
End Namespace
