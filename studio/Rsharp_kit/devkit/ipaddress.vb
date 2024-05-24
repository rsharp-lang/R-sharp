Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' ip address tools
''' </summary>
''' 
<Package("ipaddress")>
Module ipaddress

    ''' <summary>
    ''' Network calculator for subnet mask and other classless (CIDR) network information.
    ''' </summary>
    ''' <param name="network">
    ''' an ipv4 network string, example as: 192.168.112.203/23
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("CIDR")>
    Public Function cidr(network As String) As IPv4
        Dim token As String() = network.Split("/"c)
        Dim netmask As Integer = Integer.Parse(token(1))
        Dim subnet As New IPv4(token(0), IPv4.NumericNetmaskToSymbolic(netmask))
        Return subnet
    End Function

End Module
