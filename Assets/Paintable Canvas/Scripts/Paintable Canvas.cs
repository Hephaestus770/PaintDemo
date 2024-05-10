using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaintableCanvas : MonoBehaviour
{
    enum EPaintingMode
    {
        Off,
        Draw,
        Bucket,
        Stamp,
        Erase
    }

    [SerializeField] float RaycastDistance = 10.0f;
    [SerializeField] LayerMask PaintableCanvasLayerMask = ~0;
    [SerializeField] MeshFilter CanvasMeshFilter;
    [SerializeField] MeshRenderer CanvasMeshRenderer;
    [SerializeField] int PixelsPerMetre = 200;
    [SerializeField] Color CanvasDefaultColor = Color.white;
    [SerializeField] float BrushScale = 0.25f;
    [SerializeField] float BrushWeight = 500.0f;

    [SerializeField] AudioClip[] BrushSoundClips;
    [SerializeField] AudioClip[] BucketSoundClips;
    [SerializeField] AudioClip[] StampSoundClips;
    [SerializeField] AudioClip[] EraseSoundClips;

    private bool isDrawingSoundPlaying = false;
    EPaintingMode PaintingMode_PrimaryMouse = EPaintingMode.Draw;

    int CanvasWidthInPixels;
    int CanvasHeightInPixels;

    protected Texture2D PaintableTexture;

    BaseBrush ActiveBrush;
    Color ActiveColor = Color.white;

    void Start()
    {
        CanvasWidthInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.x * CanvasMeshFilter.transform.localScale.x * PixelsPerMetre);
        CanvasHeightInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.y * CanvasMeshFilter.transform.localScale.y * PixelsPerMetre);
        PaintableTexture = new Texture2D(CanvasWidthInPixels, CanvasHeightInPixels, TextureFormat.ARGB32, false);

        // if this is the first time create blank canvas
        if (!PlayerPrefs.HasKey("firstTime"))
        {
            Debug.Log("First time in the game.");
            PlayerPrefs.SetInt("firstTime", 0);
                                                
            for (int Y = 0; Y < CanvasHeightInPixels; Y++)
            {
                for (int X = 0; X < CanvasWidthInPixels; X++)
                {
                    PaintableTexture.SetPixel(X, Y, CanvasDefaultColor);
                }
            }

            PaintableTexture.Apply();

        }
        //if this isn't the first time load the saved Texture
        else
        {
            Debug.Log("It is not the first time in the game.");
            // Get the saved texture data from PlayerPrefs
            string TextureDataString = PlayerPrefs.GetString("PaintableCanvasTexture");

            // Convert texture data from Base64 string to byte array
            byte[] TextureData = System.Convert.FromBase64String(TextureDataString);

            // Create a new texture and load the saved texture 
            PaintableTexture = new Texture2D(1, 1);
            PaintableTexture.LoadImage(TextureData);
            PaintableTexture.Apply();

           
        }


        CanvasMeshRenderer.material.mainTexture = PaintableTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (ActiveBrush != null)
        {

            if (PaintingMode_PrimaryMouse == EPaintingMode.Bucket && Input.GetMouseButtonDown(0))
            {
                Update_PerformFlood();
            }


            if (PaintingMode_PrimaryMouse == EPaintingMode.Stamp && Input.GetMouseButtonDown(0))
            {
                Update_PerformStamp();
            }

            if (PaintingMode_PrimaryMouse == EPaintingMode.Erase && Input.GetMouseButton(0))
            {
                Update_PerformErase();
            }


            if (PaintingMode_PrimaryMouse == EPaintingMode.Draw && Input.GetMouseButton(0))
            {
                Update_PerformDrawing();
            }

        }
    }

    RaycastHit[] HitResults = new RaycastHit[1];
    
    void Update_PerformDrawing()
    {   
         Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
         if (Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
         {      
            PerformDrawingWith(ActiveBrush, ActiveColor, HitResults[0].textureCoord);
            if (!isDrawingSoundPlaying)
            {
                SoundEffectManager.Instance.PlaySoundFXClips(BrushSoundClips, transform, 1f);
                isDrawingSoundPlaying = true;
                StartCoroutine(ResetDrawingSoundFlag(BrushSoundClips));
            }
            
            
         }

    }
   
    void Update_PerformFlood()
    {   
        Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
        {
            FillFlood(ActiveColor, HitResults[0].textureCoord);
            if (!isDrawingSoundPlaying)
            {
                SoundEffectManager.Instance.PlaySoundFXClips(BucketSoundClips, transform, 1f);
                isDrawingSoundPlaying = true;
                StartCoroutine(ResetDrawingSoundFlag(BucketSoundClips));
            }
        }
    }

    void Update_PerformStamp()
    {
        Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
        {
            Stamp(ActiveBrush.BrushTexture, HitResults[0].textureCoord);
            if (!isDrawingSoundPlaying)
            {
                SoundEffectManager.Instance.PlaySoundFXClips(StampSoundClips, transform, 1f);
                isDrawingSoundPlaying = true;
                StartCoroutine(ResetDrawingSoundFlag(StampSoundClips));
            }
        }
    }

    void Update_PerformErase()
    {
        Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
        {
            Erase(ActiveBrush, HitResults[0].textureCoord);
            if (!isDrawingSoundPlaying)
            {
                SoundEffectManager.Instance.PlaySoundFXClips(EraseSoundClips, transform, 1f);
                isDrawingSoundPlaying = true;
                StartCoroutine(ResetDrawingSoundFlag(EraseSoundClips));
            }

        }
    }
   
      void PerformDrawingWith(BaseBrush ActiveBrush, Color ActiveColor, Vector2 LocationUV)
      {
          int DrawingOriginX = Mathf.RoundToInt(LocationUV.x * CanvasWidthInPixels);
          int DrawingOriginY = Mathf.RoundToInt(LocationUV.y * CanvasHeightInPixels);
        
          int ScaledBrushWidth = Mathf.RoundToInt(ActiveBrush.BrushTexture.width * BrushScale);
          int ScaledBrushHeight = Mathf.RoundToInt(ActiveBrush.BrushTexture.height * BrushScale);

          for (int BrushY = 0; BrushY < ScaledBrushHeight; BrushY++)
          {
              int PixelY = DrawingOriginY + BrushY - (ScaledBrushHeight / 2);
              if (PixelY < 0 || PixelY >= CanvasHeightInPixels)
                  continue;

              float BrushUV_Y = (float)BrushY / (float)ScaledBrushHeight;

              for (int BrushX = 0; BrushX < ScaledBrushWidth; BrushX++)
              {
                  int PixelX = DrawingOriginX + BrushX - (ScaledBrushWidth / 2);
                  if (PixelX < 0 || PixelX >= CanvasWidthInPixels)
                      continue;

                  // calculate the brush UV to lookup
                  float BrushUV_X = (float)BrushX / (float)ScaledBrushWidth;

                  Color BrushPixel = ActiveBrush.BrushTexture.GetPixelBilinear(BrushUV_X, BrushUV_Y);
                  Color CanvasPixel = PaintableTexture.GetPixel(PixelX, PixelY);

                  CanvasPixel = ActiveBrush.Apply(CanvasPixel, BrushPixel, ActiveColor, BrushWeight * Time.deltaTime);
                  PaintableTexture.SetPixel(PixelX, PixelY, CanvasPixel);
              }
          }
          PaintableTexture.Apply();

      }



    void FillFlood(Color TargetColor, Vector2 StartPointUV)
    {
        int StartX = Mathf.RoundToInt(StartPointUV.x * CanvasWidthInPixels);
        int StartY = Mathf.RoundToInt(StartPointUV.y * CanvasHeightInPixels);

        // Get the target color at the start point
        Color OriginalColor = PaintableTexture.GetPixel(StartX, StartY);

        // Create a Stack for storing pixels to be processed
        Stack<Vector2Int> Stack = new Stack<Vector2Int>();
        Stack.Push(new Vector2Int(StartX, StartY));

        // Create a HashSet to keep track of processed pixels
        HashSet<Vector2Int> ProcessedPixels = new HashSet<Vector2Int>();

        // Flood fill loop
        while (Stack.Count > 0)
        {
            Vector2Int Pixel = Stack.Pop();
            int x = Pixel.x;
            int y = Pixel.y;

            if (x >= 0 && x < CanvasWidthInPixels && y >= 0 && y < CanvasHeightInPixels &&
                PaintableTexture.GetPixel(x, y) == OriginalColor && !ProcessedPixels.Contains(Pixel))
            {
                PaintableTexture.SetPixel(x, y, TargetColor);
                ProcessedPixels.Add(Pixel);

                Stack.Push(new Vector2Int(x + 1, y));
                Stack.Push(new Vector2Int(x - 1, y));
                Stack.Push(new Vector2Int(x, y + 1));
                Stack.Push(new Vector2Int(x, y - 1));
            }
        }

        PaintableTexture.Apply();
    }

    void Stamp(Texture2D BrushTexture, Vector2 LocationUV)
    {
        int StampWidth = Mathf.RoundToInt(BrushTexture.width * BrushScale);
        int StampHeight = Mathf.RoundToInt(BrushTexture.height * BrushScale);

   

        int StartX = Mathf.RoundToInt(LocationUV.x * CanvasWidthInPixels) - StampWidth / 2;
        int StartY = Mathf.RoundToInt(LocationUV.y * CanvasHeightInPixels) - StampHeight / 2;

        for (int y = 0; y < StampHeight; y++)
        {
            for (int x = 0; x < StampWidth; x++)
            {
                Color stampPixel = BrushTexture.GetPixelBilinear((float) x / StampWidth, (float) y / StampHeight);
                PaintableTexture.SetPixel(StartX + x, StartY + y, stampPixel);
            }
        }

        PaintableTexture.Apply();
    }

    void Erase(BaseBrush EraserBrush, Vector2 LocationUV)
    {
        int ErasingOriginX = Mathf.RoundToInt(LocationUV.x * CanvasWidthInPixels);
        int ErasingOriginY = Mathf.RoundToInt(LocationUV.y * CanvasHeightInPixels);

        int ScaledBrushWidth = Mathf.RoundToInt(EraserBrush.BrushTexture.width * BrushScale * .5f);
        int ScaledBrushHeight = Mathf.RoundToInt(EraserBrush.BrushTexture.height * BrushScale * .5f);

        for (int BrushY = 0; BrushY < ScaledBrushHeight; BrushY++)
        {
            int PixelY = ErasingOriginY + BrushY - (ScaledBrushHeight / 2);
            if (PixelY < 0 || PixelY >= CanvasHeightInPixels)
                continue;

            float BrushUV_Y = (float)BrushY / (float)ScaledBrushHeight;

            for (int BrushX = 0; BrushX < ScaledBrushWidth; BrushX++)
            {
                int PixelX = ErasingOriginX + BrushX - (ScaledBrushWidth / 2);
                if (PixelX < 0 || PixelX >= CanvasWidthInPixels)
                    continue;

                float BrushUV_X = (float)BrushX / (float)ScaledBrushWidth;

                               
                PaintableTexture.SetPixel(PixelX, PixelY, CanvasDefaultColor);
            }
        }

        PaintableTexture.Apply();
    }
    
    public void SelectBrush(BaseBrush InBrush)
    {
        ActiveBrush = InBrush;
        if(InBrush.bIsStamp==true)
        {
            PaintingMode_PrimaryMouse = EPaintingMode.Stamp;
        }

        if (InBrush.bIsBucket==true)
        {
            PaintingMode_PrimaryMouse = EPaintingMode.Bucket;
        }

        if(InBrush.bIsEraser == true)
        {
            PaintingMode_PrimaryMouse = EPaintingMode.Erase;
        }

        if(InBrush.bIsStamp==false && InBrush.bIsBucket==false && InBrush.bIsEraser==false)
        {
            PaintingMode_PrimaryMouse = EPaintingMode.Draw;
        }


    }


    public void SetColor(Color InColor)
    {
        ActiveColor = InColor;
        
    }


    public void SaveCanvas()
    {
        if (PaintableTexture != null)
        {
            byte[] TextureData = PaintableTexture.EncodeToPNG();

            // Save the texture data to PlayerPrefs
            PlayerPrefs.SetString("PaintableCanvasTexture", System.Convert.ToBase64String(TextureData));

            PlayerPrefs.Save();
        }
    }

    IEnumerator ResetDrawingSoundFlag(AudioClip[] AudioClip)
    {
        int number = SoundEffectManager.Instance.RandomGen(AudioClip);
        // Wait for the duration of the sound effect
        yield return new WaitForSeconds(AudioClip[number].length);
        isDrawingSoundPlaying = false;
    }


    public Texture2D GetPaintableTexture() 
    {
        return PaintableTexture;
    }


    private void OnApplicationQuit()
    {
        SaveCanvas();
    }
}


