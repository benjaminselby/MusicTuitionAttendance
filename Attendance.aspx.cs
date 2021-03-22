using System;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using ApplicationCode;
using System.Data;
using System.IO;

namespace MusicTuitionAttendance
{
    public partial class Attendance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                // If a debug user has been set in the Web.Config file, use that network login. 
                // Otherwise get the current user's network login from their current Windows session. 
                string currentUserNetworkLogin =
                    (ConfigurationManager.AppSettings["debugUser"] is null
                        || ConfigurationManager.AppSettings["debugUser"] == "")
                    ? User.Identity.Name.Substring(ConfigurationManager.AppSettings["usernamePrefix"].Length)
                    : currentUserNetworkLogin = ConfigurationManager.AppSettings["debugUser"];

                User currentUser = new User(currentUserNetworkLogin);

                if (currentUser == null)
                {
                    string errorMessage = String.Format(
                        "User information could not be obtained from Synergy for tutor " +
                        "with Network Login = {0}",
                        currentUserNetworkLogin);

                    ErrorHandler.HandleError(errorMessage, true);
                    Response.Redirect("./Error.aspx");
                }
                else
                {
                    Session["currentUser"] = currentUser;
                }

                lblTutorUsername.Text = currentUser.networkLogin;
                lblTutorName.Text = currentUser.fullName;

