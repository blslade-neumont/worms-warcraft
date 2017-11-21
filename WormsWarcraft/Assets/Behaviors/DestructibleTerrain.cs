using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DestructibleTerrain : NetworkBehaviour
{
    [SerializeField] public Sprite initialSprite;
    [SerializeField] public Sprite initialDebrisSprite;
    [SerializeField] public Sprite initialPhysicsMap;

    private Texture2D spriteTex;
    private Texture2D debrisSpriteTex;
    private Texture2D physicsMapTex;

    private Sprite currentSprite;
    private Sprite currentDebrisSprite;
    private Sprite currentPhysicsMap;

    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public SpriteRenderer debrisRenderer;
    [SerializeField] public SpriteRenderer physicsColliderRenderer;

    private void Start()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (!initialSprite && !!spriteRenderer) initialSprite = spriteRenderer.sprite;
        if (!initialDebrisSprite && !!debrisRenderer) initialDebrisSprite = debrisRenderer.sprite;

        if (!initialSprite) throw new NotSupportedException("The DestructableTerrain must have an initial sprite.");
        if (!initialDebrisSprite) throw new NotSupportedException("The DestructableTerrain must have an initial debris sprite.");
        if (!initialPhysicsMap) throw new NotSupportedException("The DestructableTerrain must have an initial physics map.");

        if (!spriteRenderer) throw new NotSupportedException("The DestructableTerrain must have a sprite renderer.");
        if (!physicsColliderRenderer) throw new NotSupportedException("The DestructableTerrain must have a physics collider renderer for the physics.");

        cloneSprite(this.initialSprite, false, out spriteTex, out currentSprite);
        cloneSprite(this.initialDebrisSprite, false, out debrisSpriteTex, out currentDebrisSprite);
        cloneSprite(this.initialPhysicsMap, true, out physicsMapTex, out currentPhysicsMap);

        spriteRenderer.sprite = currentSprite;
        debrisRenderer.sprite = currentDebrisSprite;
        physicsColliderRenderer.sprite = currentPhysicsMap;
        this.updatePhysicsCollider();
    }

    private void cloneSprite(Sprite origSprite, bool isPhysicsMap, out Texture2D newTex, out Sprite newSprite)
    {
        var origTex = origSprite.texture;
        int width = (int)origSprite.textureRect.width, height = (int)origSprite.textureRect.height;
        int xmin = (int)origSprite.textureRect.xMin, ymin = (int)origSprite.textureRect.yMin;
        newTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        newTex.wrapMode = origTex.wrapMode;
        //Debug.Log("Sprite size: [" + width + ", " + height + "], mapped from [" + xmin + ", " + ymin + "]");
        if (isPhysicsMap) copyTexturesGrayscale(origTex, newTex, 0, 0, width, height, xmin, ymin);
        copyTextures(origTex, newTex, 0, 0, width, height, xmin, ymin);
        newTex.Apply();
        var meshType = origSprite.packed ? SpriteMeshType.Tight : SpriteMeshType.FullRect;
        newSprite = Sprite.Create(newTex, new Rect(0, 0, width, height), origSprite.pivot, origSprite.pixelsPerUnit, 0, meshType);
    }
    private void copyTextures(Texture2D orig, Texture2D newTex, int startx, int starty, int endx, int endy, int offx = 0, int offy = 0)
    {
        for (var xx = startx; xx < endx; xx++)
        {
            for (var yy = starty; yy < endy; yy++)
            {
                var pix = orig.GetPixel(offx + xx, offy + yy);
                newTex.SetPixel(xx, yy, pix);
            }
        }
    }
    private void copyTexturesGrayscale(Texture2D orig, Texture2D newTex, int startx, int starty, int endx, int endy, int offx = 0, int offy = 0)
    {
        for (var xx = startx; xx < endx; xx++)
        {
            for (var yy = starty; yy < endy; yy++)
            {
                var pix = orig.GetPixel(offx + xx, offy + yy);
                newTex.SetPixel(xx, yy, new Color(1, 1, 1, pix.grayscale));
            }
        }
    }

    private void updatePhysicsCollider()
    {
        var physicsObj = this.physicsColliderRenderer.gameObject;
        var oldCollider = physicsObj.GetComponent<PolygonCollider2D>();
        Destroy(oldCollider);
        physicsObj.AddComponent<PolygonCollider2D>();
    }
}
