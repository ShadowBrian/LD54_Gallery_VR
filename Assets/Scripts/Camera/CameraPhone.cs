using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CapturedImage
{
    public CapturedImage(Sprite sprite, List<Capturable> captured)
    {
        Sprite = sprite;
        UnstableCaptured = captured;
    }
    public readonly Sprite Sprite;
    public readonly List<Capturable> UnstableCaptured; // for deletion, store any unstable in photo
};

[RequireComponent(typeof(Animator))]
public class CameraPhone : MonoBehaviour
{
    private const float PhotoAspect = 1.0f; // width / height
    private const float PhotoHeightToScreen = 0.8f;
    private const int PhotoWidth = 256;
    
    [SerializeField] private Transform hand;
    [SerializeField] private Transform handZoomed;

    [SerializeField] private GameObject buttonIcon;
    [SerializeField] private Animator apertureAnimator;
    [SerializeField] private Image lastPic;
    [SerializeField] private List<Image> thumbnails = new();
    private int _selectedThumbnail = -1;
    [SerializeField] private GameObject cursor;

    public float zoomAlpha = 0;

    private Animator _animator;
    private int _zoomDirParam;
    private int _zoomDir = 1;

    private bool _screenshotQueued;

    public Camera _camera;
    [SerializeField] private LayerMask layerMask;

    private Rect _screenshotRect;
    private List<Capturable> _unstableCaptured = new();

    private int _storageSize = 2;
    private List<CapturedImage> _screenshots = new();
    [SerializeField] private GameObject storageWarning;

    private bool _triedDelete = false;
    [SerializeField] private GameObject deleteWarnImage;
    [SerializeField] private GameObject deleteWarnPanel;

    [SerializeField] private Sprite defaultLastPic;

    [SerializeField] private GameObject photosEmptyText;

    [SerializeField] private TextMeshProUGUI storageCounterText;
    [SerializeField] private Image storageBar;
    [SerializeField] private TextMeshProUGUI camStorageCounterText;
    [SerializeField] private Image camStorageBar;
    [SerializeField] private Color goodColor;
    [SerializeField] private Color badColor;

    [NonSerialized] public CollectionManager CollectionManager;
    
    private void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    
    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void Start()
    {
        //f_camera = Camera.main;
        _animator = GetComponent<Animator>();
        _zoomDirParam = Animator.StringToHash("ZoomDir");

        if (_storageSize > thumbnails.Count)
        {
            Debug.LogError("not enough image objects for storage size");
        }
        
        UpdateImages();
    }

    public void UpdateTransform()
    {
        transform.position = Vector3.Lerp(hand.position, handZoomed.position, zoomAlpha);
        transform.rotation = Quaternion.Lerp(hand.rotation, handZoomed.rotation, zoomAlpha);
    }

    public void ToggleZoomAnim()
    {
        if (_triedDelete)
        {
            _triedDelete = false;
            deleteWarnImage.SetActive(false);
            deleteWarnPanel.SetActive(false);
        }
        _animator.SetFloat(_zoomDirParam, _zoomDir);
        _animator.Play("PhoneZoom", 0, zoomAlpha);
        _zoomDir = -1 * _zoomDir;
    }

    public void TakePicture()
    {
        if (_screenshots.Count >= _storageSize)
        {
            return;
        }
        if (zoomAlpha >= 1)
        {
            apertureAnimator.Play("ApertureFlash", 0, 0);
            
            int width = (int)(PhotoHeightToScreen * PhotoAspect * Screen.height);
            int height = (int)(PhotoHeightToScreen * Screen.height);
            int x = (int)((Screen.width - width) / 2);
            int y = (int)((Screen.height - height) / 2);
            _screenshotRect = new Rect(x, y, width, height);
            _screenshotQueued = true;

            foreach (var obj in Capturable.Rendered)
            {
                var objectScreenPoint = Camera.main.WorldToScreenPoint(obj.transform.position);
                RaycastHit hit;
                bool captured = _screenshotRect.Contains(objectScreenPoint) && (!Physics.Linecast(Camera.main.transform.position, obj.transform.position, out hit, layerMask) || hit.collider == obj.Collider);
                if (captured)
                {
                    CollectionManager.AddPhoto(obj, _screenshots.Count);
                    if (obj.unstable)
                    {
                        _unstableCaptured.Add(obj); // for deletion. Used only at the end of this frame in render callback
                    }
                }
            }
        }
    }

