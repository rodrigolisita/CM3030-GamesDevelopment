using UnityEngine;
using System.Collections.Generic;

public class ImageLoopBackground : MonoBehaviour
{
    [Header("背景图片设置")]
    public Sprite[] backgroundImages;                 // 背景图片数组
    public float switchInterval = 2f;                 // 图片切换间隔（秒）
    
    [Header("移动设置")]
    public float moveSpeed = 1f;                      // 移动速度
    [Range(0f, 1f)]
    public float parallaxMultiplier = 0.8f;          // 视差效果倍数
    
    [Header("移动方向")]
    public bool moveHorizontally = false;             // 水平移动
    public bool moveVertically = true;                // 垂直移动
    
    [Header("循环设置")]
    public bool loopBackground = true;                // 是否循环背景
    public int backgroundCopies = 2;                  // 背景副本数量
    public bool hideStaticBackgrounds = true;         // 隐藏静态背景
    
    [Header("手动位置设置")]
    [Tooltip("手动设置主背景的起始Y位置。如果设为0，将使用当前Transform位置")]
    public float manualStartPositionY = 0f;           // 手动设置主背景起始Y位置
    [Tooltip("手动设置主背景的起始X位置。如果设为0，将使用当前Transform位置")]
    public float manualStartPositionX = 0f;           // 手动设置主背景起始X位置
    [Tooltip("手动设置背景副本的Y偏移量。如果设为0，将自动计算；否则使用此值")]
    public float manualCopyOffsetY = 0f;              // 手动设置副本Y偏移
    [Tooltip("手动设置背景副本的X偏移量。如果设为0，将自动计算；否则使用此值")]
    public float manualCopyOffsetX = 0f;              // 手动设置副本X偏移
    [Tooltip("手动设置循环触发的Y位置。如果设为0，将使用自动计算的位置")]
    public float manualTriggerPositionY = 0f;         // 手动设置循环触发Y位置
    [Tooltip("手动设置循环触发的X位置。如果设为0，将使用自动计算的位置")]
    public float manualTriggerPositionX = 0f;         // 手动设置循环触发X位置
    
    [Header("游戏状态控制")]
    public bool pauseOnGameOver = true;               // 游戏结束时暂停
    
    [Header("调试信息")]
    public bool showDebugInfo = false;                // 显示调试信息
    
    private Vector3 startPosition;
    private float backgroundWidth;
    private float backgroundHeight;
    private SpriteRenderer spriteRenderer;
    private GameManager2D gameManager;
    private bool isPaused = false;
    private int currentImageIndex = 0;
    private float imageSwitchTimer = 0f;
    private List<GameObject> backgroundCopiesList;
    
    void Start()
    {
        InitializeImageLoopBackground();
    }
    
    void InitializeImageLoopBackground()
    {
        // 获取游戏管理器
        gameManager = GameManager2D.Instance;
        
        // 记录初始位置，使用手动设置或当前Transform位置
        if (manualStartPositionX != 0f || manualStartPositionY != 0f)
        {
            startPosition = new Vector3(
                manualStartPositionX != 0f ? manualStartPositionX : transform.position.x,
                manualStartPositionY != 0f ? manualStartPositionY : transform.position.y,
                transform.position.z
            );
            
            // 立即设置到手动位置
            transform.position = startPosition;
            
            if (showDebugInfo)
            {
                Debug.Log($"使用手动起始位置: {startPosition}");
            }
        }
        else
        {
            startPosition = transform.position;
            
            if (showDebugInfo)
            {
                Debug.Log($"使用当前Transform位置: {startPosition}");
            }
        }
        
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ImageLoopBackground: SpriteRenderer组件未找到!", this);
            return;
        }
        
        // 检查是否有背景图片
        if (backgroundImages == null || backgroundImages.Length == 0)
        {
            Debug.LogWarning("ImageLoopBackground: 没有设置背景图片!", this);
            return;
        }
        
        // 设置第一张图片
        SetCurrentImage(0);
        
        // 计算背景尺寸
        CalculateBackgroundSize();
        
