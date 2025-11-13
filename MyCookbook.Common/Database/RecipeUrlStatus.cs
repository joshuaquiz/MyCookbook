namespace MyCookbook.Common.Database;

public enum RecipeUrlStatus
{
    NotStarted = 0,

    Downloading = 1,

    DownloadSucceeded = 2,

    DownloadFailed = 3,

    Parsing = 4,

    FinishedError = 5,

    FinishedSuccess = 6
}