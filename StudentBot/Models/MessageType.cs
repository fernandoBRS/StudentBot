

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace StudentBot.Models
{
    public class MessageType
    {
        public MessageType(Activity activity, IConnectorClient connector)
        {
            MessageActivity = activity;
            Connector = connector;
            ReplyMessage = string.Empty;
            StudentId = string.Empty;
            SendOneReply = true;
        }

        public Activity MessageActivity { get; set; }
        public IConnectorClient Connector { get; set; }
        public string ReplyMessage { get; set; }
        public string StudentId { get; set; }
        public bool SendOneReply { get; set; }

        public async Task ValidateMessageAsync()
        {
            var stateClient = MessageActivity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(MessageActivity.ChannelId, MessageActivity.From.Id);
            var studentId = userData.GetProperty<string>(TagHelper.StudentId);
            
            if (IsGreetingMessage())
                ReplyMessage = GetGreetingReply();
            else if (isQuestionMessage() || studentId == TagHelper.StudentDefaultIdValue)
                ReplyMessage = await GetQuestionReply();
            else ReplyMessage = "Sorry, I didn't understand what you typed :(";
            //else ReplyMessage = MessageActivity.From.Name + " ID = " + MessageActivity.From.Id;
            //else ReplyMessage = string.Empty;

            if (!string.IsNullOrEmpty(ReplyMessage))
            {
                var reply = MessageActivity.CreateReply(ReplyMessage);
                await Connector.Conversations.ReplyToActivityAsync(reply);
            }
            //else
            //{
            //    var replyToConversation = MessageActivity.CreateReply("title");

            //    replyToConversation.Recipient = MessageActivity.From;
            //    replyToConversation.Type = "message";
            //    replyToConversation.Attachments = new List<Attachment>();

            //    var cardButtons = new List<CardAction>();

            //    var plButton = new CardAction() {
            //        Value = "https://<OAuthSignInURL>",
            //        Type = "signin",
            //        Title = "Connect"
            //    };

            //    cardButtons.Add(plButton);

            //    var plCard = new SigninCard("You need to authorize me", cardButtons);
            //    var plAttachment = plCard.ToAttachment();

            //    replyToConversation.Attachments.Add(plAttachment);

            //    await Connector.Conversations.SendToConversationAsync(replyToConversation);
            //}
        }

        #region Greeting message

        private bool IsGreetingMessage()
        {
            return new List<string> { "hello", "hi", "hey" }
            .Any(ContainsKeyword);
        }

        private static string GetGreetingReply()
        {
            return "Hi there! How can I help you?";
        }

        #endregion

        #region Question message

        private bool isQuestionMessage()
        {
            return new List<string>
            {
                "where", "what is", "what's", "whats",
                "what are", "what r", "do you", "do u",
                "when", "need information"
            }.Any(ContainsKeyword);
        }

        private async Task<string> GetQuestionReply()
        {
            if (ContainsKeyword("libraries") && !ContainsKeyword("art"))
                return "You can find libraries here: " + UrlHelper.LibrariesUrl;

            if (ContainsKeyword("library") || (ContainsKeyword("art") && ContainsKeyword("libraries")))
                return await GetLibraryLocationByType();

            if (ContainsKeyword("grade"))
                return GetGradeBySubject();

            if(ContainsKeyword("next class"))
                return await GetNextClass();

            if (ContainsKeyword("next event"))
                return await GetNextEvent();

            if (ContainsKeyword("career"))
                return GetCareersInfo();

            return "Sorry, I didn't find an answer :(";
        }

        #region Question - Libraries

        private async Task<string> GetLibraryLocationByType()
        {
            // TODO: Complete all USC libraries validation

            if (ContainsKeyword("accounting"))
                return GetReplyLibraryLocation(UrlHelper.AccountingLibraryTitle, UrlHelper.AccountingLibraryAddress);

            if(ContainsKeyword("art"))
                return await GetArtsLibrary();

            if (new List<string> { "arch", "fine" }.Any(ContainsKeyword))
                return GetReplyLibraryLocation(UrlHelper.ArchAndFineArtsLibraryTitle, UrlHelper.ArchAndFineArtsLibraryAddress);

            if (ContainsKeyword("cinema"))
                return GetReplyLibraryLocation(UrlHelper.CinematicArtsLibraryTitle, UrlHelper.CinematicArtsLibraryAddress);

            if (ContainsKeyword("law"))
                return GetReplyLibraryLocation(UrlHelper.LawLibraryTitle, UrlHelper.LawLibraryAddress);

            return "Sorry, I didn't find information about this library.";
        }
        
        private async Task<string> GetArtsLibrary()
        {
            if (new List<string> { "arch", "fine"}.Any(ContainsKeyword))
                return GetReplyLibraryLocation(UrlHelper.ArchAndFineArtsLibraryTitle, UrlHelper.ArchAndFineArtsLibraryAddress);

            if (ContainsKeyword("cinema"))
                return GetReplyLibraryLocation(UrlHelper.CinematicArtsLibraryTitle, UrlHelper.CinematicArtsLibraryAddress);

            // List of all types of Arts Libraries

            var replyArchAndFineMsg = "- " + GetReplyLibraryLocation(UrlHelper.ArchAndFineArtsLibraryTitle, UrlHelper.ArchAndFineArtsLibraryAddress);

            var replyCinematicArtsMsg = "- " + GetReplyLibraryLocation(UrlHelper.CinematicArtsLibraryTitle, UrlHelper.CinematicArtsLibraryAddress);

            await ReplyToActivityAsync(replyArchAndFineMsg);
            await ReplyToActivityAsync(replyCinematicArtsMsg);

            return string.Empty;
        }

        private static string GetReplyLibraryLocation(string libraryTitle, string libraryAddress, string libraryUrl = null)
        {
            var message = $"The {libraryTitle} is located at {libraryAddress}. ";

            return libraryUrl == null
                ? message
                : message + $"You can go to {libraryUrl} to see more information.";
        }

        #endregion

        #region Question - Grades

        private static string GetGradeBySubject()
        {
            return $"You can see your grades on {UrlHelper.GradesPortalTitle}. " +
                   $"For general information about grades, you can access {UrlHelper.GradesGeneralInfoUrl}.";
        }

        #endregion

        #region Question - Next Class

        private async Task<string> GetNextClass()
        {
            await GetStudentIdAsync();

            //if (StudentId == "-1")
            //{
            //    const string msg = "Please enter your Student ID";
            //    await ReplyToActivityAsync(msg);
            //}
            //else
            //{
            //    var msg = "Your ID is: " + StudentId;
            //    await ReplyToActivityAsync(msg);
            //}

            return "";
        }

        private async Task GetStudentIdAsync()
        {
            //var stateClient = MessageActivity.GetStateClient();
            //var userData = await stateClient.BotState.GetUserDataAsync(MessageActivity.ChannelId, MessageActivity.From.Id);

            await ValidateStudentIdReceivedAsync();

            //userData.SetProperty<string>(TagHelper.StudentId, "-1");
            //await stateClient.BotState.SetUserDataAsync(MessageActivity.ChannelId, MessageActivity.From.Id, userData);



            //var stateClient = MessageActivity.GetStateClient();
            //var userData = await stateClient.BotState.GetUserDataAsync(MessageActivity.ChannelId, MessageActivity.From.Id);
            //var studentIdFromState = userData.GetProperty<string>(TagHelper.StudentId);
            //string msg;

            //await ValidateStudentId(userData, studentIdFromState);

            //await stateClient.BotState.SetUserDataAsync(MessageActivity.ChannelId, MessageActivity.From.Id, userData);

            //msg = "Your ID is: " + StudentId;
            //await ReplyToActivityAsync(msg);
        }

        //private async Task ValidateStudentId(BotData userData, string studentIdFromState)
        //{
        //    if (string.IsNullOrEmpty(studentIdFromState))
        //    {
        //        userData.SetProperty<string>(TagHelper.StudentId, TagHelper.StudentDefaultIdValue);
        //        StudentId = TagHelper.StudentDefaultIdValue;
                
        //        await ReplyToActivityAsync("Please enter your Student ID.");
        //    }
        //    else
        //    {
        //        if (studentIdFromState == TagHelper.StudentDefaultIdValue)
        //        {
        //            userData.SetProperty<string>(TagHelper.StudentId, MessageActivity.Text);
        //            StudentId = MessageActivity.Text;
        //        }
        //        else
        //        {
        //            userData.SetProperty<string>(TagHelper.StudentId, studentIdFromState);
        //            StudentId = studentIdFromState;
        //        }

        //        await ValidateStudentIdReceivedAsync(userData);
        //    }
        //}

        //private async Task ValidateStudentIdReceivedAsync(BotData userData)
        //{
        //    int id;

        //    if (int.TryParse(StudentId, out id))
        //    {
        //        userData.SetProperty(TagHelper.StudentId, StudentId);
        //        StudentId = MessageActivity.Text;

        //        var schedule = GetStudentSchedule().Result;

        //        var message = schedule.Any() 
        //            ? $"Your {schedule["Course"]} class is {schedule["Weekday"]} at {schedule["StartTime"]} in {schedule["RoomAddress"]}." 
        //            : "There is no scheduled class yet.";

        //        await ReplyToActivityAsync(message);
        //    }
        //    else
        //    {
        //        await ReplyToActivityAsync("Your Student ID is invalid, please provide it again.");

        //        userData.SetProperty(TagHelper.StudentId, TagHelper.StudentDefaultIdValue);
        //        StudentId = TagHelper.StudentDefaultIdValue;
        //    }
        //}

        private async Task ValidateStudentIdReceivedAsync()
        {
            var schedule = await GetStudentSchedule();
            
            // Using **<text>** makes it bold
            var message = schedule.Any()
                ? $"Your {schedule["Course"]} **{schedule["Subject"]}** class is {schedule["Weekday"]} at {schedule["StartTime"]} in {schedule["RoomAddress"]}."
                : "There is no scheduled class yet.";

            await ReplyToActivityAsync(message);
        }

        private async Task<Dictionary<string, string>> GetStudentSchedule()
        {
            var con = new SqlConnection(TagHelper.ConnectionString);
            con.Open();
            
            var students = new Dictionary<string, string>();
            var currentWeekday = (int) DateTime.Today.DayOfWeek;
            var currentTime = DateTime.UtcNow.ToString("HH:mm");
            var weekdayCount = currentWeekday;
            
            //if (await HasScheduleValue(con) == false)
            //    return students;

            while (true)
            {
                var cmd = new SqlCommand
                {
                    //CommandText = "select st.Name as 'Student'," +
                    //          "c.Name as 'Course'," +
                    //          "subj.Name as 'Subject'," +
                    //          "sch.SubjectWeekday," +
                    //          "sch.StartTime," +
                    //          "subj.RoomAddress " +
                    //          "from Schedule sch " +
                    //          "inner join Student st on sch.StudentId = st.Id " +
                    //          "inner join Course c on sch.CourseId = c.Id " +
                    //          "inner join Subject subj on sch.SubjectId = subj.Id " +
                    //          $"where st.Id = {int.Parse(StudentId)} and sch.SubjectWeekday = {weekdayCount} ",
                    CommandText = "select st.Name as 'Student'," +
                              "c.Name as 'Course'," +
                              "subj.Name as 'Subject'," +
                              "sch.SubjectWeekday," +
                              "sch.StartTime," +
                              "subj.RoomAddress " +
                              "from Schedule sch " +
                              "inner join Student st on sch.StudentId = st.Id " +
                              "inner join Course c on sch.CourseId = c.Id " +
                              "inner join Subject subj on sch.SubjectId = subj.Id " +
                              $"where st.Id = {1} and sch.SubjectWeekday = {weekdayCount} ",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                cmd.CommandText += weekdayCount == currentWeekday
                    ? $"and sch.StartTime >= '{currentTime}' order by sch.StartTime"
                    : "order by sch.StartTime";
                
                var reader = cmd.ExecuteReader();

                if (reader.HasRows && reader.Read())
                {
                    FillStudentsDict(reader, students);

                    reader.Close();
                    con.Close();
                    return students;
                }

                reader.Close();

                // 6 = Saturday

                if (weekdayCount == 6)
                    weekdayCount = 0;
                else weekdayCount++;
            }
        }

        //private bool HasScheduleValue(SqlConnection con)
        //{
        //    var cmd = new SqlCommand
        //    {
        //        //CommandText = $"select * from Schedule where StudentId = {int.Parse(StudentId)}",
        //        CommandText = $"select * from Schedule where StudentId = {1}",
        //        CommandType = CommandType.Text,
        //        Connection = con
        //    };

        //    var reader = cmd.ExecuteReader();

        //    return reader.HasRows;
        //}

        private static void FillStudentsDict(SqlDataReader reader, IDictionary<string, string> students)
        {
            students.Add("Student", reader.GetString(0));
            students.Add("Course", reader.GetString(1));
            students.Add("Subject", reader.GetString(2));
            students.Add("Weekday", GetWeekdayName(reader.GetInt32(3)));
            students.Add("StartTime", GetStartTime(reader.GetTimeSpan(4)));
            students.Add("RoomAddress", reader.GetString(5));
        }

        private static string GetWeekdayName(int weekday)
        {
            var currentWeekday = (int)DateTime.Today.DayOfWeek;

            if (weekday == currentWeekday)
                return "today";

            if (weekday == currentWeekday + 1)
                return "tomorrow";

            return "on " + Enum.GetName(typeof (DayOfWeek), weekday);
        }

        private static string GetStartTime(TimeSpan timeSpan)
        {
            var time = DateTime.Today.Add(timeSpan);
            return time.ToString("hh:mm tt");
        }

        #endregion

        #region Questions - Next Event

        private async Task<string> GetNextEvent()
        {
            var eventData = await GetEventSchedule();

            // Using **<text>** makes it bold
            var message = eventData.Any()
                ? $"The event [{eventData["Title"]}]({eventData["Url"]}) will start in {eventData["EventDate"]} " +
                  $"at {eventData["StartTime"]} to {eventData["EndTime"]}."
                : "There is no event yet.";
            await ReplyToActivityAsync(message);

            return string.Empty;
        }

        private async Task<Dictionary<string, string>> GetEventSchedule()
        {
            var con = new SqlConnection(TagHelper.ConnectionString);
            con.Open();

            var eventData = new Dictionary<string, string>();
            var currentDate = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm");
            var dayCount = currentDate;

            //if (await HasScheduleValue(con) == false)
            //    return students;

            while (true)
            {
                var cmd = new SqlCommand
                {
                    CommandText = "select top 1 Title, EventUrl, EventDate, StartTime, EndTime " +
                                  "from Event where " +
                                  $"EventDate = '{dayCount.ToString("yyyy-MM-dd")}' ",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                cmd.CommandText += dayCount == currentDate
                    ? $"and StartTime >= '{currentTime}' order by StartTime"
                    : "order by StartTime";
                
                var reader = cmd.ExecuteReader();

                if (reader.HasRows && reader.Read())
                {
                    FillEventDict(reader, eventData);

                    reader.Close();
                    con.Close();
                    return eventData;
                }

                reader.Close();
                dayCount = dayCount.AddDays(1);
            }
        }

        private static void FillEventDict(SqlDataReader reader, IDictionary<string, string> eventData)
        {
            eventData.Add("Title", reader.GetString(0));
            eventData.Add("Url", reader.GetString(1));
            eventData.Add("EventDate", GetDateTimeFormat(reader.GetDateTime(2)));
            eventData.Add("StartTime", GetStartTime(reader.GetTimeSpan(3)));
            eventData.Add("EndTime", GetStartTime(reader.GetTimeSpan(4)));
        }

        private static string GetDateTimeFormat(DateTime dt)
        {
            return $"{dt.DayOfWeek}, {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dt.Month)} {dt.Day}";
        }

        #endregion

        #region Question - Careers

        private static string GetCareersInfo()
        {
            return $"You can find information about careers at USC on {UrlHelper.CareerTitle}.";
        }

        #endregion

        #endregion

        #region Shared methods
        private async Task ReplyToActivityAsync(string message)
        {
            var reply = MessageActivity.CreateReply(message);
            await Connector.Conversations.ReplyToActivityAsync(reply);
        }

        private bool ContainsKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return true;

            if (string.IsNullOrEmpty(MessageActivity.Text))
                return false;

            // case insensitive
            return MessageActivity.Text.IndexOf(keyword, 
                StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        #endregion
    }
}