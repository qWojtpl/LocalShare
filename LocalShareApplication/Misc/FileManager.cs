
using LocalShareApplication.Misc;
using LocalShareCommunication;

namespace LocalShareApplication;

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

            Shared.FilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LocalShare/files") + "/";

            CommunicationManager.Client.GetHashCode();
            foreach(FileResult fileResult in results)
            {
                CommunicationManager.Server.SendFile(fileResult.FullPath);
            }
        });
    }

}