        // 创建背景副本（如果需要循环）
        if (loopBackground)
        {
            CreateBackgroundCopies();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"图片循环背景初始化完成: {gameObject.name}, 图片数量: {backgroundImages.Length}");
        }
    }
    
    void SetCurrentImage(int index)
    {
        if (backgroundImages != null && index >= 0 && index < backgroundImages.Length)
        {
            currentImageIndex = index;
            spriteRenderer.sprite = backgroundImages[index];
            
            if (showDebugInfo)
            {
                Debug.Log($"切换到图片 {index}: {backgroundImages[index].name}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"无法设置图片索引 {index}: 图片数组={backgroundImages?.Length ?? 0}");
            }
        }
    }
    
    void CalculateBackgroundSize()
    {
        backgroundWidth = spriteRenderer.bounds.size.x;
        backgroundHeight = spriteRenderer.bounds.size.y;
        
        if (showDebugInfo)
        {
            Debug.Log($"背景尺寸: 宽度={backgroundWidth}, 高度={backgroundHeight}, 精灵名称: {spriteRenderer.sprite?.name}");
        }
    }
    
    void CreateBackgroundCopies()
    {
        backgroundCopiesList = new List<GameObject>();
        
        for (int i = 1; i < backgroundCopies; i++)
        {
            Vector3 offset = Vector3.zero;
            
            if (moveVertically)
            {
                // 使用手动设置的Y偏移，如果没有设置则使用自动计算
                if (manualCopyOffsetY != 0f)
                {
                    offset = Vector3.up * manualCopyOffsetY;
                }
                else
                {
                    // 确保副本直接放在主背景的上方，没有间隙
                    offset = Vector3.up * backgroundHeight * i;
                }
            }
            else if (moveHorizontally)
            {
                // 使用手动设置的X偏移，如果没有设置则使用自动计算
                if (manualCopyOffsetX != 0f)
                {
                    offset = Vector3.right * manualCopyOffsetX;
                }
                else
                {
                    offset = Vector3.right * backgroundWidth * i;
                }
            }
            
            GameObject backgroundCopy = Instantiate(gameObject, 
                startPosition + offset, 
                transform.rotation, 
                transform.parent);
            
            // 重命名副本
            backgroundCopy.name = $"{gameObject.name}_Copy_{i}";
            
            // 移除ImageLoopBackground脚本以避免重复创建
            Destroy(backgroundCopy.GetComponent<ImageLoopBackground>());
            
            // 设置相同的图片
            SpriteRenderer copyRenderer = backgroundCopy.GetComponent<SpriteRenderer>();
            if (copyRenderer != null)
            {
                copyRenderer.sprite = backgroundImages[currentImageIndex];
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"创建背景副本: {backgroundCopy.name}, 位置: {backgroundCopy.transform.position}, 偏移: {offset}, 背景高度: {backgroundHeight}");
            }
            
            backgroundCopiesList.Add(backgroundCopy);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"总共创建了 {backgroundCopiesList.Count} 个背景副本");
        }
    }
    
    void Update()
    {
        // 检查是否需要暂停
        if (pauseOnGameOver && gameManager != null)
        {
            isPaused = !(GameManager2D.Instance.gameState == GameState.Active);
        }
        
        if (isPaused) return;
        
        // 更新图片切换
        UpdateImageSwitching();
        
        // 计算移动距离
        float actualSpeed = moveSpeed * parallaxMultiplier;
        float moveDistance = actualSpeed * Time.deltaTime;
        
        // 应用移动
        ApplyMovement(moveDistance);
        
        // 处理循环
        if (loopBackground)
        {
            HandleLooping();
        }
        
        // 定期检查背景位置（调试用）
        if (showDebugInfo && Time.frameCount % 300 == 0) // 每5秒检查一次
        {
            DebugBackgroundPositions();
        }
    }
    
    void UpdateImageSwitching()
    {
        if (backgroundImages == null || backgroundImages.Length <= 1) 
        {
            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log($"图片切换检查: 图片数量={backgroundImages?.Length ?? 0}");
            }
            return;
        }
        
        imageSwitchTimer += Time.deltaTime;
        
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"图片切换计时器: {imageSwitchTimer:F1}/{switchInterval:F1}, 当前图片: {currentImageIndex}");
        }
        
        if (imageSwitchTimer >= switchInterval)
        {
            imageSwitchTimer = 0f;
            
            // 切换到下一张图片
            int nextIndex = (currentImageIndex + 1) % backgroundImages.Length;
            SetCurrentImage(nextIndex);
            
            // 更新所有副本的图片
            UpdateAllCopiesImage();
            
            if (showDebugInfo)
            {
                Debug.Log($"图片切换完成: {currentImageIndex} -> {nextIndex}, 图片名称: {backgroundImages[nextIndex].name}");
            }
        }
    }
    
    void UpdateAllCopiesImage()
    {
        if (backgroundCopiesList != null)
        {
            foreach (GameObject copy in backgroundCopiesList)
            {
                if (copy != null)
                {
                    SpriteRenderer copyRenderer = copy.GetComponent<SpriteRenderer>();
                    if (copyRenderer != null)
                    {
                        copyRenderer.sprite = backgroundImages[currentImageIndex];
                    }
                }
            }
        }
    }
    
    void ApplyMovement(float moveDistance)
    {
        Vector3 newPosition = transform.position;
        
        // 垂直移动（向下移动，模拟向上飞行）
        if (moveVertically)
        {
            newPosition.y -= moveDistance;
        }
        
        // 水平移动（向左移动，模拟向右飞行）
        if (moveHorizontally)
        {
            newPosition.x -= moveDistance;
        }
        
        transform.position = newPosition;
        
        // 同时移动所有背景副本
        if (backgroundCopiesList != null)
        {
            foreach (GameObject copy in backgroundCopiesList)
            {
                if (copy != null)
                {
                    Vector3 copyPosition = copy.transform.position;
                    
                    if (moveVertically)
                    {
                        copyPosition.y -= moveDistance;
                    }
                    
                    if (moveHorizontally)
                    {
                        copyPosition.x -= moveDistance;
                    }
                    
                    copy.transform.position = copyPosition;
                }
            }
        }
        
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"背景移动: {gameObject.name}, 位置: {newPosition}");
        }
    }
    
    void HandleLooping()
    {
        // 垂直循环
        if (moveVertically)
        {
            // 计算触发位置
            float triggerY = manualTriggerPositionY != 0f ? manualTriggerPositionY : startPosition.y - backgroundHeight;
            
            if (transform.position.y <= triggerY)
            {
                transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
                
                if (showDebugInfo)
                {
                    Debug.Log($"垂直循环重置: {gameObject.name}, 触发位置: {triggerY}, 重置位置: {transform.position}");
                }
            }
            
            // 处理背景副本的循环
            if (backgroundCopiesList != null)
            {
                foreach (GameObject copy in backgroundCopiesList)
                {
                    if (copy != null)
                    {
                        if (copy.transform.position.y <= triggerY)
                        {
                            // 重置到主背景的上方，使用手动偏移量
                            float resetOffset = manualCopyOffsetY != 0f ? manualCopyOffsetY : backgroundHeight;
                            copy.transform.position = new Vector3(copy.transform.position.x, startPosition.y + resetOffset, copy.transform.position.z);
                            
                            if (showDebugInfo)
                            {
                                Debug.Log($"背景副本循环重置: {copy.name}, 触发位置: {triggerY}, 新位置: {copy.transform.position}, 偏移量: {resetOffset}");
                            }
                        }
                    }
                }
            }
            
            // 隐藏太高的背景（看起来静态的）
            if (hideStaticBackgrounds)
            {
                float hideThreshold = startPosition.y + backgroundHeight * 0.5f;
                if (transform.position.y > hideThreshold)
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                    }
                }
                else
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = true;
                    }
                }
                
                // 处理背景副本可见性
                if (backgroundCopiesList != null)
                {
                    foreach (GameObject copy in backgroundCopiesList)
                    {
                        if (copy != null)
                        {
                            SpriteRenderer copyRenderer = copy.GetComponent<SpriteRenderer>();
                            if (copyRenderer != null)
                            {
                                if (copy.transform.position.y > hideThreshold)
                                {
                                    copyRenderer.enabled = false;
                                }
                                else
                                {
                                    copyRenderer.enabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // 水平循环
        if (moveHorizontally)
        {
            // 计算水平触发位置
            float triggerX = manualTriggerPositionX != 0f ? manualTriggerPositionX : startPosition.x - backgroundWidth;
            
            if (transform.position.x <= triggerX)
            {
                transform.position = new Vector3(startPosition.x, transform.position.y, transform.position.z);
                
                if (showDebugInfo)
                {
                    Debug.Log($"水平循环重置: {gameObject.name}, 触发位置: {triggerX}, 重置位置: {transform.position}");
                }
            }
            
            // 处理背景副本的水平循环
            if (backgroundCopiesList != null)
            {
                foreach (GameObject copy in backgroundCopiesList)
                {
                    if (copy != null)
                    {
                        if (copy.transform.position.x <= startPosition.x - backgroundWidth)
                        {
                            copy.transform.position = new Vector3(startPosition.x, copy.transform.position.y, copy.transform.position.z);
                        }
                    }
                }
            }
        }
    }
    
    // 公共方法：设置移动速度
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (showDebugInfo)
        {
            Debug.Log($"移动速度更新: {gameObject.name}, 新速度: {moveSpeed * parallaxMultiplier}");
        }
    }
    
    // 公共方法：设置视差倍数
    public void SetParallaxMultiplier(float multiplier)
    {
        parallaxMultiplier = Mathf.Clamp01(multiplier);
        if (showDebugInfo)
        {
            Debug.Log($"视差倍数更新: {gameObject.name}, 新倍数: {parallaxMultiplier}");
        }
    }
    
    // 公共方法：设置图片切换间隔
    public void SetSwitchInterval(float interval)
    {
        switchInterval = Mathf.Max(0.1f, interval);
        if (showDebugInfo)
        {
            Debug.Log($"图片切换间隔更新: {gameObject.name}, 新间隔: {switchInterval}");
        }
    }
    
    // 公共方法：添加背景图片
    public void AddBackgroundImage(Sprite image)
    {
        if (image != null)
        {
            List<Sprite> imageList = new List<Sprite>(backgroundImages ?? new Sprite[0]);
            imageList.Add(image);
            backgroundImages = imageList.ToArray();
            
            if (showDebugInfo)
            {
                Debug.Log($"添加背景图片: {image.name}, 总数量: {backgroundImages.Length}");
            }
        }
    }
    
    // 公共方法：清除所有图片
    public void ClearAllImages()
    {
        backgroundImages = new Sprite[0];
        currentImageIndex = 0;
        imageSwitchTimer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"清除所有背景图片: {gameObject.name}");
        }
    }
    
    // 公共方法：暂停/恢复移动
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (showDebugInfo)
        {
            Debug.Log($"移动状态更新: {gameObject.name}, 暂停: {paused}");
        }
    }
    
    // 公共方法：重置到初始位置
    public void ResetToStartPosition()
    {
        transform.position = startPosition;
        currentImageIndex = 0;
        imageSwitchTimer = 0f;
        
        // 设置第一张图片
        if (backgroundImages != null && backgroundImages.Length > 0)
        {
            SetCurrentImage(0);
            UpdateAllCopiesImage();
        }
        
        // 确保重置时可见性恢复
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"重置到初始位置: {gameObject.name}");
        }
    }
    
    // 获取当前图片索引
    public int GetCurrentImageIndex()
    {
        return currentImageIndex;
    }
    
    // 获取图片总数
    public int GetImageCount()
    {
        return backgroundImages != null ? backgroundImages.Length : 0;
    }
    
    // 测试方法：显示所有背景位置
    public void DebugBackgroundPositions()
    {
        if (showDebugInfo)
        {
            Debug.Log($"=== 背景位置调试信息 ===");
            Debug.Log($"主背景位置: {transform.position}, 起始位置: {startPosition}");
            Debug.Log($"背景尺寸: 宽度={backgroundWidth}, 高度={backgroundHeight}");
            
            if (backgroundCopiesList != null)
            {
                for (int i = 0; i < backgroundCopiesList.Count; i++)
                {
                    GameObject copy = backgroundCopiesList[i];
                    if (copy != null)
                    {
                        Debug.Log($"背景副本 {i}: {copy.name}, 位置: {copy.transform.position}");
                    }
                }
            }
            
            Debug.Log($"手动偏移: Y={manualCopyOffsetY}, X={manualCopyOffsetX}");
            Debug.Log($"手动起始位置: Y={manualStartPositionY}, X={manualStartPositionX}");
            
            // 计算并显示触发位置
            float triggerY = manualTriggerPositionY != 0f ? manualTriggerPositionY : startPosition.y - backgroundHeight;
            float triggerX = manualTriggerPositionX != 0f ? manualTriggerPositionX : startPosition.x - backgroundWidth;
            Debug.Log($"循环触发位置: Y={triggerY}, X={triggerX}");
            Debug.Log($"================================");
        }
    }
    
    // 公共方法：设置手动Y偏移量
    public void SetManualCopyOffsetY(float offsetY)
    {
        manualCopyOffsetY = offsetY;
        if (showDebugInfo)
        {
            Debug.Log($"手动Y偏移量设置为: {offsetY}");
        }
    }
    
    // 公共方法：设置手动X偏移量
    public void SetManualCopyOffsetX(float offsetX)
    {
        manualCopyOffsetX = offsetX;
        if (showDebugInfo)
        {
            Debug.Log($"手动X偏移量设置为: {offsetX}");
        }
    }
    
    // 公共方法：设置主背景起始Y位置
    public void SetManualStartPositionY(float startY)
    {
        manualStartPositionY = startY;
        if (showDebugInfo)
        {
            Debug.Log($"主背景起始Y位置设置为: {startY}");
        }
    }
    
    // 公共方法：设置主背景起始X位置
    public void SetManualStartPositionX(float startX)
    {
        manualStartPositionX = startX;
        if (showDebugInfo)
        {
            Debug.Log($"主背景起始X位置设置为: {startX}");
        }
    }
    
    // 公共方法：立即移动到新的起始位置
    public void MoveToNewStartPosition()
    {
        Vector3 newStartPosition = new Vector3(
            manualStartPositionX != 0f ? manualStartPositionX : transform.position.x,
            manualStartPositionY != 0f ? manualStartPositionY : transform.position.y,
            transform.position.z
        );
        
        startPosition = newStartPosition;
        transform.position = newStartPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"移动到新起始位置: {newStartPosition}");
        }
    }
    
    // 公共方法：设置循环触发Y位置
    public void SetManualTriggerPositionY(float triggerY)
    {
        manualTriggerPositionY = triggerY;
        if (showDebugInfo)
        {
            Debug.Log($"循环触发Y位置设置为: {triggerY}");
        }
    }
    
    // 公共方法：设置循环触发X位置
    public void SetManualTriggerPositionX(float triggerX)
    {
        manualTriggerPositionX = triggerX;
        if (showDebugInfo)
        {
            Debug.Log($"循环触发X位置设置为: {triggerX}");
        }
    }
    
    // 公共方法：重新创建背景副本（使用新的偏移量）
    public void RecreateBackgroundCopies()
    {
        // 删除现有的副本
        if (backgroundCopiesList != null)
        {
            foreach (GameObject copy in backgroundCopiesList)
            {
                if (copy != null)
                {
                    DestroyImmediate(copy);
                }
            }
            backgroundCopiesList.Clear();
        }
        
        // 重新创建副本
        if (loopBackground)
        {
            CreateBackgroundCopies();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("背景副本已重新创建");
        }
    }
    
    // 公共方法：完全重新初始化背景（包括位置和副本）
    public void ReinitializeBackground()
    {
        // 重新计算起始位置
        if (manualStartPositionX != 0f || manualStartPositionY != 0f)
        {
            startPosition = new Vector3(
                manualStartPositionX != 0f ? manualStartPositionX : transform.position.x,
                manualStartPositionY != 0f ? manualStartPositionY : transform.position.y,
                transform.position.z
            );
            transform.position = startPosition;
        }
        
        // 重新创建副本
        RecreateBackgroundCopies();
        
        if (showDebugInfo)
        {
            Debug.Log("背景完全重新初始化完成");
        }
    }
    

    
    // 在编辑器中显示调试信息
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo && spriteRenderer != null)
        {
            // 绘制背景边界
            Gizmos.color = Color.green;
            Vector3 currentSize = new Vector3(backgroundWidth, backgroundHeight, 0.1f);
            Gizmos.DrawWireCube(transform.position, currentSize);
            
            // 绘制移动方向
            Gizmos.color = Color.cyan;
            if (moveHorizontally)
            {
                Gizmos.DrawRay(transform.position, Vector3.left * 3f);
            }
            if (moveVertically)
            {
                Gizmos.DrawRay(transform.position, Vector3.down * 3f);
            }
            
            // 绘制循环边界
            if (loopBackground)
            {
                Gizmos.color = Color.yellow;
                Vector3 loopPosition = startPosition;
                if (moveVertically)
                {
                    loopPosition.y -= backgroundHeight;
                }
                else if (moveHorizontally)
                {
                    loopPosition.x -= backgroundWidth;
                }
                Gizmos.DrawWireCube(loopPosition, currentSize);
                
                // 绘制隐藏阈值
                if (hideStaticBackgrounds && moveVertically)
                {
                    Gizmos.color = Color.red;
                    Vector3 hidePosition = startPosition;
                    hidePosition.y += backgroundHeight * 0.5f;
                    Gizmos.DrawWireCube(hidePosition, currentSize);
                }
            }
        }
    }
} 