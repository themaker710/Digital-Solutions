using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public RawImage rawImage;
    Texture[] textures;
    // Start is called before the first frame update
    void Start()
    {
        textures = new Texture[2];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = Resources.Load<Texture>("Images/" + i + ".png");
        }

        rawImage.texture = textures[1];

        //rawImage.texture = Resources.Load<Texture2D>("Images/1.png");
        rawImage.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/1.png");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
