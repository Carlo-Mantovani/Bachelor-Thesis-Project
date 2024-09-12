using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class TextManager : MonoBehaviour
{
    public InputField inputField;
    public Text displayText;
    public Button submitButton;
    public Dropdown optionDropdown; // Dropdown menu for options

    private string pythonScriptPath;
    private string responseFilePath;

    void Start()
    {
        // Paths to the Python script and response text file
        pythonScriptPath = Application.dataPath + "/RAG/Scripts/query_data.py";
        responseFilePath = Application.dataPath + "/RAG/Scripts/model_output.txt";

        if (File.Exists(responseFilePath))
        {
            File.Delete(responseFilePath);
        }
        submitButton.onClick.AddListener(OnSubmit);
        UpdateText();
    }

    void OnSubmit()
    {
        string inputText = inputField.text;
        string selectedOption = optionDropdown.options[optionDropdown.value].text; // Get selected option text

        RunPythonScript(inputText, selectedOption); // Pass both the input text and selected option to the script
        UpdateText(); // Then update the display with the content of the response file
    }

    void UpdateText()
    {
        if (File.Exists(responseFilePath))
        {
            displayText.text = File.ReadAllText(responseFilePath); // Display the content of the response.txt file
        }
        else
        {
            displayText.text = "Aguardando Pergunta"; // Handle cases where the file doesn't exist
        }
    }

    void RunPythonScript(string inputText, string selectedOption)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.WorkingDirectory = Application.dataPath + "/RAG/Scripts";
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
}

