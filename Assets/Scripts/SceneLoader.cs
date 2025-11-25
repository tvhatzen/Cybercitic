using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cybercitic.SceneManagement
{
    public class SceneLoader : SingletonBase<SceneLoader>
    {
        #region Enum, Variables
        private enum TransitionState
        {
            Idle,
            SlidingIn,
            LoadingScene,
            SlidingOut
        }

        [Header("Transition Animation")]
        [SerializeField] private Image transitionImage;
        [SerializeField] private float transitionDuration = 1.0f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Debug")]
        [SerializeField] private bool debug;

        public event Action<string> PlaySoundRequested;

        private TransitionState currentState = TransitionState.Idle;
        private SceneTransitionAnimator transitionAnimator;
        private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        #endregion

        protected override void Awake()
        {
            base.Awake();
            InitializeAnimator();
        }

        public void LoadSceneWithTransition(string sceneName)
        {
            if (!IsSceneLoadRequestValid(sceneName))
            {
                return;
            }

            if (currentState != TransitionState.Idle)
            {
                if (debug) Debug.LogWarning("[SceneLoader] Transition already in progress.");
                return;
            }

            StartCoroutine(TransitionAndLoadScene(sceneName));
        }

        public static void LoadScene(string sceneName) => Instance.LoadSceneWithTransition(sceneName);

        private void InitializeAnimator()
        {
            if (transitionImage == null)
            {
                if (debug) Debug.LogError("[SceneLoader] Transition image reference is missing.");
                return;
            }

            transitionAnimator = new SceneTransitionAnimator(transitionImage, transitionDuration, transitionCurve);
            if (!transitionAnimator.TryInitialize(debug, out var error))
            {
                if (debug) Debug.LogError(error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator TransitionAndLoadScene(string sceneName)
        {
            SetState(TransitionState.SlidingIn);
            yield return transitionAnimator.Slide(true, debug);

            SetState(TransitionState.LoadingScene);
            PlaySoundRequested?.Invoke("levelTransition");
            yield return LoadSceneRoutine(sceneName);

            SetState(TransitionState.SlidingOut);
            yield return transitionAnimator.Slide(false, debug);

            SetState(TransitionState.Idle);
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOperation == null)
            {
                if (debug) Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneName}'.");
                yield break;
            }

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            yield return waitForEndOfFrame;
            yield return waitForEndOfFrame;

            if (debug) Debug.Log($"[SceneLoader] Scene loaded. Current scene: {SceneManager.GetActiveScene().name}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private bool IsSceneLoadRequestValid(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                if (debug) Debug.LogError("[SceneLoader] Scene name cannot be null or empty.");
                return false;
            }

            if (!SceneExists(sceneName))
            {
                if (debug) Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not listed in build settings.");
                return false;
            }

            if (transitionAnimator == null || !transitionAnimator.IsInitialized)
            {
                if (debug) Debug.LogError("[SceneLoader] Transition animator is not initialized.");
                return false;
            }

            return true;
        }

        // 
        private static bool SceneExists(string sceneName)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetState(TransitionState newState)
        {
            currentState = newState;
            if (debug) Debug.Log($"[SceneLoader] Transition state changed to {currentState}");
        }
    }

    internal sealed class SceneTransitionAnimator
    {
        private readonly Image transitionImage;
        private readonly RectTransform transitionRectTransform;
        private readonly AnimationCurve transitionCurve;
        private readonly float transitionDuration;

        private Vector2 originalImageSize;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        internal SceneTransitionAnimator(Image image, float duration, AnimationCurve curve)
        {
            transitionImage = image;
            transitionRectTransform = image != null ? image.GetComponent<RectTransform>() : null;
            transitionCurve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            transitionDuration = Mathf.Max(0.01f, duration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal bool TryInitialize(bool debug, out string error)
        {
            if (transitionImage == null || transitionRectTransform == null)
            {
                error = "[SceneTransitionAnimator] Transition image or RectTransform is missing.";
                return false;
            }

            originalImageSize = DetermineOriginalImageSize();
            transitionRectTransform.sizeDelta = originalImageSize;

            transitionImage.gameObject.SetActive(true);
            transitionImage.enabled = true;

            transitionRectTransform.anchoredPosition = new Vector2(GetOffScreenLeft(), 0f);

            if (debug) Debug.Log($"[SceneTransitionAnimator] Initialized at size {originalImageSize}");           

            isInitialized = true;
            error = string.Empty;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slideIn"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        internal IEnumerator Slide(bool slideIn, bool debug)
        {
            if (!isInitialized)
            {
                yield break;
            }

            float elapsedTime = 0f;
            Vector2 startPosition = slideIn ? new Vector2(GetOffScreenLeft(), 0f) : Vector2.zero;
            Vector2 endPosition = slideIn ? Vector2.zero : new Vector2(GetOffScreenRight(), 0f);

            transitionRectTransform.anchoredPosition = startPosition;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / transitionDuration);
                float curveValue = transitionCurve.Evaluate(progress);
                transitionRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, curveValue);

                if (debug && Mathf.Abs((elapsedTime % 0.1f) - 0f) < Time.deltaTime)                
                    Debug.Log($"[SceneTransitionAnimator] Progress: {progress:F2}, Position: {transitionRectTransform.anchoredPosition}");                

                yield return null;
            }

            transitionRectTransform.anchoredPosition = endPosition;

            if (debug) Debug.Log($"[SceneTransitionAnimator] Slide {(slideIn ? "in" : "out")} completed.");
            
        }

        // 
        private Vector2 DetermineOriginalImageSize()
        {
            if (transitionImage.sprite != null)
            {
                return transitionImage.sprite.rect.size;
            }

            if (transitionImage.mainTexture != null)
            {
                return new Vector2(transitionImage.mainTexture.width, transitionImage.mainTexture.height);
            }

            return transitionRectTransform.sizeDelta;
        }

        private float GetOffScreenLeft() => -Screen.width - (originalImageSize.x * 0.5f);
        private float GetOffScreenRight() => Screen.width + (originalImageSize.x * 0.5f);
    }
}