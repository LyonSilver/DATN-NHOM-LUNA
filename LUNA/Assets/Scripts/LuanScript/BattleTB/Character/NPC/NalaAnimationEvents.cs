
using UnityEngine;

public class NalaAnimationEvents : MonoBehaviour
{
    [Header("Ultimate Controller")]
    [SerializeField]
    private NalaUltimateController ultimateController;

    public void OnUltimateEffect()
    {
        if (ultimateController != null)
            ultimateController.OnUltimateEffect();
    }

    public void OnUltimateFinished()
    {
        if (ultimateController != null)
            ultimateController.OnUltimateFinished();
    }
}