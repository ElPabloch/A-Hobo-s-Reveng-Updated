# Chapter 1: sewer details — art brief

256 × 128 transparent atlas, four columns and two rows of 64 × 64 cells. One source frame, one editable art layer. Orthographic oblique top-down projection; visible front faces and short top planes. Upper-left light, selective dark blue outlines, hard pixel clusters, no gradients/antialiasing. Exact established 18-colour palette from environment_props.aseprite, unchanged.

Row-major objects: barred arched passage (48px clear architectural height), broken stair exit, pipe elbow and wheel valve, battered electrical cabinet with bent door; refuse with cartons and bottles, improvised bed, warning sign НЕ ЛЕЗЬ, leaking utility sink.

The screenshot reference informs gritty cool shadows and wear, not side-view gameplay. Forms use the existing 32px floor tile density: each large prop occupies up to two tiles. Gate and stairs intentionally larger than existing 32px small props. Refuse and bed asymmetrical. No character/weapon changes and no new game mechanics.

Palette roles: #101a24 outline; #1c2d31 #293c3e #3a4d49 #52625a #708075 stone/metal; #633d3c #9a5845 rust; #b2b59a #d1c9a9 dirty enamel/paper; #33504a #46675a glass/moss; #65563d #9c7b4b #c4a16a wood/cardboard; #3d4964 #69778a cloth; #e2b48e small warm accent.

Spec accepted from established project style and autonomous work request. No additional image-model concept round; existing assets and user's screenshot are the style reference.

## Передача разработчику — 6 ноября 2026

- Исходник: `ArtSource/chapter1_details.aseprite`.
- Экспорт: `Assets/Hobo/Resources/Art/chapter1_details.png` и соседний JSON.
- Размер: 256×128, один кадр. Объекты расположены по строкам, ячейки 64×64.
- Восемь именованных slices: `sewer_gate`, `broken_stairs`, `pipe_valve`, `electrical_cabinet`, `refuse_pile`, `improvised_bed`, `warning_sign`, `utility_sink`.
- Точка привязки каждого slice: `(32, 60)` относительно его ячейки, координаты от верхнего левого угла Aseprite. Для Unity нормализованный pivot: `(0.5, 0.0625)` с Y от нижнего края.
- Экспорт без обрезки и без отступов между кадрами, чтобы сохранить фиксированную сетку. Использовать Point filtering и отключённые mipmaps.
- Коллизии этим объектам не назначены. Новые PNG не расставлены по комнатам; существующий рендерер режет свои старые атласы на 32-пиксельные ячейки и не должен так же нарезать этот.
- Изображение просмотрено в Aseprite; независимый художественный разбор: 7/10, блокирующих замечаний нет. Койка менее однозначна, чем остальные объекты: будущая правка — яснее показать верх матраса. Пиксели после разбора не менялись.
- Игра, кодовые тесты и валидатор не запускались на этом этапе по просьбе автора. Художественный просмотр не является игровым тестированием.
