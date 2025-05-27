using UnityEngine;
using TMPro;
using Neocortex.Data;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Video;

namespace Neocortex.Samples
{
    public class LlamaParagraphGenerator : MonoBehaviour
    {
        [Header("Text Generation")]
        [SerializeField] private TMP_Text outputText;
        [SerializeField] private Button generateButton;
        [SerializeField] private OllamaModelDropdown modelDropdown;
        [SerializeField, TextArea] private string systemPrompt;

        [Header("Video Playback")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject videoScreenPanel;
        [SerializeField] private VideoClip defaultVideoClip;

        private OllamaRequest request;
        private string originalText;
        private int hoveredWordIndex = -1;
        private int selectedWordIndex = -1;
        private Camera uiCamera;
        private string[] currentConceptWords;
        private Dictionary<string, VideoClip> conceptToClip = new();
        private bool isVideoPlaying = false;

        private void Start()
        {
            request = new OllamaRequest();
            request.OnChatResponseReceived += OnChatResponseReceived;
            request.ModelName = "llama2:latest";

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                request.AddSystemMessage(systemPrompt);
            }

            generateButton.onClick.AddListener(OnGenerateButtonClicked);

            Canvas canvas = outputText.canvas;
            uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

            InitializeVideoPlayer();
        }

        private void InitializeVideoPlayer()
        {
            if (videoPlayer == null)
            {
                videoPlayer = FindObjectOfType<VideoPlayer>();
            }

            if (videoScreenPanel != null)
            {
                videoScreenPanel.SetActive(false);
            }

            videoPlayer.loopPointReached += OnVideoEnded;
        }

        private void OnGenerateButtonClicked()
        {
            var (subjectName, concepts) = BackendManager.GetRandomSubjectAndConcepts();
            currentConceptWords = concepts;
            LoadConceptVideos(concepts);

            string formattedPrompt =
                $"Write three short and simple sentences for a school subject called \"{subjectName}\". " +
                $"Each sentence must include exactly one of the following words: \"{concepts[0]}\", \"{concepts[1]}\", and \"{concepts[2]}\". " +
                $"Each sentence should be no more than 15 words. " +
                $"The sentences should be clear and child-friendly. " +
                $"Do not repeat any concept word or 'word'. " +
                $"Do NOT include any title, introduction, greeting, or explanation. " +
                $"Only output the three sentences as one paragraph, separated by periods. " +
                $"Do not explain anything. Just output the paragraph.";

            outputText.text = $"Learning about {subjectName}...";
            request.Send(formattedPrompt.Trim());
        }

        private void LoadConceptVideos(string[] words)
        {
            conceptToClip.Clear();

            foreach (var word in words)
            {
                string cleanedWord = word.ToLower();
                var clip = Resources.Load<VideoClip>($"aslVideos/{cleanedWord}") ?? defaultVideoClip;
                conceptToClip[cleanedWord] = clip;

                if (clip == null)
                {
                    Debug.LogWarning($"No video found for concept: {cleanedWord}");
                }
            }
        }

        private void OnChatResponseReceived(ChatResponse response)
        {
            originalText = CleanResponse(response.message);
            outputText.text = originalText;
        }

        private string CleanResponse(string raw)
        {
            var sentences = raw.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => s.Length > 0 && s.Split(' ').Length >= 3)
                                .Take(3)
                                .ToList();

            if (sentences.Count < 3)
                return raw.Trim();

            return string.Join(". ", sentences) + ".";
        }

        private void Update()
        {
            HandleWordSelection();
        }

        private void HandleWordSelection()
        {
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(outputText, Input.mousePosition, uiCamera);

            if (wordIndex != hoveredWordIndex && wordIndex != selectedWordIndex)
            {
                hoveredWordIndex = wordIndex;
                UpdateHighlightedText();
            }

            if (Input.GetMouseButtonDown(0) && wordIndex != -1)
            {
                selectedWordIndex = wordIndex;
                UpdateHighlightedText();

                string clickedWord = GetWordAtIndex(wordIndex);
                if (!string.IsNullOrEmpty(clickedWord))
                {
                    HandleVideoPlayback(clickedWord);
                }
            }
        }

        private void HandleVideoPlayback(string word)
        {
            string cleaned = word.Trim(new[] { '.', ',', ';', ':', '?', '!' }).ToLower();
            bool isConceptWord = currentConceptWords.Any(c => c.ToLower() == cleaned);

            if (isConceptWord && conceptToClip.TryGetValue(cleaned, out var clip) && clip != null)
            {
                PlayVideo(clip);
            }
            else
            {
                HideVideo(); // Hide if it's a non-concept word
                Debug.Log(isConceptWord ? $"No video for: {cleaned}" : $"Clicked non-concept word: {cleaned}");
            }
        }

        private void PlayVideo(VideoClip clip)
        {
            if (videoScreenPanel != null)
            {
                videoScreenPanel.SetActive(true); // ✅ Enable panel before playing
            }

            videoPlayer.Stop();
            videoPlayer.clip = clip;
            videoPlayer.Play();
            isVideoPlaying = true;
            Debug.Log($"Playing concept video for: {clip.name}");

        }

        private void OnVideoEnded(VideoPlayer vp)
        {
            // Do nothing, video remains visible until a non-concept word is clicked
        }

        private void HideVideo()
        {
            if (videoScreenPanel != null)
            {
                videoScreenPanel.SetActive(false);
            }
            isVideoPlaying = false;
            videoPlayer.Stop();
        }

        private void UpdateHighlightedText()
        {
            if (string.IsNullOrEmpty(originalText) || outputText.textInfo == null) return;

            TMP_TextInfo textInfo = outputText.textInfo;
            if (hoveredWordIndex < 0 || hoveredWordIndex >= textInfo.wordCount) return;

            TMP_WordInfo wordInfo = textInfo.wordInfo[hoveredWordIndex];
            string word = originalText.Substring(wordInfo.firstCharacterIndex, wordInfo.characterCount);
            string beforeWord = originalText.Substring(0, wordInfo.firstCharacterIndex);
            string afterWord = originalText.Substring(wordInfo.firstCharacterIndex + wordInfo.characterCount);

            string colorHex = "#FFFF00"; // yellow by default

            if (hoveredWordIndex == selectedWordIndex)
            {
                string clickedWord = word.Trim(new[] { '.', ',', ';', ':', '?', '!' }).ToLower();
                bool isMatch = currentConceptWords.Any(c => c.ToLower() == clickedWord);
                colorHex = isMatch ? "#00FF00" : "#FF0000"; // green if concept, red if not
            }

            outputText.text = beforeWord + $"<mark={colorHex}>{word}</mark>" + afterWord;
        }

        private string GetWordAtIndex(int index)
        {
            if (index < 0 || outputText.textInfo == null || index >= outputText.textInfo.wordCount)
                return null;

            TMP_WordInfo wordInfo = outputText.textInfo.wordInfo[index];
            return originalText.Substring(wordInfo.firstCharacterIndex, wordInfo.characterCount);
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEnded;
            }
        }
    }
}
