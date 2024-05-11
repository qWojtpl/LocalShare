
using LocalShareApplication.Misc;
using LocalShareCommunication;

namespace LocalShareApplication;

public static class FileManager
{

    private static LocalShareClient _client = CommunicationManager.Client;
    private static LocalShareServer _server = CommunicationManager.Server;


    public static void OpenFilePicker()
    {
        Task.Run(async () =>
        {
            IEnumerable<FileResult> results = await FilePicker.Default.PickMultipleAsync();
            if (results == null)
            {
                return;
            }

            Shared.FilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LocalShare/files") + "/";

            foreach(FileResult fileResult in results)
            {
                _server.SendFile(fileResult.FullPath);
                Thread.Sleep(500);
            }
        });
    }

}
