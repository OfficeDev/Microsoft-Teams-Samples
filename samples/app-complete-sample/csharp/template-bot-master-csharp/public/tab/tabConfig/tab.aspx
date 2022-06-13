<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="tab.aspx.cs" Inherits="Microsoft.Teams.TemplateBotCSharp.src.tab.tab" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="https://res.cdn.office.net/teams-js/2.0.0/js/MicrosoftTeams.min.js" crossorigin="anonymous"></script>
    <script src='https://code.jquery.com/jquery-1.11.3.min.js'></script>
</head>
<body>
    <form id="form1" runat="server">
        <div style="width: 15%; float: left;">
            <asp:Repeater ID="ListOfCodeFiles" OnItemDataBound="ListOfCodeFiles_OnItemDataBound" OnItemCommand="ListOfCodeFiles_OnItemCommand" runat="server">
                <HeaderTemplate>
                    <ul>
                </HeaderTemplate>
                <ItemTemplate>
                    <li>
                        <asp:LinkButton runat="server" ID="nameOfFile" CommandName="GOTO"></asp:LinkButton>
                    </li>
                </ItemTemplate>
                <FooterTemplate>
                    </ul>
                </FooterTemplate>
            </asp:Repeater>
        </div>
        <div style="width: 65%; float: right;">
            <div>
                <asp:Label ID="fileName" runat="server" ForeColor="Black" Font-Bold="true"></asp:Label>
            </div>
            <div id="divFileContent" runat="server" style="width: 90%; border: solid; border-width: 1px; padding-left: 25px; display: none">
                <asp:Label ID="fileContent" runat="server" ForeColor="Green"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>
