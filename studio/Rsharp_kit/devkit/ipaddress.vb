#Region "Microsoft.VisualBasic::36a2f92fd4709bbf669781a2308c1dbe, studio\Rsharp_kit\devkit\ipaddress.vb"

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

'   Total Lines: 50
'    Code Lines: 33 (66.00%)
' Comment Lines: 11 (22.00%)
'    - Xml Docs: 90.91%
' 
'   Blank Lines: 6 (12.00%)
'     File Size: 1.88 KB


' Module ipaddress
' 
'     Function: cidr, print_string
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports Darwinism.IPC.Networking
Imports Microsoft.VisualBasic.CommandLine.Reflection
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
        Dim prefix As Integer = Integer.Parse(token(1))
        Dim netmaskSymbolic = IPUtils.GetSubnetMaskFromPrefixLength(prefix)
        Dim subnet As New IPv4(token(0), netmaskSymbolic.ToString)
        Return subnet
    End Function

End Module