    private void UpdateImages()
    {
        lastPic.sprite = _screenshots.Count > 0 ? _screenshots[^1].Sprite : defaultLastPic;
        for (int i = 0; i < thumbnails.Count; i++)
        {
            if (i < _screenshots.Count)
            {
                thumbnails[i].sprite = _screenshots[i].Sprite;
                thumbnails[i].gameObject.SetActive(true);
            }
            else
            {
                thumbnails[i].sprite = null;
                thumbnails[i].gameObject.SetActive(false);
            }
        }

        photosEmptyText.SetActive(_screenshots.Count == 0);
        storageWarning.SetActive(_screenshots.Count >= _storageSize);
        UpdateCursorPosition();

        storageCounterText.text = _screenshots.Count + "/" + _storageSize;
        camStorageCounterText.text = _screenshots.Count + "/" + _storageSize;
        float storageRatio = _screenshots.Count / (float)_storageSize;
        storageBar.fillAmount = storageRatio;
        storageBar.color = Color.Lerp(goodColor, badColor, storageRatio);
        camStorageBar.fillAmount = storageRatio;
        camStorageBar.color = Color.Lerp(goodColor, badColor, storageRatio);
    }

    private void UpdateCursorPosition()
    {
        if (_screenshots.Count == 0)
        {
            cursor.SetActive(false);
            _selectedThumbnail = -1;
        }
        else if (_selectedThumbnail == -1)
        {
            cursor.SetActive(true);
            _selectedThumbnail = 0;
            cursor.transform.position = thumbnails[_selectedThumbnail].transform.position;
        }
        else if (_selectedThumbnail >= _screenshots.Count)
        {
            cursor.SetActive(true);
            _selectedThumbnail = _screenshots.Count - 1;
            cursor.transform.position = thumbnails[_selectedThumbnail].transform.position;
        }
        else
        {
            cursor.SetActive(true);
            cursor.transform.position = thumbnails[_selectedThumbnail].transform.position;
        }
    }

    public void ToggleButtonIcon(bool pressed)
    {
        if (zoomAlpha >= 1)
        {
            buttonIcon.SetActive(pressed && _screenshots.Count < _storageSize);
        }
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam != _camera || !_screenshotQueued)
        {
            return;
        }

        _screenshotQueued = false;
        Texture2D screenshotTexture = new Texture2D(256, 256, TextureFormat.RGB24, false, true, true);
        _screenshotRect = new Rect(0, 0, 256, 256);
        screenshotTexture.ReadPixels(_screenshotRect, 0, 0);
        // Should resize texture here to save memory, but can't figure out easy way to do that
        screenshotTexture.Apply();
        Sprite screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height),
            new Vector2((float)screenshotTexture.width / 2, (float)screenshotTexture.height / 2));
        List<Capturable> _thisUnstableCaptured = new();
        foreach (var obj in _unstableCaptured)
        {
            _thisUnstableCaptured.Add(obj);
        }
        _screenshots.Add(new CapturedImage(screenshotSprite, _thisUnstableCaptured));
        _unstableCaptured.Clear();
        UpdateImages();
    }

    public void MoveCursor(bool prev)
    {
        if (_triedDelete)
        {
            _triedDelete = false;
            deleteWarnImage.SetActive(false);
            deleteWarnPanel.SetActive(false);
        }
        if (zoomAlpha <= 0)
        {
            if (prev && _selectedThumbnail > 0)
            {
                _selectedThumbnail -= 1;
                UpdateCursorPosition();
            }
            else if (!prev && _screenshots.Count > 0 && _selectedThumbnail < _screenshots.Count - 1)
            {
                _selectedThumbnail += 1;
                UpdateCursorPosition();
            }
        }
    }

    public void TryDeleteImage()
    {
        if (zoomAlpha > 0 || _screenshots.Count == 0)
        {
            return;
        }

        if (_triedDelete)
        {
            _triedDelete = false;
            deleteWarnImage.SetActive(false);
            deleteWarnPanel.SetActive(false);
            DeleteImage();
        }
        else
        {
            _triedDelete = true;
            deleteWarnImage.SetActive(true);
            var panelPos = deleteWarnPanel.transform.localPosition;
            deleteWarnPanel.transform.localPosition = new Vector3(panelPos.x, cursor.transform.localPosition.y, panelPos.z);
            deleteWarnPanel.SetActive(true);
        }
    }

    private void DeleteImage()
    {
        foreach (var obj in _screenshots[_selectedThumbnail].UnstableCaptured)
        {
            // "delete" the obj (but not really because we may respawn)
            obj.gameObject.SetActive(false);
            if (obj.endsGame)
            {
                EndGame();
            }
        }
        
        _screenshots.RemoveAt(_selectedThumbnail);
        
        if (_selectedThumbnail > _screenshots.Count)
        {
            _selectedThumbnail -= 1;
        }
        CollectionManager.DeletePhoto(_selectedThumbnail);
        UpdateImages();
        
    }

    public void SetStorageSize(int size)
    {
        _storageSize = size;

        int prevSelectedThumbnail = _selectedThumbnail;
        
        while (_screenshots.Count > size)
        {
            _selectedThumbnail = _screenshots.Count - 1;
            DeleteImage();
        }

        if (prevSelectedThumbnail < _screenshots.Count)
        {
            _selectedThumbnail = prevSelectedThumbnail;
        }
    }

    private void EndGame()
    {
        RestartManager.Restart();
    }
}
