SWORD SLASH PIXEL FX v1 - animated pixel-art sword swings and a hit spark
===============================================================================

One effect, a moon-blade sword slash: three swings and an impact spark. Each
plays once. 4 animations on transparent sprite sheets at native size (80x64
cells), and a matching swing sound. Free for any game, commercial or not (CC0,
below).

ANIMATIONS
  slash        8 frames   411 ms  a sideways swing: a white-edged violet
                                  crescent with a dithered afterimage sweeps
                                  over the top from left to right (plays once,
                                  ends empty)
  slash-down   8 frames   411 ms  a downward chop on the right: the crescent
                                  sweeps from top to bottom (plays once, ends
                                  empty)
  slash-spin  10 frames   511 ms  a spin attack: the crescent runs once round a
                                  flat ellipse (plays once, ends empty)
  hit          6 frames   330 ms  an impact spark: a three-pointed gold-white
                                  flash with a hot burst, then a scatter of
                                  sparks (plays once, ends empty)

  The frames' pivot is the cell's centre (0.5, 0.5): put it on the swordsman's
  hand, or the hit spark's on the point of impact. Flip the sprite horizontally
  for a swing to the left (Godot flip_h, Unity flipX, Phaser setFlipX); the
  pivot is the cell's centre, so it stays put.

FOLDERS
  sheets/          fx-slash.png (RGBA), its atlas (JSON) and a Godot 4
                   SpriteFrames (.tres). The swings' white edge has a dithered
                   dark-violet halo, for dark backgrounds; the hit has none, so
                   its frames are the same in both sheets.
  sheets-no-halo/  fx-slash-clean: the same frames without the halo, for light
                   or busy backgrounds.
  previews/        a GIF of each animation at 4x on a dark background
  recipe/          the Pixelbolt recipe that draws every frame (see MAKE YOUR
                   OWN)
  sounds/          fx-slash-swing.wav, for slash and slash-down (see SOUND)
  pieces.json      the animations, frame counts, cell size and lengths

USING THE SHEETS
  Every frame's rectangle and duration (ms) is in the atlas JSON; the sheet is
  a grid of equal cells, left to right, top to bottom. Frames run 40 to 90 ms
  (fast at the start of a one-shot, slower as it settles). Scale only by whole
  numbers (2x, 3x, 4x) with nearest-neighbour filtering.
  Godot 4: copy a .tres and the PNG beside it into your project and set an
    AnimatedSprite2D's Sprite Frames to the .tres; the animation names are
    slash, slash-down, slash-spin, hit. For crisp pixels set the node's Texture
    Filter to Nearest.
  Unity (we tested the files in Godot 4 only): import the PNG as Sprite (2D and
    UI), Sprite Mode Multiple, Filter Mode Point, Compression None; in the
    Sprite Editor use Slice > Grid By Cell Size 80x64 and tick Keep Empty
    Rects, so one-shots keep their empty last frame.
  Phaser and PixiJS (untested): the atlas follows TexturePacker's JSON (Hash)
    layout, with each frame's 0-1 pivot (the cell's centre; in pixels under
    points.anchor_px), "animations" as lists of frame names and a "loops" map.
    Build each animation from those lists; neither loader reads the per-frame
    durations.

SOUND
  fx-slash-swing.wav goes with slash, and fits slash-down too (the same frame
  durations): 411 ms, the same length as the animation. Start it on the same
  tick as frame 0. A soft, airy whoosh swells up through frame 0 and is loudest
  over frames 1 and 2, where the crescent changes fastest, then falls away with
  the trail. With slash-spin (511 ms) it ends before the spin does: it still
  works as a swing sound there, but it isn't timed to the spin. The hit spark
  has no sound of its own.
  The timing assumes each frame plays for its atlas duration, as the Godot
  .tres does. In Phaser give each frame its duration in anims.create, in PixiJS
  use {texture, time} frames, and in Unity key the clip from the atlas's
  durations.
  Mono WAV, 48 kHz, 16-bit, mastered quiet (about -27 LUFS on its loudest 100
  ms) so it sits under music: start at 100 % and mix down rather than adding
  gain.

LICENCE
  CC0 1.0 Universal (public domain dedication): see LICENSE.txt. Use the art
  and the sound in any project, commercial or not, change them, no permission
  needed. Credit isn't required, but it is appreciated:
    "Pixel FX by Skulitom Rivskiy, made with Pixelbolt (github.com/skulitom/Pixelbolt)"

MAKE YOUR OWN
  recipe/fx-slash.fx.json is the whole effect as a Pixelbolt recipe: shapes,
  colour ramps and particle settings. Change colours or sizes, then rebuild
  with Node 20 or newer:
    npx pixelbolt@0.13.0 compile fx-slash.fx.json --out fx-slash.pixel.json
    npx pixelbolt@0.13.0 render fx-slash.pixel.json --out fx-slash
  Pixelbolt's own atlas gives each frame's anchor in pixels and the animations
  as objects; if you load it in Phaser or PixiJS, use the 0-1 pivot instead (or
  keep the shipped atlas when only the colours change). The no-halo sheet is
  the same recipe with every look's "halo" removed.

HOW IT WAS MADE (AI DISCLOSURE)
  Made by AI agents with Pixelbolt (MIT). A language model (Anthropic's Claude)
  wrote the recipe from Pixelbolt's shape layers: the shapes' sizes and timing,
  colour ramps, glow and particle settings. Pixelbolt, our MIT-licensed
  pixel-art engine (version 0.13.0), then drew every frame with its own
  algorithms (no machine learning). No image model was used, and nothing was
  traced or sampled from anyone else's art. The sound was made by AI agents
  too: Claude designed it and wrote the code that synthesises it from
  band-filtered noise in Python (NumPy and SciPy), timed to the frames. No AI
  audio model, samples or recordings were used.

LINKS
  Pixelbolt (make effects like these from JSON recipes): https://github.com/skulitom/Pixelbolt
  Try it in your browser: https://skulitom.github.io/Pixelbolt/playground/
