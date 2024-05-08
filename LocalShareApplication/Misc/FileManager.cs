
using Microsoft.Extensions.Logging;

namespace LocalShareApplication.Misc;

public static class FileManager
{

    public static void OpenFilePicker()
    {
        Task.Run(async () =>
        {
            IEnumerable<FileResult> results = await FilePicker.Default.PickMultipleAsync();
            if (results == null)
            {
                return;
            }
            foreach(FileResult fileResult in results)
            {
                CommunicationManager.Server.SendFile(fileResult.FullPath);
            }
        });
    }

}
