
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Animations;
using TMPro;

public class AnimationInputController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationSetting
    {
        [Header("Animation Clip")]
        public AnimationClip animationClip;

        [Header("Keyboard")]
        public Key key = Key.None;
    }

    [Header("Character Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Settings")]
    [SerializeField]
    private List<AnimationSetting> animations = new();

    [Header("Animation UI")]
    [SerializeField] private TMP_Text animationText;

    [SerializeField]
    private string textPrefix = "Animation: ";

    private PlayableGraph playableGraph;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Text ban đầu
        if (animationText != null)
            animationText.text = textPrefix + "None";
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        foreach (AnimationSetting setting in animations)
        {
            if (setting.key == Key.None)
                continue;

            if (Keyboard.current[setting.key].wasPressedThisFrame)
            {
                PlayAnimation(setting.animationClip);
                break;
            }
        }
    }

    public void PlayAnimation(AnimationClip clip)
    {
        if (animator == null || clip == null)
        {
            Debug.LogWarning(
                "Thiếu Animator hoặc AnimationClip!"
            );
            return;
        }

        // Dừng animation cũ
        StopAnimation();

        // Tạo PlayableGraph
        playableGraph = PlayableGraph.Create(
            "AnimationGraph"
        );

        playableGraph.SetTimeUpdateMode(
            DirectorUpdateMode.GameTime
        );

        // Phát AnimationClip
        AnimationClipPlayable clipPlayable =
            AnimationClipPlayable.Create(
                playableGraph, clip
            );

        AnimationPlayableOutput output =
            AnimationPlayableOutput.Create(
                playableGraph,
                "AnimationOutput",
                animator
            );

        output.SetSourcePlayable(clipPlayable);

        // Chạy animation
        playableGraph.Play();

        // Cập nhật TextMeshPro
        UpdateAnimationText(clip.name);

        Debug.Log("Playing Animation: " + clip.name);
    }

    private void UpdateAnimationText(string animationName)
    {
        if (animationText != null)
        {
            animationText.text =
                textPrefix + animationName;
        }
    }

    public void StopAnimation()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }
}
