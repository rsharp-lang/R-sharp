#Region "Microsoft.VisualBasic::bb341ec2db29a6d5e419a9083365d246, R#\Language\Syntax\SyntaxImplements\VectorLiteral.vb"

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

'   Total Lines: 244
'    Code Lines: 196 (80.33%)
' Comment Lines: 14 (5.74%)
'    - Xml Docs: 35.71%
' 
'   Blank Lines: 34 (13.93%)
'     File Size: 10.90 KB


'     Module VectorLiteralSyntax
' 
'         Function: GetVectorElements, LiteralSyntax, ParseAnnotation, ParseAnnotations, (+2 Overloads) SequenceLiteral
'                   TypeCodeOf, VectorLiteral
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module VectorLiteralSyntax

        Public Function UnitLiteralSyntax(value As Token, unit As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim constant As SyntaxResult = LiteralSyntax(value, opts)

            If constant.isException Then
                Return constant
            End If

            Dim num As Double = CType(constant.expression, Literal).Evaluate(Nothing)

            Select Case unit.text.ToLower
            ' --- Byte Size (Standard: Byte) ---
            ' 假设 ByteSize 类已定义并包含 PB, EB 等常量，若没有可自行定义为数字
                Case "b" : Return New Literal(num)
                Case "kb" : Return New Literal(num * ByteSize.KB)
                Case "mb" : Return New Literal(num * ByteSize.MB)
                Case "gb" : Return New Literal(num * ByteSize.GB)
                Case "tb" : Return New Literal(num * ByteSize.TB)
                Case "pb" : Return New Literal(num * ByteSize.PB) ' Petabyte
                Case "eb" : Return New Literal(num * ByteSize.EB) ' Exabyte

            ' --- Time (Standard: Second) ---
                Case "ms" : Return New Literal(num / 1000)
                Case "s" : Return New Literal(num)
                Case "min" : Return New Literal(num * 60)
                Case "h" : Return New Literal(num * 60 * 60)
                Case "day" : Return New Literal(num * 60 * 60 * 24)
                Case "week" : Return New Literal(num * 604800)          ' 周 (7天)
                Case "year" : Return New Literal(num * 31557600)        ' 年 (儒略年)
                Case "us", "µs" : Return New Literal(num * 0.000001)     ' 微秒
                Case "ns" : Return New Literal(num * 0.000000001)       ' 纳秒

            ' --- Distance / Length (Standard: Meter) ---
                Case "mm" : Return New Literal(num * 0.001)
                Case "cm" : Return New Literal(num * 0.01)
                Case "m" : Return New Literal(num)
                Case "km" : Return New Literal(num * 1000)
                Case "inch", "in" : Return New Literal(num * 0.0254)    ' 英寸
                Case "ft" : Return New Literal(num * 0.3048)           ' 英尺
                Case "yd" : Return New Literal(num * 0.9144)           ' 码
                Case "mi" : Return New Literal(num * 1609.344)          ' 英里
                Case "um" : Return New Literal(num * 0.000001)         ' 微米
                Case "nm" : Return New Literal(num * 0.000000001)      ' 纳米
                Case "angstrom", "å" : Return New Literal(num * 0.0000000001) ' 埃 (常见物理/化学单位)

            ' --- Weight / Mass (Standard: Kilogram) ---
                Case "mg" : Return New Literal(num * 0.000001)
                Case "g" : Return New Literal(num * 0.001)
                Case "kg" : Return New Literal(num)
                Case "t" : Return New Literal(num * 1000)              ' 吨
                Case "lb", "lbs" : Return New Literal(num * 0.45359237)' 磅
                Case "oz" : Return New Literal(num * 0.02834952)        ' 盎司
                Case "amu", "u" : Return New Literal(num * 1.6605390666E-27) ' 原子质量单位

            ' --- Volume (Standard: Liter) ---
                Case "ml" : Return New Literal(num * 0.001)
                Case "l" : Return New Literal(num)
                Case "m3" : Return New Literal(num * 1000)             ' 立方米 (1 m3 = 1000 L)
                Case "gal" : Return New Literal(num * 3.785411784)    ' 加仑 (美制)

            ' --- Pressure (Standard: Pascal) ---
            ' 物理/化学常见单位
                Case "pa" : Return New Literal(num)                    ' 帕斯卡
                Case "kpa" : Return New Literal(num * 1000)
                Case "mpa" : Return New Literal(num * 1000000)
                Case "bar" : Return New Literal(num * 100000)          ' 巴
                Case "atm" : Return New Literal(num * 101325)          ' 标准大气压
                Case "mmhg", "torr" : Return New Literal(num * 133.322368) ' 毫米汞柱 / 托
                Case "psi" : Return New Literal(num * 6894.75729)      ' 磅每平方英寸

            ' --- Energy (Standard: Joule) ---
                Case "j" : Return New Literal(num)                     ' 焦耳
                Case "kj" : Return New Literal(num * 1000)             ' 千焦
                Case "cal" : Return New Literal(num * 4.184)           ' 卡路里
                Case "kcal" : Return New Literal(num * 4184)           ' 千卡/大卡
                Case "ev" : Return New Literal(num * 1.602176634E-19)  ' 电子伏特 (物理/化学常用)
                Case "kwh" : Return New Literal(num * 3600000)         ' 千瓦时

            ' --- Force (Standard: Newton) ---
                Case "n" : Return New Literal(num)                     ' 牛顿
                Case "kn" : Return New Literal(num * 1000)              ' 千牛
                Case "lbf" : Return New Literal(num * 4.4482216152605) ' 磅力
                Case "kgf" : Return New Literal(num * 9.80665)        ' 千克力

            ' --- Power (Standard: Watt) ---
                Case "w" : Return New Literal(num)                     ' 瓦特
                Case "kw" : Return New Literal(num * 1000)             ' 千瓦
                Case "mw" : Return New Literal(num * 1000000)          ' 兆瓦
                Case "hp" : Return New Literal(num * 745.699872)       ' 马力

            ' --- Temperature (Standard: Kelvin) ---
            ' 注意：温度转换涉及偏移量，不仅仅是乘数
            ' 这里将输入转换为开尔文(K)进行标准化存储
                Case "k" : Return New Literal(num)                     ' 开尔文
                Case "c" : Return New Literal(num + 273.15)            ' 摄氏度 -> 开尔文
                Case "f" : Return New Literal((num + 459.67) * 5 / 9)  ' 华氏度 -> 开尔文

            ' --- Amount of Substance (Standard: Mole) ---
            ' 化学常用单位
                Case "mol" : Return New Literal(num)                   ' 摩尔
                Case "mmol" : Return New Literal(num * 0.001)         ' 毫摩尔
                Case "umol" : Return New Literal(num * 0.000001)      ' 微摩尔

                                ' ========================================================
            ' 新增：经济学与统计学单位
            ' ========================================================

            ' --- Statistics & Ratios ---
                Case "%" : Return New Literal(num / 100)           ' 百分比 -> 纯小数
                Case "‰", "per" : Return New Literal(num / 1000)   ' 千分比 -> 纯小数
                Case "bps", "bp" : Return New Literal(num / 10000) ' 基点 (Basis Point, 金融/统计常用) -> 纯小数
                Case "pp" : Return New Literal(num / 100)          ' 百分点 (Percentage Point，数值与%相同，但含义不同)
                Case "sigma", "σ" : Return New Literal(num)        ' 标准差倍数 (直接存储倍数)

            ' --- Economics / Currency (Scalar units) ---
            ' 注意：货币转换通常基于汇率，这里仅作为数量级单位处理（假设基准货币为1）
                Case "k$" : Return New Literal(num * 1000)         ' 千美元
                Case "m$" : Return New Literal(num * 1000000)      ' 百万美元
                Case "b$" : Return New Literal(num * 1000000000)   ' 十亿美元
                Case "cnym" : Return New Literal(num * 10000000)   ' 千万人民币 (常用单位)

            ' ========================================================
            ' 新增：生物学单位
            ' ========================================================

            ' --- Biology: Concentration (Molarity) ---
            ' 避免与 Meter (m) 和 Mega (M) 冲突，使用全拼或特定组合
                Case "molar" : Return New Literal(num)             ' M (摩尔/升)
                Case "mmolar" : Return New Literal(num * 0.001)    ' mM (毫摩尔)
                Case "umolar" : Return New Literal(num * 0.000001) ' μM (微摩尔)
                Case "nmolar" : Return New Literal(num * 0.000000001) ' nM (纳摩尔)

            ' --- Biology: Length (DNA/Protein) ---
                Case "bp" : Return New Literal(num)      ' 碱基对
                Case "kbp" : Return New Literal(num * 1000)      ' 千碱基对
                Case "aa" : Return New Literal(num)       ' 氨基酸残基

            ' --- Biology: Mass (Protein/Molecules) ---
                Case "kda" : Return New Literal(num * 1.6605390666E-24) ' 千道尔顿 -> 千克
                Case "da" : Return New Literal(num * 1.6605390666E-27)   ' 道尔顿 -> 千克

            ' --- Biology: Activity / Centrifuge ---
                Case "rpm" : Return New Literal(num / 60)          ' 每分钟转速 -> 赫兹 (每秒转数)
                Case "iu" : Return New Literal(num)                ' 国际单位 (酶活力/维生素等，无量纲或特定单位，通常按数值计算)
                Case "u" : Return New Literal(num)                 ' 酶单位 (同IU，在某些语境下区分，这里简化为数值)

                Case Else
                    Return SyntaxResult.CreateError(
                    err:=New SyntaxErrorException($"Not implemented literal unit token: {unit.text}"),
                    opts:=opts.SetCurrentRange({value})
                )
            End Select
        End Function


        Public Function LiteralSyntax(token As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Select Case token.name
                Case TokenType.booleanLiteral
                    Return New Literal With {.m_type = TypeCodes.boolean, .value = token.text.ParseBoolean}
                Case TokenType.integerLiteral
                    Return New Literal With {.m_type = TypeCodes.integer, .value = CLng(token.text.ParseInteger)}
                Case TokenType.numberLiteral
                    Return New Literal With {.m_type = TypeCodes.double, .value = token.text.ParseDouble}
                Case TokenType.stringLiteral, TokenType.cliShellInvoke
                    Return New Literal With {.m_type = TypeCodes.string, .value = token.text}
                Case TokenType.missingLiteral
                    Dim type As TypeCodes = TypeCodes.NA
                    Dim value As Object

                    Select Case token.text
                        ' null literal for R/python/javascript
                        Case "NULL", "None", "null" : value = Nothing
                        Case "NA", "NA_real_", "NA_integer_", "NA_complex_", "NA_character_" : value = GetType(Void)
                        Case "Inf" : value = Double.PositiveInfinity
                        Case Else
                            Return SyntaxResult.CreateError(
                                err:=New SyntaxErrorException($"Unknown literal token: {token.ToString}"),
                                opts:=opts.SetCurrentRange({token})
                            )
                    End Select

                    Return New Literal With {
                        .m_type = type,
                        .value = value
                    }
                Case Else
                    Return SyntaxResult.CreateError(
                        err:=New InvalidExpressionException(token.ToString),
                        opts:=opts.SetCurrentRange({token})
                    )
            End Select
        End Function

        <Extension>
        Public Iterator Function ParseAnnotations(blocks As Token()) As IEnumerable(Of NamedValue(Of String))
            For Each block As Token() In blocks.Split(4)
                Yield block.Skip(1).Take(2).ToArray.ParseAnnotation
            Next
        End Function

        <Extension>
        Public Function ParseAnnotation(block As Token()) As NamedValue(Of String)
            Dim name As String = block(Scan0).text.Substring(1)
            Dim value As String = block(1).text

            Return New NamedValue(Of String)(name, value)
        End Function

        <Extension>
        Private Function GetVectorElements(tokens As Token()) As List(Of Token())
            tokens = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .ToArray

            Dim isSimples As Boolean = tokens _
                .All(Function(a)
                         Return a.isLiteral OrElse
                            a.name = TokenType.stringInterpolation OrElse
                            a.name = TokenType.identifier OrElse
                            a.name = TokenType.keyword
                     End Function)

            If isSimples Then
                Return tokens.Select(Function(a) New Token() {a}).AsList
            Else
                Return tokens.SplitByTopLevelDelimiter(TokenType.comma)
            End If
        End Function

        Public Function VectorLiteral(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens.GetVectorElements
            Dim values As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            If blocks Is Nothing AndAlso tokens.Last.name = TokenType.cliShellInvoke Then
                Dim cli = CommandLineSyntax.CommandLine(tokens.Last, opts)

                If cli.isException Then
                    Return cli
                End If

                blocks = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 3) _
                    .SplitByTopLevelDelimiter(TokenType.comma)

                Dim annotation As NamedValue(Of String) = blocks(Scan0).ParseAnnotation
                DirectCast(cli.expression, ExternalCommandLine).SetAttribute(annotation)
                Return cli
            ElseIf blocks.Count = 1 AndAlso blocks(Scan0).Length = 2 Then
                Dim block As Token() = blocks(Scan0)

                ' is user annotation
                If block(Scan0).name = TokenType.annotation AndAlso block(1).isLiteral Then
                    Dim annotation As NamedValue(Of String) = block.ParseAnnotation
                    Call opts.annotations.Add(annotation)
                    Return New CodeComment($"{annotation.Name}:={annotation.Value}")
                End If
            End If

            For Each block As Token() In blocks
                ' is a comma symbol
                If block.Length = 1 AndAlso block(Scan0).name = TokenType.comma Then
                    Continue For
                End If

                syntaxTemp = block.DoCall(Function(code) opts.ParseExpression(code))

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    values.Add(syntaxTemp.expression)
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Return New SyntaxResult(New VectorLiteral(values, values.DoCall(AddressOf TypeCodeOf)))
        End Function

        ''' <summary>
        ''' get type code value of the vector literal 
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function TypeCodeOf(values As IEnumerable(Of Expression)) As TypeCodes
            With values.ToArray
                ' fix for System.InvalidOperationException: Nullable object must have a value.
                '
                If .Length = 0 Then
                    Return TypeCodes.generic
                ElseIf .Length = 1 Then
                    Return DirectCast(.GetValue(Scan0), Expression).type
                Else
                    ' generic > string > double > float > long > integer > byte > boolean
                    Static typeCodeWeights As Index(Of TypeCodes) = {
                        TypeCodes.boolean,
                        TypeCodes.integer,
                        TypeCodes.double,
                        TypeCodes.string,
                        TypeCodes.generic
                    }

                    ' get unique types
                    Dim types As TypeCodes() = .Select(Function(exp)
                                                           Dim t = exp.type

                                                           If t = TypeCodes.NA Then
                                                               t = TypeCodes.generic
                                                           End If

                                                           Return t
                                                       End Function) _
                                               .Distinct _
                                               .ToArray
                    If types.Length = 1 Then
                        Return types(Scan0)
                    Else
                        Dim maxType As TypeCodes = TypeCodes.boolean
                        Dim maxWeight As Integer

                        For Each code As TypeCodes In types
                            If typeCodeWeights.IndexOf(code) > maxWeight Then
                                maxType = code
                                maxWeight = typeCodeWeights(maxType)
                            End If
                        Next

                        Return maxType
                    End If
                End If
            End With
        End Function

        Public Function SequenceLiteral(from As Token, [to] As Token, steps As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            Return SequenceLiteral({from}, {[to]}, {steps}, opts)
        End Function

        Public Function SequenceLiteral(from As Token(), [to] As Token(), steps As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim fromSyntax = opts.ParseExpression(from)
            Dim toSyntax = opts.ParseExpression([to])
            Dim sourceMap As StackFrame = opts.GetStackTrace(from(Scan0), "sequence")

            If fromSyntax.isException Then
                Return fromSyntax
            ElseIf toSyntax.isException Then
                Return toSyntax
            End If

            If steps.IsNullOrEmpty Then
                Return New SequenceLiteral(fromSyntax.expression, toSyntax.expression, New Literal(1), sourceMap)
            ElseIf steps.isLiteral Then
                Dim stepLiteral As SyntaxResult = SyntaxImplements.LiteralSyntax(steps(Scan0), opts)

                If stepLiteral.isException Then
                    Return stepLiteral
                End If

                Return New SequenceLiteral(
                    from:=fromSyntax.expression,
                    [to]:=toSyntax.expression,
                    steps:=stepLiteral.expression,
                    stackFrame:=sourceMap
                )
            Else
                Dim stepsSyntax As SyntaxResult = opts.ParseExpression(steps)

                If stepsSyntax.isException Then
                    Return stepsSyntax
                Else
                    Return New SequenceLiteral(
                        from:=fromSyntax.expression,
                        [to]:=toSyntax.expression,
                        steps:=stepsSyntax.expression,
                        stackFrame:=sourceMap
                    )
                End If
            End If
        End Function
    End Module
End Namespace
