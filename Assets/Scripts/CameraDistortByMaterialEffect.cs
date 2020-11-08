using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDistortByMaterialEffect : MonoBehaviour {

    public Material EffectMat;

    private Vector2 EffectSize{get; set;}

    private void OnRenderImage(RenderTexture sourceImage, RenderTexture destinationImage)
    {
        Graphics.Blit(sourceImage, destinationImage, EffectMat);
    }

    public void SetUpEffect(GameController gc)
    {
        var screenSize = gc.ScreenSize;
        var effectSize = Mathf.Max(screenSize.width, screenSize.height) / 5f;

        EffectSize = new Vector2(effectSize, effectSize);

        EffectMat = Resources.Load<Material>("Materials/PaperDistortionMaterial");

        EffectMat.SetFloat("_EffectSizeX", effectSize);
        EffectMat.SetFloat("_EffectSizeY", effectSize);
        EffectMat.SetFloat("_EffectStartPosX", 0f);
        EffectMat.SetFloat("_EffectStartPosY", 0f);
    }

    public void Activate(GameController gc){
        if (!Input.touchSupported)
            return;

        EffectMat.SetFloat("_ScreenSizeX", gc.ScreenSize.width);
        EffectMat.SetFloat("_ScreenSizeY", gc.ScreenSize.height);

        EffectMat.SetFloat("_EffectStartPosX", Input.mousePosition.x - EffectSize.x / 2f);
        EffectMat.SetFloat("_EffectStartPosY", Input.mousePosition.y - EffectSize.y / 2f);

        enabled = true;
    }

    public void Update(){
        EffectMat.SetFloat("_EffectStartPosX", Input.mousePosition.x - EffectSize.x / 2f);
        EffectMat.SetFloat("_EffectStartPosY", Input.mousePosition.y - EffectSize.y / 2f);
    }

    public void Deactivate(){
        EffectMat.SetFloat("_EffectStartPosX", 0);
        EffectMat.SetFloat("_EffectStartPosY", 0);

        enabled = false;
    }

}
