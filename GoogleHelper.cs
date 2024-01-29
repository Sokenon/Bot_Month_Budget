using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot_Month_Budget
{
    class GoogleHelper
    {
        private static GoogleHelper instance = null;
        private readonly string Token;
        private readonly string GoogleToken;
        private readonly string GoogleSheet;
        private UserCredential credentials;
        private DriveService driveService;
        private SheetsService sheetsService;
        private string sheetFileID;
        private int? sheetID;
        private string sheetName;
        public string Month => sheetName;
        private int? simpleID;
        private int week;
        public int Week => week;
        private string[] Diaposons = new string[] { "C4:C46", "E4:E46", "G4:G46", "I4:I46" };
        private string[] Columns = new string[] { ":C", ":E", ":G", ":I" };
        private string[] CategoryColumn = new string[] { "B", "D", "F", "H" };
        private string category;
        public string Category => category;

        private GoogleHelper(string token, string table)
        {
            this.Token = token;
            this.GoogleSheet = table;
        }
        public static GoogleHelper GetInstance(string token, string table)
        {
            if (instance == null)
            {
                instance = new GoogleHelper(token, table);
                return instance;
            }
            else
            {
                return instance;
            }
        }

        public string ApplicationName { get; private set; } = "Budget";
        public string[] Scopes { get; private set; } = new string[] { DriveService.Scope.Drive, SheetsService.Scope.Spreadsheets };
        internal async Task<bool> RefreshToken()
        {


            if (await this.credentials.RefreshTokenAsync(CancellationToken.None))
            {
                // Успешно обновлены учетные данные
            }

            return true;
        }

        internal async Task<bool> Start()
        {
            string credentialPath = Path.Combine(Environment.CurrentDirectory, ".credentials", ApplicationName);
            using (var strm = new MemoryStream(Encoding.UTF8.GetBytes(this.Token)))
            {
                this.credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets: GoogleClientSecrets.FromStream(strm).Secrets,
                    scopes: this.Scopes,
                    user: "user",
                    taskCancellationToken: CancellationToken.None,
                    new FileDataStore(credentialPath, true));
            }
            // ApiKey = "AIzaSyDYhsgn_dNNdV9LVMGfvhsFVAsxSKyYh7M

            this.driveService = new DriveService(new Google.Apis.Services.BaseClientService.Initializer { ApplicationName = ApplicationName, HttpClientInitializer = this.credentials });
            //this.driveService = new DriveService(new Google.Apis.Services.BaseClientService.Initializer { HttpClientInitializer = this.credentials, ApplicationName = ApplicationName });
            this.sheetsService = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer { ApplicationName = ApplicationName, HttpClientInitializer = this.credentials });


            var request = this.driveService.Files.List();
            var response = request.Execute();

            foreach (var file in response.Files)
            {
                if (file.Name == this.GoogleSheet)
                {
                    this.sheetFileID = file.Id;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(this.sheetFileID))
            {
                var sheetRequest = this.sheetsService.Spreadsheets.Get(this.sheetFileID);
                var sheetResponse = sheetRequest.Execute();
                this.sheetID = sheetResponse.Sheets[0].Properties.SheetId;
                this.sheetName = sheetResponse.Sheets[0].Properties.Title;
                int id = sheetResponse.Sheets.Count - 1;
                this.simpleID = sheetResponse.Sheets[id].Properties.SheetId;

                await Task.Delay(3000);

                var range = this.sheetName + "!C4:I4";
                var Request = this.sheetsService.Spreadsheets.Values.Get(
                    spreadsheetId: this.sheetFileID,
                    range: range
                    );
                var Response = Request.Execute();
                switch (Response.Values.First().Count)
                {
                    case 1:
                        this.week = 0;
                        break;
                    case 3:
                        this.week = 1;
                        break;
                    case 5:
                        this.week = 2;
                        break;
                    case 7:
                        this.week = 3;
                        break;
                    default:
                        this.week = 0;
                        break;
                }
                return true;
            }
            return false;
        }

        internal string Set(object value)
        {
            string cell = Get();
            if (cell == "end")
            {
                return "В этой неделе кончились ячейки";
            }
            else
            {
                var range = this.sheetName + "!" + cell;
                var values = new List<List<object>> { new List<object> { this.category, value } };
                var request = this.sheetsService.Spreadsheets.Values.Update(
                    new Google.Apis.Sheets.v4.Data.ValueRange { Values = new List<IList<object>>(values) },
                    spreadsheetId: this.sheetFileID,
                    range: range
                    );
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var response = request.Execute();
                return "";
            }
        }

        internal string Get()
        {
            string result = "";
            try
            {
                var range = this.sheetName + "!" + this.Diaposons[this.week];
                var request = this.sheetsService.Spreadsheets.Values.Get(
                    spreadsheetId: this.sheetFileID,
                    range: range
                    );
                var response = request.Execute();
                if (response.Values == null)
                {
                    result = (this.CategoryColumn[this.week] + "4" + this.Columns[this.week] + "4");
                }
                else
                {
                    if (response.Values.Count == 46)
                    {
                        return "end";
                    }
                    else
                    {
                        result = (this.CategoryColumn[this.week] + (response.Values.Count + 4).ToString() + this.Columns[this.week] + (response.Values.Count + 4).ToString());
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

            return result;
        }
        public void NewCategory(string name)
        {
            this.category = name;
        }
        public int NextWeek()
        {
            if (week < 3)
            {
                this.week = this.week + 1;
                return this.week + 1;
            }
            else
            {
                return 0;
            }
        }
        public void NextMonth()
        {
            try
            {
                string[] months = new string[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
                string newMonth = "";
                for (int i = 0; i < months.Length; i++)
                {
                    if (months[i] == this.sheetName)
                    {
                        if (months.Length - 1 == i)
                        {
                            newMonth = months[0];
                            break;
                        }
                        else
                        {
                            newMonth = months[i + 1];
                            break;
                        }
                    }
                }
                string spreadsheetId = this.sheetFileID;
                int? sheetIdToDuplicate = this.simpleID;

                var duplicateRequest = new DuplicateSheetRequest();

                duplicateRequest.NewSheetName = newMonth;
                duplicateRequest.SourceSheetId = sheetIdToDuplicate;

                var request = new Request { DuplicateSheet = duplicateRequest };

                BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetReq = new BatchUpdateSpreadsheetRequest();
                batchUpdateSpreadsheetReq.Requests = new List<Google.Apis.Sheets.v4.Data.Request> { request };

                SpreadsheetsResource.BatchUpdateRequest req = this.sheetsService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetReq, spreadsheetId);
                req.Execute();
                var sheetRequest = this.sheetsService.Spreadsheets.Get(this.sheetFileID);
                var sheetResponse = sheetRequest.Execute();
                this.sheetID = sheetResponse.Sheets[0].Properties.SheetId;
                this.sheetName = sheetResponse.Sheets[0].Properties.Title;
                this.week = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public string Balance()
        {
            string result = "";
            var range = this.sheetName + "!" + this.Columns[this.week].Substring(1) + "3";
            var request = this.sheetsService.Spreadsheets.Values.Get(
                spreadsheetId: this.sheetFileID,
                range: range
                );
            var response = request.Execute();
            result = response.Values.First().First().ToString();
            return result;
        }
    }
}
