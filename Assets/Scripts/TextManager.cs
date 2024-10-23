using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using UnityEngine.Events;
using SFB;
using TMPro;

public class TextManager : MonoBehaviour
{
    public InputField inputField;
    public TextMeshProUGUI displayText;
    public Button submitButton;
    public Button uploadButton;
    public Button updateButton;
    public Dropdown optionDropdown; // Dropdown menu for options

    private string pythonScriptPath;
    private string responseFilePath;
    private string dataFolderPath;


    void Start()
    {
        // Paths to the Python script and response text file
        pythonScriptPath = Application.dataPath + "/RAG/Scripts/query_data.py";
        responseFilePath = Application.dataPath + "/RAG/Scripts/model_output.txt";
        dataFolderPath = Application.dataPath + "/RAG/Scripts/data";

        // Ensure the response file is deleted if it exists
        if (File.Exists(responseFilePath))
        {
            File.Delete(responseFilePath);
        }

        // Set up the button listeners
        submitButton.onClick.AddListener(OnSubmit);
        uploadButton.onClick.AddListener(OnUploadDocument);
        updateButton.onClick.AddListener(OnUpdateDatabase);

        // Initialize the text display
        UpdateText();
    }

    void OnSubmit()
    {
        string inputText = inputField.text;
        string selectedOption = optionDropdown.options[optionDropdown.value].text; // Get selected option text

        // Run the Python script
        RagQuery(inputText, selectedOption);

        // Update the text display after running the script
        UpdateText();
    }

    void OnUploadDocument()
    {
        string filePath = OpenFileDialog(); // Open file explorer and get the path of the selected file
        if (!string.IsNullOrEmpty(filePath) && Path.GetExtension(filePath).ToLower() == ".pdf")
        {
            string destinationPath = Path.Combine(dataFolderPath, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, true); // Copy the PDF file to the data folder
            UnityEngine.Debug.Log("PDF uploaded successfully.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Invalid file or no file selected.");
        }
    }

    void OnUpdateDatabase()
    {
        RunPythonScript("populate_database.py"); // Run the Python script to update the database
    }

    void UpdateText()
    {
        if (File.Exists(responseFilePath))
        {
            string content = File.ReadAllText(responseFilePath); // Read the content of the response file
            displayText.text = content; // Display the content in the text box
        }
        else
        {
            displayText.text = "Aguardando Pergunta"; // Display a default message if file doesn't exist
        }
    }


    void RagQuery(string inputText, string selectedOption)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.WorkingDirectory = Application.dataPath + "/RAG/Scripts"; // Set the working directory
        start.FileName = "python"; // Use "python" or full path to python.exe
        start.Arguments = $"\"{pythonScriptPath}\" \"{inputText}\" \"--illness\" \"{selectedOption}\""; // Pass arguments to the Python script
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true; // Also capture errors

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log(result); // Log the Python script's output (if any)
            }
            using (StreamReader errorReader = process.StandardError)
            {
                string error = errorReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError("Python error: " + error); // Log any errors from the script
                }
            }
        }
    }

    void RunPythonScript(string scriptName)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.WorkingDirectory = Application.dataPath + "/RAG/Scripts"; // Set the working directory
        start.FileName = "python3"; // Use "python3" or full path to python3 executable
        start.Arguments = scriptName; // Run the specified Python script
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true; // Also capture errors

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log(result); // Log the Python script's output (if any)
            }
            using (StreamReader errorReader = process.StandardError)
            {
                string error = errorReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError("Python error: " + error); // Log any errors from the script
                }
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
        if (paths.Length > 0)
        {
            return paths[0]; // Return the first selected file path
        }
        return string.Empty;
    }
}
