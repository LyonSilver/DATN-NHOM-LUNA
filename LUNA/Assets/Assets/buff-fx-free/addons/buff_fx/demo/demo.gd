extends Node2D
## Pick a colour and an effect to play it where it belongs: on the hero or over their head, on
## the loot, round the item slot or across the card. Loops stay until picked again or replaced by
## one in the same place; left alone, the demo plays through them.

const HERO := Vector2(360, 560)   # feet
const LOOT := Vector2(760, 560)
const CARD := Rect2(1010, 190, 240, 120)
const SLOT := Rect2(1070, 380, 120, 120)
const SHAPES := ["buff", "debuff", "heal", "shield", "levelup", "aura", "haste", "orbit", "charge", "wings", "absorb",
	"stun", "statup", "statdown", "alert", "halo", "crown",
	"beam", "drop", "coins", "chest", "powerup", "pickup", "glow", "sparkle", "frame", "shine", "ping", "rating"]
## The loops, by the place each plays in: one at a time there.
const LOOPS := {"hero": ["buff", "debuff", "shield", "aura", "haste", "orbit", "charge", "wings", "absorb"],
	"head": ["stun", "statup", "statdown", "halo", "crown"],
	"loot": ["glow", "sparkle", "beam", "powerup", "chest", "ping"], "slot": ["frame"]}
const IDLE := 3.0

var palette := ""
var idle := 0.5
var palettes: Array[String] = []
var shapes: Array[String] = []
var palette_row: HFlowContainer
var looping := {}   # place: the BuffFX looping there
var auto := 0


func _ready() -> void:
	var have := BuffFX.effects()
	for fx_name in have:
		var p := fx_name.get_slice("_", 0)
		if p not in palettes:
			palettes.append(p)
	for s: String in SHAPES:
		if palettes.any(func(p: String) -> bool: return (p + "_" + s) in have):
			shapes.append(s)
	palette = palettes[0]
	var bar := VBoxContainer.new()
	bar.position = Vector2(16, 12)
	add_child(bar)
	palette_row = _choices(palettes, true, func(v: String) -> void: palette = v)
	bar.add_child(palette_row)
	bar.add_child(_choices(shapes, false, func(v: String) -> void:
		idle = IDLE
		palette = _colour(palette, v)
		_show(palette_row, palette)
		play(palette, v)))
	var first: Dictionary = BuffFX.info(have[0])
	var sets := ["256", "128", "pixel"].filter(func(s: String) -> bool:  # the web build leaves 256 out
		return ResourceLoader.exists(BuffFX.sheets_dir.path_join(s).path_join(first.file)))
	BuffFX.default_set = sets[0]
	bar.add_child(_choices(sets, true, func(v: String) -> void:
		BuffFX.default_set = v
		for fx: BuffFX in looping.values():
			if is_instance_valid(fx):
				play(fx.effect.get_slice("_", 0), fx.effect.get_slice("_", 1), true)))
	var hint := Label.new()
	hint.text = "Loops stay on until picked again."
	hint.add_theme_color_override("font_color", Color(0.75, 0.78, 0.9))
	bar.add_child(hint)


func _choices(values: Array, toggles: bool, pick: Callable) -> HFlowContainer:
	var row := HFlowContainer.new()
	row.custom_minimum_size.x = 1248
	var group := ButtonGroup.new()
	for v: String in values:
		var b := Button.new()
		b.text = v
		if toggles:
			b.toggle_mode = true
			b.button_group = group
			b.button_pressed = v == values[0]
		b.pressed.connect(pick.bind(v))
		row.add_child(b)
	return row


## Plays `p`_`what` in its place. A loop replaces the one there; picked again, it ends.
## `p` if it comes in `what`, else the first colour that does: a free sampler has each effect in one.
func _colour(p: String, what: String) -> String:
	for q: String in [p] + palettes:
		if (q + "_" + what) in BuffFX.effects():
			return q
	return p


func _show(row: HFlowContainer, v: String) -> void:
	for b: Button in row.get_children():
		b.set_pressed_no_signal(b.text == v)


