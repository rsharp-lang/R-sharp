Imports System.Collections.ObjectModel
Imports System.Text
Imports Microsoft.VisualBasic.SecurityString.MD5Hash

Namespace Interpreter.Linker.APIHandler

    ''' <summary>
    ''' 用于表示一个已经被签名的函数
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SignedFuncEntryPoint

        Dim _InternalTypeSignatureValue As String
        Dim _InternalEntryPoint As Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint
        ''' <summary>
        ''' 参数列表按照从小到大排序之后计算MD5哈希值作为参数列表的数字签名
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InternalParameterSignature As String

        ''' <summary>
        ''' 返回值的数字签名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TypeSignature As String
            Get
                Return _InternalTypeSignatureValue
            End Get
        End Property

        Public ReadOnly Property ParameterCounts As Integer
            Get
                Return Me._MyParametersHash.Count
            End Get
        End Property

        Public ReadOnly Property EntryPoint As Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint
            Get
                Return _InternalEntryPoint
            End Get
        End Property

        ''' <summary>
        ''' 当参数有多个的时候，出了第一个之外，其他的参数都是可选的或者类型为逻辑值，则该函数被定义为伪单参数函数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FakeSingleParameter As Boolean
            Get
                If Me._MyParametersHash.IsNullOrEmpty Then
                    Return False '函数没有参数，很明显不是单参数函数
                ElseIf Me._MyParametersHash.Count = 1
                    Return False  '这个是真实的单参数函数，但是我们要的是伪单参数函数
                End If

                Dim Tokens = Me._MyParametersHash.ToArray
                For i As Integer = 1 To Tokens.Length - 1 '跳过第一个参数
                    Dim Parameter = Tokens(i)
                    If Not (Parameter.Value.ParameterType.Equals(GetType(Boolean)) OrElse Parameter.Value.ParameterInfo.IsOptional) Then
                        Return False
                    End If
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' 参数列表按照从小到大排序之后计算MD5哈希值作为参数列表的数字签名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ParameterSignature As String
            Get
                Return _InternalParameterSignature
            End Get
        End Property

        Public ReadOnly Property Parameters As ReadOnlyDictionary(Of String, ParameterWithAlias)
            Get
                Return _MyParametersHash
            End Get
        End Property

        Dim _MyParametersHash As ReadOnlyDictionary(Of String, ParameterWithAlias)
        ''' <summary>
        ''' 非可选参数的数目
        ''' </summary>
        ''' <remarks></remarks>
        Dim _InternalNonOptionalCounts As Integer

        Public Structure ParameterWithAlias
            Dim ParameterInfo As System.Reflection.ParameterInfo, [Alias] As Microsoft.VisualBasic.Scripting.MetaData.Parameter

            Public ReadOnly Property ParameterType As Type
                Get
                    Return ParameterInfo.ParameterType
                End Get
            End Property

            Sub New(ParameterInfo As System.Reflection.ParameterInfo, [Alias] As Microsoft.VisualBasic.Scripting.MetaData.Parameter)
                Me.ParameterInfo = ParameterInfo
                Me.Alias = If([Alias] Is Nothing, New Microsoft.VisualBasic.Scripting.MetaData.Parameter(ParameterInfo.Name.ToLower), [Alias])
            End Sub

            Public Overrides Function ToString() As String
                If String.IsNullOrEmpty([Alias].Description) Then
                    Return [Alias].Alias
                Else
                    Return [Alias].Alias & ": " & [Alias].Description
                End If
            End Function
        End Structure

        Sub New(EntryPoint As Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint, [Handles] As Microsoft.VisualBasic.Scripting.MetaData.OverloadsSignatureHandle())
            Call Me._InternalEntryPoint.InvokeSet(EntryPoint)
            Call Me._InternalHandlesTypeSignature([Handles])

            Dim pInfo As System.Reflection.ParameterInfo() = EntryPoint.EntryPoint.GetParameters '在这里生成变量名的别名
            Dim InternalGetSigned = (From parameter As System.Reflection.ParameterInfo In pInfo
                                     Let pAlias = Microsoft.VisualBasic.Scripting.MetaData.Parameter.GetParameterNameAlias(parameter, False)
                                     Let Name As String = If(pAlias Is Nothing, parameter.Name, pAlias.Alias).ToLower
                                     Select Name, parameter.ParameterType.FullName, pAlias, parameter).ToArray '这里不能够打乱原始顺序！！！
            Me._InternalParameterSignature = String.Join("+", (From sign In InternalGetSigned Let strSignValue As String = sign.ToString Select strSignValue).ToArray)

            If String.IsNullOrEmpty(ParameterSignature) Then
                _InternalParameterSignature = "NULL" '函数不需要任何参数
            Else
                _InternalParameterSignature = GetMd5Hash(ParameterSignature)
            End If

            Me._MyParametersHash = New ReadOnlyDictionary(Of String, ParameterWithAlias)(
                InternalGetSigned.ToDictionary(keySelector:=Function(p) p.Name.ToLower, elementSelector:=Function(obj) New ParameterWithAlias(obj.parameter, obj.pAlias)))
            Me._InternalNonOptionalCounts = (From p As KeyValuePair(Of String, ParameterWithAlias)
                                             In _MyParametersHash
                                             Where Not p.Value.ParameterInfo.IsOptional
                                             Select 1).ToArray.Sum
        End Sub

        Private Sub _InternalHandlesTypeSignature([Handles] As Microsoft.VisualBasic.Scripting.MetaData.OverloadsSignatureHandle())
            If [Handles].IsNullOrEmpty Then
                Me._InternalTypeSignatureValue = EntryPoint.EntryPoint.ReturnType.FullName
            End If

            Dim TypeSignature = (From Hwnd In [Handles] Where Hwnd.FullName = EntryPoint.EntryPoint.ReturnType Select Hwnd).FirstOrDefault

            If TypeSignature Is Nothing Then  '没有定义返回值的签名，则直接使用返回值的全名
                Me._InternalTypeSignatureValue = EntryPoint.EntryPoint.ReturnType.FullName
            Else
                Me._InternalTypeSignatureValue = TypeSignature.TypeIDBrief  'var <- [typeidbrief] function <parameters>
            End If
        End Sub

        ''' <summary>
        ''' 创建共享方法的签名实例
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateObject(EntryPoint As System.Reflection.MethodInfo, [Handles] As Microsoft.VisualBasic.Scripting.MetaData.OverloadsSignatureHandle()) As SignedFuncEntryPoint
            Dim EntryPointInfo As New CommandLine.Reflection.EntryPoints.APIEntryPoint(New CommandLine.Reflection.ExportAPIAttribute("VB$InternalAnonymousSharedMethod"), Invoke:=EntryPoint)
            Return New SignedFuncEntryPoint(EntryPointInfo, [Handles])
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", ParameterSignature, EntryPoint.ToString)
        End Function

        Public Function GetDescription(DescriptionGeneration As Func(Of System.Reflection.MethodInfo, String, String)) As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            Call sBuilder.AppendLine("[" & Me.EntryPoint.EntryPointFullName(False) & "]" & vbCrLf)
            Call sBuilder.AppendLine(DescriptionGeneration(Me.EntryPoint.EntryPoint, ParameterSignature))

            Return sBuilder.ToString
        End Function

        Public Function CanDelegateCalling(paras As Object()) As Boolean

            If _MyParametersHash.IsNullOrEmpty Then
                Return True
            ElseIf ParameterCounts > paras.Length Then '可能有可选参数
                Dim p As KeyValuePair(Of String, ParameterWithAlias)() =
                    Me._MyParametersHash.ToArray.Skip(paras.Length).ToArray
                Dim LQuery = (From ParameterInfo In p Where ParameterInfo.Value.ParameterInfo.IsOptional Select ParameterInfo).ToArray
                Return LQuery.Length = p.Length
            End If

            Dim InternalHashList = _MyParametersHash.ToArray

            For i As Integer = 0 To paras.Length - 1
                If Alignment.TypeEquals.TypeEquals(InternalHashList(i).Value.ParameterType, paras(i).GetType) < 0 Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 判断两个函数入口点对象是否具有完全一样的数字签名
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Equals(obj As Object) As Boolean
            If Not TypeOf (obj) Is SignedFuncEntryPoint Then
                Return False
            End If

            Dim Signature As SignedFuncEntryPoint = DirectCast(obj, SignedFuncEntryPoint)

            Return String.Equals(ParameterSignature, Signature.ParameterSignature) AndAlso String.Equals(TypeSignature, Signature.TypeSignature, StringComparison.OrdinalIgnoreCase)
        End Function
    End Class
End Namespace