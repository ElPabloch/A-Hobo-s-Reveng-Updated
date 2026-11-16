# Smolnogorsk 42 visual baseline

This asset set establishes a working palette and scale for the first playable chapter. It is original art, drawn in the connected Aseprite window. The reference informs gritty colours and readable adult silhouettes; its side-view camera is adapted to the game's oblique top-down view.

## Shared rules

- Nearest filtering and integer display scaling. Never rotate character sheets; choose a facing and mirror right for left.
- Humanoid cell 32 by 48 pixels. Feet pivot 16,42. Visible figure height about 35 pixels. Head remains the same size in every pose.
- Rat cell 32 by 24 pixels; centre pivot 16,12.
- Three directional rows: down, right, up. Four columns: step A, pass, step B, pass. Suggested durations 140,100,140,100 milliseconds. Column 1 can serve as idle initially.
- Selective dark blue outline, cool shadows, warm skin and ochre hero jacket. Enemy wears burgundy and a wool cap. The upward facing shows the back of the head.
- Humanoid gear is rendered separately so weapon pickup does not require redrawing each outfit.
- Character shadows and hit flashes belong to the engine. Atlas source pixels are fully opaque or fully transparent.
- Later factions should keep cell size and shared outline/skin ramps. Change clothing, equipment and silhouette deliberately; avoid arbitrary global colour shifts.

## Atlases

hero.png and hobo.png: 128 by 144, 32 by 48 cells, 4 columns and 3 rows. rat.png: 128 by 72, 32 by 24 cells, same order.

props.png: 256 by 32. Cells: beer, vodka, brick, pipe, bat, knife, collectible whiskey, rusted barrel.

sewer_tiles.png: 192 by 32. Cells: concrete floor slab, brick wall, water, grate, horizontal pipe, vertical pipe. Floor deliberately has a slab seam; water has low-contrast ripples. Pipes are transparent overlays.

All PNG files have matching editable .aseprite sources and compact JSON layout metadata. These are atlases on one source frame, not timeline animations; the engine selects cells.

## Remaining art work

This is the first coherent playable asset set, not a promise of final reference-level art. Further floor variants and intermediate animation poses will reduce repetition. Mirroring the side sheet also mirrors its highlight. Draw a separate west-facing set if directional lighting makes that visible. Source atlases are not yet separated into a full animation rig.

## Combat and factions

hero_combat.png and hobo_combat.png use the same 128 by 144 atlas and 32 by 48 cells as the walks. Rows remain down, right, up. Columns are anticipation, impact, recovery, block. Player anticipation should stay at most 40 milliseconds; impact can hold 80 milliseconds, recovery 120 milliseconds. Enemy anticipation follows the gameplay telegraph. Weapons and impact effects remain separate engine layers.

cop.png uses a wide peaked cap, epaulettes, tunic pockets and a belt. gunner.png uses slicked hair, sunglasses, suit lapels and a dark red tie. Both use the same walk atlas layout and foot pivot. Faction palettes were cleaned of near-duplicates through Aseprite palette remapping.

boss.png has a broader dark suit, burgundy waistcoat, gold chain and cigar. elite.png has the same broader shoulder budget with exposed arms, a patched sleeveless olive coat and a bald bearded head. Both remain on the 32 by 48 grid with the same pivot and walk layout; increase their collision footprint only through deliberate balance changes, not from the atlas rectangle.

environment_props.png is 128 by 64, four columns and two rows of 32 by 32 cells. Row-major order: barrel cluster, rubbish sack and carton, cracked sink, wall ventilator, pallet and crates, fusebox and cables, rusted riser with valve, torn mattress. Render at 2x alongside the characters. These are transparent decorative objects; collision must be explicitly chosen by placement rather than inferred from their rectangular cell.

## Validation

Every atlas was visually inspected through Aseprite preview. Palette analysis reported no off-palette colours. Read-only PNG checks verified dimensions, nonempty cells and alpha limited to 0 or 255. The advertised validate tool was not exposed by this MCP session, so no claim is made that it ran.