func play(p: String, what: String, restart := false) -> void:
	p = _colour(p, what)
	var fx_name := p + "_" + what
	if fx_name not in BuffFX.effects():
		return
	var at := HERO
	if what == "shield":
		at = HERO + Vector2(0, -75)
	elif what in ["stun", "halo"]:
		at = HERO + Vector2(0, -150)
	elif what == "crown":
		at = HERO + Vector2(0, -182)
	elif what in ["statup", "statdown", "alert"]:
		at = HERO + Vector2(0, -200)
	elif what == "rating":
		at = CARD.get_center()
	elif what in ["pickup", "glow", "sparkle"]:
		at = LOOT + Vector2(0, -22)
	elif what == "powerup":
		at = LOOT + Vector2(0, -110)
	elif what in ["beam", "drop", "coins", "ping"]:
		at = LOOT
	elif what == "chest":
		at = LOOT + Vector2(0, -42)
	elif what == "shine":
		at = CARD.get_center()
	elif what == "frame":
		at = SLOT.get_center()
	var fx := BuffFX.spawn(self, fx_name, at)
	var e := BuffFX.info(fx_name)
	var cell := Vector2(e.cell[fx.sheet_set][0], e.cell[fx.sheet_set][1])
	fx.scale = Vector2.ONE * e.cell["256"][0] / cell.x   # every set at one size
	if what == "shine":
		fx.scale = CARD.size / cell
	elif what == "frame":
		fx.scale = Vector2.ONE * SLOT.size.x / (0.8 * cell.x)   # its border (0.74 of the cell) just inside the slot
	for place: String in LOOPS:
		if what in LOOPS[place]:
			looping[place] = _swap(looping.get(place), fx, restart)


func _swap(old: BuffFX, fx: BuffFX, restart: bool) -> BuffFX:
	if is_instance_valid(old):
		old.finish()
		if old.effect == fx.effect and not restart:
			fx.queue_free()
			return null
	return fx


func _process(delta: float) -> void:
	idle -= delta
	if idle > 0.0:
		return
	idle = 1.3
	auto += 1
	var p: String = palettes[(auto / 4) % palettes.size()]
	var loops: Array = []
	for place: String in LOOPS:
		loops.append_array(LOOPS[place])
	var once := shapes.filter(func(s: String) -> bool: return s not in loops)
	var places := LOOPS.keys().filter(func(place: String) -> bool:
		return LOOPS[place].any(func(s: String) -> bool: return s in shapes))
	if auto % 2 == 1 and not places.is_empty():   # every other step, the next place's next loop
		var there: Array = LOOPS[places[(auto / 2) % places.size()]].filter(func(s: String) -> bool: return s in shapes)
		play(p, there[(auto / (2 * places.size())) % there.size()], true)
	elif not once.is_empty():
		play(p, once[(auto / 2) % once.size()])


func _draw() -> void:
	draw_rect(get_viewport_rect(), Color(0.07, 0.06, 0.11))
	draw_rect(Rect2(0, 560, 1280, 160), Color(0.1, 0.09, 0.15))
	# a hero, a chest of loot, an item slot and a card, in flat shapes
	draw_colored_polygon(PackedVector2Array([HERO + Vector2(-26, 0), HERO + Vector2(0, -120),
		HERO + Vector2(26, 0)]), Color(0.3, 0.32, 0.5))
	draw_circle(HERO + Vector2(0, -136), 18, Color(0.85, 0.75, 0.6))
	draw_rect(Rect2(LOOT + Vector2(-30, -42), Vector2(60, 42)), Color(0.5, 0.33, 0.18))
	draw_rect(Rect2(LOOT + Vector2(-30, -42), Vector2(60, 10)), Color(0.8, 0.62, 0.25))
	draw_rect(Rect2(LOOT + Vector2(-5, -30), Vector2(10, 12)), Color(0.9, 0.78, 0.35))
	draw_rect(SLOT, Color(0.13, 0.12, 0.2))
	draw_rect(SLOT.grow(-10), Color(0.18, 0.17, 0.28))
	draw_circle(SLOT.get_center(), 26, Color(0.55, 0.6, 0.75))   # a gem in the slot
	draw_rect(CARD, Color(0.2, 0.2, 0.34))
	draw_rect(CARD.grow(-6), Color(0.28, 0.27, 0.46))
	draw_string(ThemeDB.fallback_font, CARD.position + Vector2(0, 70), "REWARD", HORIZONTAL_ALIGNMENT_CENTER,
			CARD.size.x, 30, Color(0.95, 0.9, 0.75))
