<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="MusicTuitionAttendance.Attendance" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .heading4 {
            text-decoration: underline;
        }

        .auto-style1 {
            font-weight: normal;
        }

        #StudentsGridView {
            text-align: left;
        }
    </style>
</head>
<body style="font-weight: 700">
    <form id="form1" runat="server">
        <div>
            <p>
                <asp:Label ID="TutorUsernameTitle" runat="server" Text="Tutor username: " Font-Bold="True"></asp:Label><asp:Label ID="lblTutorUsername" runat="server" Font-Bold="False"></asp:Label>
            </p>
            <p>
                <asp:Label ID="TutorNameTitle" runat="server" Text="Tutor name: " Font-Bold="True"></asp:Label><asp:Label ID="lblTutorName" runat="server" Font-Bold="False"></asp:Label>
            </p>
        </div>
        <asp:GridView ID="StudentsGridView" runat="server"
            AutoGenerateColumns="False"
            DataKeyNames="StudentId"
            OnRowCommand="StudentsGridView_RowCommand">
            <EmptyDataTemplate>
                <p style="font-weight: bold; color: #CC0000;">
                    No students found for this tutor! 
                </p>
            </EmptyDataTemplate>
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Label ID="StudentNameLbl" Text='<%# Eval("StudentName").ToString() %>'
                            runat="server" Width="250px" Font-Bold="False"></asp:Label>
                        <asp:Button ID="PresentBtn" runat="server" CausesValidation="false" CommandName="Present"
                            Text="Present" CommandArgument='<%# Eval("StudentId") %>' />
                        <asp:Button ID="LateBtn" runat="server" CausesValidation="false" CommandName="Late"
                            Text="Late" CommandArgument='<%# Eval("StudentId") %>' />
                        <asp:Button ID="AbsentBtn" runat="server" CausesValidation="false" CommandName="Absent"
                            Text="Absent" CommandArgument='<%# Eval("StudentId") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <br />
        <div id="NewStudentAttedanceDiv" runat="server">
            <strong>Student not on list above? </strong><span class="auto-style1">Enter name below. </span>
            <br />
            <asp:TextBox ID="NewStudentNameTbx" runat="server" Width="190px"></asp:TextBox>
            <asp:Button ID="NewStudentPresentBtn" runat="server" OnClick="NewStudentBtn_Click" Text="Present" CausesValidation="true" CommandName="Present" />
            <asp:Button ID="NewStudentLateBtn" runat="server" OnClick="NewStudentBtn_Click" Text="Late" CausesValidation="true" CommandName="Late" />
            <asp:Button ID="NewStudentAbsentBtn" runat="server" OnClick="NewStudentBtn_Click" Text="Absent" CausesValidation="true" CommandName="Absent" />
            <br />
            <asp:RequiredFieldValidator ID="NewStudentNameVld" runat="server" ControlToValidate="NewStudentNameTbx" Font-Bold="True" ForeColor="#CC0000">* Please enter a name for the current student!</asp:RequiredFieldValidator>
        </div>
        <p>
            Problem? 
            <span class="auto-style1">Contact Data Management [<% DataManagementLnk.NavigateUrl = "mailto:" + ConfigurationManager.AppSettings["dataManagementEmail"]; %>
                <asp:HyperLink ID="DataManagementLnk" NavigateUrl="mailto:" runat="server">
                    <%= ConfigurationManager.AppSettings["dataManagementEmail"].ToString() %></asp:HyperLink>] for assistance.
            </span>
        </p>
        <span class="heading4">Message Log</span><asp:GridView ID="MessageLogGridView" runat="server" AutoGenerateColumns="False" ForeColor="#666666" GridLines="None" ShowHeader="False">
            <Columns>
                <asp:BoundField DataField="Message">
                    <ItemStyle Font-Bold="False" />
                </asp:BoundField>
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>
