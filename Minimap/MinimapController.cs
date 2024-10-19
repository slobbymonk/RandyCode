using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private GameObject _smallMinimap, _bigMinimap;
    [SerializeField] private DirectionIndicatorManager _indicatorManager;

    private bool minimapIsActive = false;

    private void Update()
    {
        HandleInputCheck();
    }

    void OpenMinimap()
    {
        _smallMinimap.SetActive(false);
        _bigMinimap.SetActive(true);
        _indicatorManager.SetFullscreen(true); // Switch indicator manager to fullscreen mode
    }

    void CloseMinimap()
    {
        _smallMinimap.SetActive(true);
        _bigMinimap.SetActive(false);
        _indicatorManager.SetFullscreen(false); // Switch indicator manager back to small minimap mode
    }

    private void HandleInputCheck()
    {
        if (InputHandler.GetInput._minimapIsActive)
        {
            if (!minimapIsActive)
            {
                OpenMinimap();
                minimapIsActive = true;
            }
        }
        else
        {
            if (minimapIsActive)
            {
                CloseMinimap();
                minimapIsActive = false;
            }
        }
    }
}