                PopulateStudentList();
                PopulateMessageLog();
            }
        }


        private void PopulateStudentList()
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["SynergyOneConnectionString"].ConnectionString;

                    using (SqlCommand studentListCmd = new SqlCommand(
                            ConfigurationManager.AppSettings["studentsForStaffProc"],
                            synergyOneConnection))
                    {
                        studentListCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        string currentUserId = (Session["currentUser"] as User).id;
                        studentListCmd.Parameters.AddWithValue("StaffId", currentUserId);

                        using (SqlDataAdapter studentsAdapter = new SqlDataAdapter(studentListCmd))
                        {
                            synergyOneConnection.Open();

                            DataSet students = new DataSet();
                            studentsAdapter.Fill(students);

                            if(students.Tables[0].Rows.Count == 0)
                            {
                                NewStudentAttedanceDiv.Visible = false;
                            }

                            StudentsGridView.DataSource = students;
                            StudentsGridView.DataBind();

                            Session["studentList"] = students;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }


        private void PopulateMessageLog()
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["SynergyOneConnectionString"].ConnectionString;
                    
                    using (SqlCommand messageLogCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["attendanceLogForStaffTodayProc"],
                        synergyOneConnection))
                    {
                        messageLogCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        string currentUserId = (Session["currentUser"] as User).id;
                        messageLogCommand.Parameters.AddWithValue("StaffId", currentUserId);

                        DataSet messageLog = new DataSet();
                        using (SqlDataAdapter messageLogAdapter = new SqlDataAdapter())
                        {
                            messageLogAdapter.SelectCommand = messageLogCommand;
                            synergyOneConnection.Open();
                            messageLogAdapter.Fill(messageLog);
                        };
                        MessageLogGridView.DataSource = messageLog;
                        MessageLogGridView.DataBind();

                        Session["messageLog"] = messageLog;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }


        private void WriteLogMessage(string type, string message)
        {
            SqlConnection synergyOneConnection = new SqlConnection();
            try
            {
                synergyOneConnection.ConnectionString =
                    ConfigurationManager.ConnectionStrings["SynergyOneConnectionString"].ConnectionString;

                using (SqlCommand messageLogCmd = new SqlCommand(
                    ConfigurationManager.AppSettings["attendanceLogAddMessageProc"], synergyOneConnection))
                {
                    messageLogCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    string currentUserId = (Session["currentUser"] as User).id;
                    messageLogCmd.Parameters.AddWithValue("StaffId", currentUserId);
                    messageLogCmd.Parameters.AddWithValue("Type", type);
                    messageLogCmd.Parameters.AddWithValue("Message", message);

                    synergyOneConnection.Open();

                    int rowsUpdated = messageLogCmd.ExecuteNonQuery();
                    // TODO: Confirm rows updated == 1 here? 
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(ex.Message, true);
                Response.Redirect("./Error.aspx");
                //throw (ex);
            }
            finally
            {
                synergyOneConnection.Close();
            }
        }


        protected void StudentsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Present" || e.CommandName == "Late" || e.CommandName == "Absent")
            {
                string studentId = e.CommandArgument.ToString();
                string currentUserId = (Session["currentUser"] as User).id;
                RecordAttendance(currentUserId, studentId, e.CommandName.ToUpper());
            }
        }


        protected void RecordAttendance(string staffId, string studentId, string status)
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                string currentUserId = (Session["currentUser"] as User).id;
                string currentUserFullName = (Session["currentUser"] as User).fullName;

                string studentName;
                if (studentId == null)
                {
                    studentName = NewStudentNameTbx.Text;
                }
                else
                {
                    DataTable students = (Session["studentList"] as DataSet).Tables[0];
                    DataRow[] foundRows = students.Select("StudentId = " + studentId);

                    if(foundRows.Length != 1)
                    {
                        throw new Exception(String.Format(
                            "Non-unique or non-matching key value ID:{0} in STUDENTS data table query.",
                            studentId));
                    }

                    studentName = foundRows[0][1].ToString();
                }

                studentId = studentId ?? "UNKNOWN";

                string attendanceMessage = String.Format(
                    "{0} - {1} [ID:{2}] marked as {3} for music tuition class with {4} [ID:{5}].",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    studentName,
                    studentId,
                    status.ToUpper(),
                    currentUserFullName,
                    currentUserId);

                NotifyStudentServices(attendanceMessage);

                // Log the attendance event in a database table. 
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["SynergyOneConnectionString"].ConnectionString;

                    using (SqlCommand messageLogCmd = new SqlCommand(
                        ConfigurationManager.AppSettings["attendanceLogAddEventProc"], synergyOneConnection))
                    {
                        messageLogCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        messageLogCmd.Parameters.AddWithValue("StaffId", staffId);
                        messageLogCmd.Parameters.AddWithValue("StudentId", studentId);
                        messageLogCmd.Parameters.AddWithValue("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageLogCmd.Parameters.AddWithValue("Status", status);

                        synergyOneConnection.Open();

                        int rowsUpdated = messageLogCmd.ExecuteNonQuery();
                        // TODO: Confirm rows updated == 1 here? 
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }

        }


        protected void NewStudentBtn_Click(object sender, EventArgs e)
        {
            /* It is hoped that this can be avoided as much as possible because we have no way 
             * of knowing the student ID etc. Music tutors are unreliable and often don't provide 
             * us with good information regarding their students, so we need a provision for students 
             * who the tutor has accepted but we have not been notified of. */
            string senderAddress, recipientAddress, currentUserId, currentUserEmail, currentUserFullName;

            currentUserId = (Session["currentUser"] as User).id;
            currentUserEmail = (Session["currentUser"] as User).emailAddress;
            currentUserFullName = (Session["currentUser"] as User).fullName;

            RecordAttendance(currentUserId, null, ((Button)sender).CommandName.ToUpper());

            // Notify Data Management that a new student has been marked as attending music tuition.

            senderAddress = (currentUserEmail == null || currentUserEmail == "") ?
                ConfigurationManager.AppSettings["dataManagementEmail"] :
                currentUserEmail;
            recipientAddress = ConfigurationManager.AppSettings["dataManagementEmail"];
            string attendanceMessage = String.Format(
                "{0} - {1} [ID:UNKNOWN] marked as {2} for music tuition class with {3} [ID:{4}].",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                NewStudentNameTbx.Text,
                ((Button)sender).CommandName.ToUpper(),
                currentUserFullName,
                currentUserId);

            MailHandler.SendMail(
                senderAddress,
                recipientAddress,
                "New student added to Music Tuition Attendance system.",
                attendanceMessage);
        }

        private void NotifyStudentServices(string message)
        {
            string currentUserEmail = (Session["currentUser"] as User).emailAddress;

            // Send notification email. 
            MailHandler.SendMail(
                currentUserEmail,
                ConfigurationManager.AppSettings["studentServicesEmailAddresses"],
                "Music Tuition Attendance Notification",
                message);

            // Write log message and refresh the contents of the message log page element. 
            WriteLogMessage("EMAIL", message);
            PopulateMessageLog();
        }
    }
}
