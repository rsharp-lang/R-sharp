Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal

    Public Module classPrinter

        ''' <summary>
        ''' A utils function for print user defined structure and class object instance. 
        ''' </summary>
        ''' <param name="obj">
        ''' The object class is ensure that not nothing!
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' All of the method is non generic, and the property without arguments!
        ''' All of the sub program will be mapping as returns nothing
        ''' </remarks>
        Public Function printClass(obj As Object) As String
            Dim type As Type = obj.GetType
            Dim properties As PropertyInfo() = type.GetProperties.ToArray
            Dim methods As MethodInfo() = type.GetMethods.ToArray
            Dim sb As New StringBuilder

            Call sb.AppendLine($"namespace: {type.Namespace}")
            Call sb.AppendLine($"instance of '{type.Name}'")
            Call sb.AppendLine()
            Call sb.AppendLine($" {properties.Length} properties")

            For Each [property] As PropertyInfo In properties
                Call sb.AppendLine($"  ${[property].Name}")
            Next

            Call sb.AppendLine()
            Call sb.AppendLine($" {methods.Length} methods")

            For Each method As MethodInfo In methods
                Call sb.AppendLine($"  -> {method.Name}")
            Next

            Return sb.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function getMethods(type As Type) As IEnumerable(Of MethodInfo)
            Return type _
                .GetMethods(PublicProperty) _
                .Where(Function(m)
                           Return Not m.ContainsGenericParameters
                       End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function getProperties(type As Type) As IEnumerable(Of PropertyInfo)
            Return type _
                .GetProperties(PublicProperty) _
                .Where(Function(p)
                           Return p.GetIndexParameters.IsNullOrEmpty
                       End Function)
        End Function
    End Module
End Namespace