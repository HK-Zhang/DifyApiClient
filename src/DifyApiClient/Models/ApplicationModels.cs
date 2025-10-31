using System.Text.Json.Serialization;

namespace DifyApiClient.Models;

/// <summary>
/// Application basic information
/// </summary>
public class ApplicationInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("author_name")]
    public string? AuthorName { get; set; }
}

/// <summary>
/// Application parameters information
/// </summary>
public class ApplicationParameters
{
    [JsonPropertyName("opening_statement")]
    public string? OpeningStatement { get; set; }

    [JsonPropertyName("suggested_questions")]
    public List<string>? SuggestedQuestions { get; set; }

    [JsonPropertyName("suggested_questions_after_answer")]
    public SuggestedQuestionsAfterAnswer? SuggestedQuestionsAfterAnswer { get; set; }

    [JsonPropertyName("speech_to_text")]
    public FeatureConfig? SpeechToText { get; set; }

    [JsonPropertyName("text_to_speech")]
    public TextToSpeechConfig? TextToSpeech { get; set; }

    [JsonPropertyName("retriever_resource")]
    public FeatureConfig? RetrieverResource { get; set; }

    [JsonPropertyName("annotation_reply")]
    public FeatureConfig? AnnotationReply { get; set; }

    [JsonPropertyName("user_input_form")]
    public List<UserInputFormItem>? UserInputForm { get; set; }

    [JsonPropertyName("file_upload")]
    public FileUploadConfig? FileUpload { get; set; }

    [JsonPropertyName("system_parameters")]
    public SystemParameters? SystemParameters { get; set; }
}

public class SuggestedQuestionsAfterAnswer
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class FeatureConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class TextToSpeechConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("autoPlay")]
    public string? AutoPlay { get; set; }
}

public class UserInputFormItem
{
    [JsonPropertyName("text-input")]
    public InputControl? TextInput { get; set; }

    [JsonPropertyName("paragraph")]
    public InputControl? Paragraph { get; set; }

    [JsonPropertyName("select")]
    public SelectControl? Select { get; set; }
}

public class InputControl
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("variable")]
    public string? Variable { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("default")]
    public string? Default { get; set; }
}

public class SelectControl : InputControl
{
    [JsonPropertyName("options")]
    public List<string>? Options { get; set; }
}

public class FileUploadConfig
{
    [JsonPropertyName("image")]
    public FileTypeConfig? Image { get; set; }

    [JsonPropertyName("document")]
    public FileTypeConfig? Document { get; set; }

    [JsonPropertyName("audio")]
    public FileTypeConfig? Audio { get; set; }

    [JsonPropertyName("video")]
    public FileTypeConfig? Video { get; set; }

    [JsonPropertyName("custom")]
    public FileTypeConfig? Custom { get; set; }
}

public class FileTypeConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("number_limits")]
    public int NumberLimits { get; set; }

    [JsonPropertyName("transfer_methods")]
    public List<string>? TransferMethods { get; set; }
}

public class SystemParameters
{
    [JsonPropertyName("file_size_limit")]
    public int FileSizeLimit { get; set; }

    [JsonPropertyName("image_file_size_limit")]
    public int ImageFileSizeLimit { get; set; }

    [JsonPropertyName("audio_file_size_limit")]
    public int AudioFileSizeLimit { get; set; }

    [JsonPropertyName("video_file_size_limit")]
    public int VideoFileSizeLimit { get; set; }
}

/// <summary>
/// Application meta information
/// </summary>
public class ApplicationMeta
{
    [JsonPropertyName("tool_icons")]
    public Dictionary<string, object>? ToolIcons { get; set; }
}

/// <summary>
/// Application WebApp settings
/// </summary>
public class ApplicationSite
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("chat_color_theme")]
    public string? ChatColorTheme { get; set; }

    [JsonPropertyName("chat_color_theme_inverted")]
    public bool ChatColorThemeInverted { get; set; }

    [JsonPropertyName("icon_type")]
    public string? IconType { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("icon_background")]
    public string? IconBackground { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("copyright")]
    public string? Copyright { get; set; }

    [JsonPropertyName("privacy_policy")]
    public string? PrivacyPolicy { get; set; }

    [JsonPropertyName("custom_disclaimer")]
    public string? CustomDisclaimer { get; set; }

    [JsonPropertyName("default_language")]
    public string? DefaultLanguage { get; set; }

    [JsonPropertyName("show_workflow_steps")]
    public bool ShowWorkflowSteps { get; set; }

    [JsonPropertyName("use_icon_as_answer_icon")]
    public bool UseIconAsAnswerIcon { get; set; }
}
