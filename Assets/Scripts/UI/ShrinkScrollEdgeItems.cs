using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class ShrinkScrollEdgeItems : MonoBehaviour
{
    public enum Directions
    {
        Horizontal,
        Vertical
    }

    public Directions direction = Directions.Horizontal;
    public RectTransform frameRect;
    public RectTransform containerRect;
    public Animator leftArrowAnim;
    public Animator rightArrowAnim;

    public float edgeShrinkProximity = 100.0f;


    private ScollShrinkItem[] scrollItems;
    private Camera uiCamera;
    private Vector2 frameDimensions;
    private float leftLimit;
    private float rightLimit;
    private bool showLeftArrow;
    private bool showRightArrow;


    void Awake()
    {
        uiCamera = Camera.main.GetUICamera();
        Initialize();
        index = 0;
    }

    void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        leftLimit = 0;
        rightLimit = (direction == Directions.Horizontal ? frameRect.rect.width : frameRect.rect.height);

        // Update Tower list on enable to ensure all towers are listed properly and we aren't caching previously level's towers
        scrollItems = containerRect.GetComponentsInChildren<ScollShrinkItem>(true);
        frameDimensions = frameRect.rect.size;


    }

    void Update()
    {
        showLeftArrow = showRightArrow = false;

        for (int i = 0; i < scrollItems.Length; i++)
        {
            if (scrollItems[i] == null)
                continue;
            float scale = 0.5f;
            Vector3 relativePos = (containerRect.localPosition + scrollItems[i].rectTransform.localPosition - frameRect.localPosition);
            float localPos = (direction == Directions.Horizontal ? relativePos.x : relativePos.y);

            // Tidy up the percentage calc, preferably into a single line
            if (leftLimit + edgeShrinkProximity > localPos)
            {
                float diff = localPos - (leftLimit);
                float scope = (leftLimit + edgeShrinkProximity) - leftLimit;
                scale = Mathf.Clamp01(diff / scope - 0.5f);
                if (scale <= 0.1f)
                    showLeftArrow = true;
            }
            else if (rightLimit - edgeShrinkProximity < localPos)
            {
                float diff = localPos - (rightLimit);
                float scope = (rightLimit - edgeShrinkProximity) - (rightLimit);
                scale = Mathf.Clamp01((1 - diff / scope) + 0.5f);
                if (scale >= 0.9f)
                    showRightArrow = true;
            }
            scrollItems[i].SetScale(scale);
        }

        if (transform.parent.parent.gameObject.activeSelf&& leftArrowAnim&& interval!=0)
            index = (int)containerRect.localPosition.x / (-interval);

        // Disable arrows if items are off the left or right side of the list
        if (leftArrowAnim)
            leftArrowAnim.SetTrigger((showLeftArrow) ? "On" : "Off");
        if (rightArrowAnim)
            rightArrowAnim.SetTrigger((showRightArrow) ? "On" : "Off");
    }

    public int interval;
    private int index;

    public void ArrowClick(int i)
    {
        index += i;
        containerRect.DOLocalMove(new Vector3(-interval * index, 0, 0), 0.3f);
    }
}
