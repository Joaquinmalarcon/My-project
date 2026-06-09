using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Immersal.Samples.Navigation;

public class LLMNavigationManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Groq API")]
    [SerializeField] private string groqApiKey = "API_KEY";
    private const string GROQ_URL = "https://api.groq.com/openai/v1/chat/completions";
    private const string MODEL = "llama-3.1-8b-instant";

    private bool _isProcessing = false;

    void Start()
    {
        sendButton.onClick.AddListener(OnSend);
        chatInputField.onSubmit.AddListener(_ => OnSend());
        SetStatus("¿A dónde quieres ir?");
    }

    void OnSend()
    {
        if (_isProcessing) return;
        string query = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(query)) return;
        chatInputField.text = "";
        StartCoroutine(ProcessQuery(query));
    }

    IEnumerator ProcessQuery(string query)
    {
        _isProcessing = true;
        sendButton.interactable = false;
        SetStatus("Procesando...");

        // Obtener destinos desde la escena
        IsNavigationTarget[] targets = FindObjectsByType<IsNavigationTarget>(FindObjectsSortMode.None);
        var destList = new System.Text.StringBuilder();
        foreach (var t in targets)
            destList.AppendLine($"- {t.gameObject.name}");

        string systemPrompt = $@"Eres un asistente de navegación indoor universitaria.
Identifica a cuál destino quiere ir el usuario según su mensaje.

Destinos disponibles:
{destList}

Responde SOLO con JSON sin texto adicional:
Si encontraste el destino: {{""destination_id"": ""NOMBRE_EXACTO"", ""found"": true}}
Si no lo encontraste:      {{""destination_id"": null, ""found"": false}}

El destination_id debe ser EXACTAMENTE igual al nombre de la lista.";

        // Construir body de Groq (compatible con OpenAI)
        string body = $@"{{
            ""model"": ""{MODEL}"",
            ""messages"": [
                {{""role"": ""system"", ""content"": {JsonEscape(systemPrompt)}}},
                {{""role"": ""user"",   ""content"": {JsonEscape(query)}}}
            ],
            ""response_format"": {{""type"": ""json_object""}},
            ""temperature"": 0
        }}";

        using var www = new UnityEngine.Networking.UnityWebRequest(GROQ_URL, "POST");
        www.uploadHandler   = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", $"Bearer {groqApiKey}");

        yield return www.SendWebRequest();

        if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LLMNav] Error: {www.error}\n{www.downloadHandler.text}");
            SetStatus("Error de conexión.");
        }
        else
        {
            string raw = www.downloadHandler.text;
            string content = ExtractContent(raw);

            if (!string.IsNullOrEmpty(content))
            {
                var res = JsonUtility.FromJson<LLMNavResponse>(content);
                if (res.found && !string.IsNullOrEmpty(res.destination_id))
                {
                    SetStatus($"Navegando a {res.destination_id}...");
                    NavigationManager.Instance.NavigateToName(res.destination_id);
                }
                else
                {
                    SetStatus("No encontré ese destino. Intenta con el nombre de la sala.");
                }
            }
            else
            {
                SetStatus("Error procesando respuesta.");
            }
        }

        sendButton.interactable = true;
        _isProcessing = false;
    }

    // Extrae el content del JSON de Groq: choices[0].message.content
    string ExtractContent(string groqJson)
    {
        try
        {
            var resp = JsonUtility.FromJson<GroqResponse>(groqJson);
            if (resp?.choices != null && resp.choices.Length > 0)
                return resp.choices[0].message.content;
        }
        catch (Exception e) { Debug.LogError($"[LLMNav] Parse error: {e.Message}"); }
        return null;
    }

    string JsonEscape(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "") + "\"";

    void SetStatus(string msg) { if (statusText) statusText.text = msg; }
}

// Modelos de respuesta Groq
[Serializable] public class GroqResponse
{
    public GroqChoice[] choices;
}
[Serializable] public class GroqChoice
{
    public GroqMessage message;
}
[Serializable] public class GroqMessage
{
    public string content;
}
[Serializable] public class LLMNavResponse
{
    public string destination_id;
    public bool found;
}