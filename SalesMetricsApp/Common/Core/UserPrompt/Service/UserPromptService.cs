using SalesMetricsApp.Common.Controller;
using SalesMetricsApp.Common.Core.UserPrompt.Model.Enum;
using SalesMetricsApp.Common.Utility;
using System.Data;

namespace SalesMetricsApp.Common.Core.UserPrompt.Service
{
    public class UserPromptService : IUserPromptService
    {
        // TODO: If class grows too big, make Validation modular, have its own service and each validation different component/rule.
        // Considering NuGet package is another option.
        private readonly Router _router;

        private string _invalidMessage = string.Empty; 

        public UserPromptService(Router router) 
        {
            _router = router; 
        }

        public T Prompt<T>(PromptType promptType, string message, List<string>? menuItems = null)
        {
            string? response;
            bool validInput = true;
            do
            {
                response = Execute(validInput, message, menuItems);

                // If commands received.
                if (_router.HandleCommand(response))
                    return default;

                // Otherwise, validate and return user response.
                validInput = Validate(promptType, response, menuItems?.Count);

            } while (!validInput);
                     
            return CastType<T>(promptType, response);
        }

        private string? Execute(bool validInput, string message, List<string>? menuItems = null)
        {
            if (!validInput)
            {
                CliDisplay.ShowErrorMessage(_invalidMessage);
                _invalidMessage = string.Empty;
            }
            CliDisplay.ShowRequest(message, menuItems);
            var response = Console.ReadLine();
            CliDisplay.ShowLine();

            return response;
        }

        private bool Validate(PromptType promptType, string? response, int? menuSize)
        {
            if (string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response))
            {
                _invalidMessage = "Invalid Input. Input should not be empty.";
                return false;
            }

            response = response.Trim().ToLower();

            switch (promptType)
            {
                case PromptType.Menu:
                    return ValidateMenuAction(response, menuSize);
                case PromptType.YesNo:
                    return ValidateYesNo(response);
                case PromptType.Date:
                    return ValidateDate(response);
                case PromptType.DateRange:
                    return ValidateDateRange(response);
                case PromptType.ImportPath:
                case PromptType.ExportPath:
                    return ValidatePath(response, promptType.Equals(PromptType.ImportPath)? true : false);
                case PromptType.DataSize:
                    return ValidateDataSize(response);
                case PromptType.FileExt:
                    return ValidateFileExt(response);
                default:
                    return false;
            }
        }

        private bool ValidateMenuAction(string menuAction, int? menuSize)
        {
            if (int.TryParse(menuAction, out int index))
            {
                if (index >= 1 && index <= menuSize)
                    return true;
            }

            _invalidMessage = "Invalid Input. Type a Menu Number.";
            return false;
        }

        private bool ValidateYesNo(string yesNo)
        {
            if (yesNo.Equals("yes") || yesNo.Equals("no")) 
            {
                return true;
            }

            _invalidMessage = "Invalid Input. Type 'yes' or 'no'.";

            return false;
        }

        private bool ValidateDate (string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))     
                return true;

            _invalidMessage = "Invalid Input. Provide valid Date Format.";
            return false;
        }

        private bool ValidateDateRange(string dateRangeString)
        {
            var dates = dateRangeString.Split("to", StringSplitOptions.TrimEntries);

            if (dates.Length != 2)
            {
                _invalidMessage = "Invalid Input. Provide 2 Dates separeted with 'to' (example: date1 to date2).";
                return false;
            }

            bool isValidStartDate = DateTime.TryParse(dates[0], out DateTime date1);
            bool isValidEndDate = DateTime.TryParse(dates[1], out DateTime date2);

            if (!isValidStartDate || !isValidEndDate)
            {
                _invalidMessage = "Invalid Input. Provide valid Date Format for both Dates.";
                return false;
            }
            else if (date1 > date2)
            {
                _invalidMessage = "Invalid Input. First date should not be later than second one.";
                return false;
            }

            return true;
        }

        private bool ValidatePath(string path, bool isImport)
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                _invalidMessage = "Invalid Input. Path contains Invalid Characters.";
                return false;
            }

            try
            {
                path = Path.GetFullPath(path);
                if (!Directory.Exists(path))
                {
                    _invalidMessage = "Invalid Input. Directory is Not Found.";
                    return false;
                }

                if (isImport && Directory.GetFiles(path).Length <= 0)
                {
                    _invalidMessage = "Invalid Input. Directory is empty.";
                    return false;
                }
            }
            catch (Exception e)
            {
                _invalidMessage = "Invalid Input. Path cannot be processed.";
                return false;
            }     

            return true;
        }

        private bool ValidateDataSize(string dataSize)
        {
            var dimensions = dataSize.Split(",", StringSplitOptions.TrimEntries);

            if (dimensions.Length != 2)
            {
                _invalidMessage = "Invalid Input. Provide 2 numbers separeted with ',' (example: noOfFiles - noOfEntriesPerFile).";
                return false;
            }

            bool validNoOfFiles = int.TryParse(dimensions[0], out int noOfFiles);
            bool validNoOfEntries = int.TryParse(dimensions[1], out int noOfEntries);

            if (!validNoOfFiles || !validNoOfEntries)
            {
                _invalidMessage = "Invalid Input. Provide valid Integer Numbers for both Number.";
                return false;
            }
            else if (noOfFiles <= 0 || noOfEntries <= 0)
            {
                _invalidMessage = "Invalid Input. Provide valid Positive Number for both Numbers.";
                return false;
            }

            return true;
        }

        private bool ValidateFileExt(string ext)
        {
            if (!ext.StartsWith('.'))
            {
                _invalidMessage = "Invalid Input. The Extension must start with '.'";
                return false;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (ext.IndexOfAny(invalidChars) >= 0)
            {
                _invalidMessage = "Invalid Input. The Extension contains invalid characters.";
                return false;
            }
            return true;
        }

        private T CastType<T>(PromptType type, string value)
        {
            switch (type)
            {
                case PromptType.Date:
                    DateTime.TryParse(value, out DateTime date);
                    return (T)(object)date;
                case PromptType.YesNo:
                    bool yesNo = value.Equals("yes") ? true : false;
                    return (T)(object)yesNo;
                case PromptType.DateRange:
                    var dates = value.Split("to", StringSplitOptions.TrimEntries).Select(DateTime.Parse).ToArray();
                    return (T)(object)dates;
                case PromptType.Menu:
                    int.TryParse(value, out int index);
                    return (T)(object)index;
                case PromptType.ImportPath:
                case PromptType.ExportPath:
                    var path = Path.GetFullPath(value);
                    return (T)(object)path;
                case PromptType.DataSize:
                    var dataSize = value.Split(",", StringSplitOptions.TrimEntries).Select(int.Parse).ToArray();
                    return (T)(object)dataSize;
                case PromptType.FileExt:
                    var ext = value.Trim();
                    return (T)(object)ext;
                default:
                    return default;
            }
        }
    }
}
