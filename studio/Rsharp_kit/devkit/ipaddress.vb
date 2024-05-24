Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' ip address tools
''' </summary>
''' 
<Package("ipaddress")>
Module ipaddress

    Private Function print_string(ip As IPv4) As String
        Dim sb As New StringBuilder

        sb.AppendLine($"{ip.CIDR}      Quads      Hex                           Binary    Integer")
        sb.AppendLine($"------------- ------------")
        sb.AppendLine($"IP Address:   {ip.IPAddress}")
        sb.AppendLine($"Subnet Mask:   {ip.Netmask} ")
        sb.AppendLine($"Network Portion: {ip.WildcardMask}")
        sb.AppendLine($"Host Portion:   ")
        sb.AppendLine()
        sb.AppendLine($"Number of IP Addresses: {ip.numberOfHosts}")
        sb.AppendLine($"Number of Addressable Hosts: {ip.numberOfHosts}")
        sb.AppendLine($"IP Address Range: {ip.hostAddressRange}")
        sb.AppendLine($"Broadcast Address:   {ip.BroadcastAddress} ")
        sb.AppendLine($"Min Host:  ")
        sb.AppendLine($"Max Host:   ")
        sb.AppendLine($"IPv4 ARPA Domain:    ")

        Return sb.ToString
    End Function

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
