# Zombie War — Active Development Plan

## Authoring setup hoàn tất

- [x] Prefab thật cho Soldier, Zombie, Projectile, Bomb và landscape HUD; pool nhận prefab qua serialized reference.
- [x] Scene thật `Boot`, `MainMenu`, `Level01`, `Level02`; dependency được nối sẵn trong Editor và lưu vào scene.
- [x] HUD dùng `TextMeshProUGUI`, TMP Essential Resources và font SDF; không dùng legacy `UnityEngine.UI.Text`.
- [x] Đã loại bỏ runtime bootstrap và mọi thao tác dựng hierarchy bằng `new GameObject`/`CreatePrimitive` trong gameplay.
- [x] Menu `Zombie War/Author Project Assets` là công cụ authoring thủ công, không tự chạy khi mở project hoặc trong runtime.

## Mục tiêu hiện tại

Hoàn thiện vertical slice Android landscape 2560×1440 có vòng chơi 180 giây, auto-fire,
hai súng, bom vật lý và cao trào 80–120 zombie ở 60 FPS trên thiết bị tầm trung.

## Trạng thái triển khai

- [x] Foundation: scene flow, landscape settings, joystick, soldier, camera và graybox hai level.
- [x] Combat core: damage API, auto-target, Rifle, Shotgun, switch, bomb và projectile pool.
- [x] Crowd core: pool 130, hard cap 120, AI scheduler 10/4/2 Hz và viewport spawn có NavMesh sampling.
- [x] Presentation core: safe-area HUD, hit flash, dissolve shader và recoil event hook.
- [x] Bomb aiming: `Play_Joystick_Skill_3Step` cố định, preview quỹ đạo, vòng giới hạn ném 5 m, vùng nổ và cooldown 10 giây.
- [x] Bomb inventory: tối đa 3 quả, UI 3 bước đồng bộ số lượng và JMO mobile explosion VFX được pool/tự tắt.
- [x] Weapon selection: radial menu 3 slot; icon và prefab vũ khí được lưu bằng Addressable reference trong `WeaponConfig`.
- [x] Verification: runtime/editor/test assemblies build sạch và 10 EditMode tests pass.
- [ ] Content pass: thay placeholder bằng model, animation layer/blend tree, AudioClip, particle và LOD hoàn chỉnh.
- [ ] Device pass: profile APK ARM64 IL2CPP trên Android tầm trung tại cao trào 120 zombie.

## Roadmap 7 ngày

- [ ] Ngày 1 — Foundation: landscape settings, scene flow, input, soldier, camera và Level 1 graybox.
- [ ] Ngày 2 — Combat: damage, auto-target, rifle, shotgun, switching và pooling.
- [ ] Ngày 3 — Crowd: zombie AI, scheduler, pooling, wave director và hard cap 120.
- [ ] Ngày 4 — Feel: animation layers, hit reaction, dissolve, bomb và recoil.
- [ ] Ngày 5 — Level 1: UI, audio, particle, difficulty curve và mobile layout.
- [ ] Ngày 6 — Level 2: slope, giant, ground slam và Android profiling.
- [ ] Ngày 7 — Polish: balancing, multi-resolution QA, ARM64 IL2CPP và capture.

## Milestone

### M1 — Playable loop
- Soldier di chuyển bằng virtual joystick, camera theo nhân vật.
- Zombie spawn ngoài viewport từ bốn phía, đuổi và tấn công soldier.
- Timer, health, win/lose/restart hoạt động.

### M2 — Combat complete
- Auto-target có line-of-sight và hysteresis.
- Assault Rifle, Shotgun, switch button, projectile/impact feedback.
- Bom nổ theo bán kính, damage falloff và knockback vật lý.

### M3 — Crowd and polish
- Pool prewarm 130; target 100, hard cap 120 zombie.
- AI staggered 10/4/2 Hz theo khoảng cách.
- Landscape UI 16:9, audio mix, hit flash và dissolve.

## Definition of Done

- Không Instantiate/Destroy trong gameplay ổn định và không GC allocation lặp lại.
- Android Development Build duy trì frame budget gần 16,6 ms ở cao trào.
- Code tuân thủ TSC Iron Rules; async chỉ dùng Awaitable.
- Level 1 hoàn chỉnh; Level 2 giữ slope, giant và vòng chơi đầy đủ trong timebox.
