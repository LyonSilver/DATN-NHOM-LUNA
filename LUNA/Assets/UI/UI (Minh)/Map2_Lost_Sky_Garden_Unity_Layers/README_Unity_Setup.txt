MAP 2 - VUON DIA DANG THAT LAC | UNITY 2D LAYER PACKAGE
=================================================================
Source: one generated image (1672 x 941 pixels), 16:9-ish landscape.
File output: separate PNG images, including transparent RGBA layers.

LAYER FILES (Layers_PNG):
  00_Background_Sky_Islands_Temple.png   Background: sky, clouds, floating islands, temple and waterfalls.
  01_Arena_Floor.png                    Stone battle deck, transparent outside the visible arena.
  02_Stone_Front_Arches.png             Front facade / hanging arches, transparent in main openings.
  03_Left_Guardian.png                  Left statue and pedestal with transparent surroundings.
  04_Right_Guardian.png                 Right statue and pedestal with transparent surroundings.

UNITY QUICK SETUP:
  1) Copy Layers_PNG/ into Assets/Art/Maps/Map02/ .
  2) Select all PNG files -> Texture Type: Sprite (2D and UI), Sprite Mode: Single,
     Mesh Type: Full Rect, Pixels Per Unit: SAME VALUE for all files (e.g., 100).
  3) Make 5 SpriteRenderers, assign one image to each.
  4) All 5 game objects should have EXACTLY the same position, scale, rotation.
  5) Sorting Order:
          00_Background_Sky_Islands_Temple : -30
          01_Arena_Floor                  : -20
          02_Stone_Front_Arches           : -10
          03_Left_Guardian                :   0
          04_Right_Guardian               :   1
     Place PLAYER/ENEMY sprites above floor (e.g. 5) depending on desired depth.
  6) Disable Mip Maps for pixel art if needed, use Filter Mode Point for hard pixel edges,
     or Bilinear if you prefer the painterly source-image style.
  7) The arena texture can be reused as a 2D visual only; use separate colliders for gameplay.

IMPORTANT SOURCE LIMITATION:
This package is segmented from a SINGLE FLAT 2D IMAGE, not from the original
art project. Parts hidden behind the platform or statues were never painted
in the input image; the covered portions of the background have been filled
approximately. Foreground masks are hand-traced approximations (not exact
semantic object contours). Substantial parallax motion, scene zoom, or moving
a statue far from its designed place can reveal reconstructed / clipped areas.
The temple, floating islands, waterfalls, sky and far-side ruins are still
BAKED INTO THE SAME BACKGROUND IMAGE. They are not independent editable
assets. For full production-grade parallax, they should be recreated as
independently painted source layers.

PREVIEWS:
  Preview/Layered_Map_Composite.png is how the pieces line up.
  Preview/Checker_*.jpg shows transparency against a checkerboard.
  Map2_Original_Reference.png preserves the exact original artwork.

All layer canvases: 1672 x 941 px.
