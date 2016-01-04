Imports System.Reflection

Namespace Runtime.HybridsScripting

    Public Module EnvironmentParser

        Public Function [Imports](Assembly As System.Type) As EntryPoint
            Dim attributes As Object() = Assembly.GetCustomAttributes(ShoalShell.Runtime.HybridsScripting.LanguageEntryPoint.TypeInfo, True)

            If attributes.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim InitEntry As System.Reflection.MethodInfo = GetEntry(Assembly, EntryInterface.InterfaceTypes.EntryPointInit)
            Dim Evaluate As System.Reflection.MethodInfo = GetEntry(Assembly, EntryInterface.InterfaceTypes.Evaluate)
            Dim SetValue As System.Reflection.MethodInfo = GetEntry(Assembly, EntryInterface.InterfaceTypes.SetValue)
            Dim DataConvertors = GetEntries(Of HybridsScripting.DataTransform)(Assembly)
            Dim ConservedString As System.Reflection.MethodInfo = (From cMethod As KeyValuePair(Of HybridsScripting.DataTransform, System.Reflection.MethodInfo)
                                                                   In DataConvertors
                                                                   Where cMethod.Key.ReservedStringTLTR = True
                                                                   Select cMethod.Value).FirstOrDefault
            If Evaluate Is Nothing Then
                Return Nothing
            Else
                Return New EntryPoint With
                       {
                           .DeclaredAssemblyType = Assembly,
                           .ConservedString = ConservedString,
                           .Language = DirectCast(attributes(0), ShoalShell.Runtime.HybridsScripting.LanguageEntryPoint),
                           .Init = InitEntry,
                           .Evaluate = Evaluate,
                           .TypeFullName = Assembly.FullName,
                           .SetValue = SetValue,
                           .DataConvertors = New SortedDictionary(Of Char, System.Reflection.MethodInfo)(DataConvertors.ToDictionary(Function(item) item.Key.TypeChar, elementSelector:=Function(item) item.Value))}
            End If
        End Function

        Private Function GetEntries(Of TEntryType As EntryInterface)(TypeInfo As System.Type) As KeyValuePair(Of TEntryType, System.Reflection.MethodInfo)()
            Dim EntryType As Type = GetType(TEntryType)
            Dim LQuery = (From LoadHandle As System.Reflection.MethodInfo
                          In TypeInfo.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                          Let attributes As Object() = LoadHandle.GetCustomAttributes(EntryType, False)
                          Where Not attributes.IsNullOrEmpty
                          Select (From attr As Object
                                  In attributes
                                  Let Entry As TEntryType = DirectCast(attr, TEntryType)
                                  Select New KeyValuePair(Of TEntryType, System.Reflection.MethodInfo)(Entry, LoadHandle)).ToArray).ToArray
            Return LQuery.MatrixToVector
        End Function

        Private Function GetEntry(TypeInfo As System.Type, EntryType As EntryInterface.InterfaceTypes) As System.Reflection.MethodInfo
            Dim LQuery = (From LoadHandle As System.Reflection.MethodInfo
                          In TypeInfo.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                          Let attributes As Object() = LoadHandle.GetCustomAttributes(ShoalShell.Runtime.HybridsScripting.EntryInterface.TypeInfo, False)
                          Where Not attributes.IsNullOrEmpty AndAlso DirectCast(attributes(0), ShoalShell.Runtime.HybridsScripting.EntryInterface).InterfaceType = EntryType
                          Select LoadHandle).ToArray
            Return LQuery.FirstOrDefault
        End Function
    End Module
End Namespace