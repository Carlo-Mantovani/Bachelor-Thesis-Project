using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Threading.Tasks;
using SFB;
using TMPro;

public class TextManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI displayText;
    public Button submitButton;
    public Button uploadButton;
    public Button updateButton;
    public TMP_Dropdown optionDropdown;

    private string containerName;
    private string responseFilePath;
    private string dataFolderPath;
    private bool isProcessing = false; // Flag to track if a process is running

    void Start()
    {
        containerName = "rag-inference";
        responseFilePath = Application.dataPath + "/rag-docker/outputs/model_output.txt";
        dataFolderPath = Application.dataPath + "/rag-docker/data";

        if (File.Exists(responseFilePath))
        {
            File.Delete(responseFilePath);
        }

        submitButton.onClick.AddListener(() => _ = OnSubmit());
        uploadButton.onClick.AddListener(OnUploadDocument);
        updateButton.onClick.AddListener(() => _ = OnUpdateDatabase());

        UpdateText();
    }

    async Task OnSubmit()
    {
        if (isProcessing) // Check if a process is already running
        {
            ShowWarning("A process is already running. Please wait for it to finish before starting a new one.");
            return;
        }

        isProcessing = true; // Set the flag to indicate a process is running

        string inputText = inputField.text;
        string selectedOption = optionDropdown.options[optionDropdown.value].text;

        // Run the query in the background
        await Task.Run(() => RagQuery(inputText, selectedOption));

        // Periodically check for the response file and update text
        await WaitForResponse();

        isProcessing = false; // Reset the flag after the process is done
    }

    async Task OnUpdateDatabase()
    {
        if (isProcessing) // Check if a process is already running
        {
            ShowWarning("A process is already running. Please wait for it to finish before starting a new one.");
            return;
        }

        isProcessing = true; // Set the flag to indicate a process is running

        await Task.Run(() => RunPythonScript("populate_database.py"));
        
        isProcessing = false; // Reset the flag after the process is done
    }
    void OnUploadDocument()
    {
        string filePath = OpenFileDialog();
        if (!string.IsNullOrEmpty(filePath) && Path.GetExtension(filePath).ToLower() == ".pdf")
        {
            string destinationPath = Path.Combine(dataFolderPath, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, true);
            UnityEngine.Debug.Log("PDF uploaded successfully.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Invalid file or no file selected.");
        }
    }

    void ShowWarning(string message)
    {
        // Display a warning message (You can use a pop-up dialog or a UI element in your scene)
        UnityEngine.Debug.LogWarning(message);
        // Optionally, you could implement a UI dialog for user feedback.
    }

    async Task WaitForResponse()
    {
        while (!File.Exists(responseFilePath))
        {
            await Task.Delay(500); // Check every half second
        }
        UpdateText();
    }

    void UpdateText()
    {
        if (File.Exists(responseFilePath))
        {
            string content = File.ReadAllText(responseFilePath);
            displayText.text = content;
        }
        else
        {
            displayText.text = "Aguardando Pergunta";
        }
    }

    void RagQuery(string inputText, string selectedOption)
    {
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"exec {containerName} python /app/query_data.py \"{inputText}\" \"--illness\" \"{selectedOption}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process process = Process.Start(start))
        {
            process.WaitForExit();
            LogProcessOutput(process);
        }
    }

    void RunPythonScript(string scriptName)
    {
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"exec {containerName} python /app/{scriptName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process process = Process.Start(start))
        {
            process.WaitForExit();
            LogProcessOutput(process);
        }
    }

    void LogProcessOutput(Process process)
    {
        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            UnityEngine.Debug.Log(result);
        }
        using (StreamReader errorReader = process.StandardError)
        {
            string error = errorReader.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("Python error: " + error);
            }
        }
    }

    string OpenFileDialog()
    {
        var extensions = new[] {
            new ExtensionFilter("PDF Files", "pdf"),
            new ExtensionFilter("All Files", "*" ),
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select a file", "", extensions, false);
        return paths.Length > 0 ? paths[0] : string.Empty;
    }
}
