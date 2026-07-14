# Zombie War — Game Design Document

## Tầm nhìn

Zombie War là game survival shooter top-down portrait có nhịp độ dồn dập.
Người chơi tập trung điều hướng, chọn vị trí và thời điểm đổi súng/ném bom;
soldier tự ngắm và tự bắn để thao tác một tay vẫn rõ ràng, phản hồi nhanh.

## Vòng chơi cốt lõi

1. Di chuyển để gom, né và dẫn hướng đám đông.
2. Soldier tự khóa zombie hợp lệ gần nhất và tự bắn.
3. Đổi Rifle/Shotgun theo khoảng cách và mật độ.
4. Ném bom để phá vòng vây hoặc xử lý cụm mục tiêu.
5. Sống sót đủ 180 giây để hoàn thành level.

Soldier chết khi health về 0. Level thắng khi timer về 0. Pause, retry và next
level luôn xuất hiện qua UI, không yêu cầu save progression.

## Điều khiển portrait

- Virtual joystick: góc dưới trái, điều khiển hướng/tốc độ.
- Switch Weapon: góc dưới phải, phía trên nút bom.
- Throw Bomb: góc dưới phải, hiển thị radial cooldown.
- Pause: góc trên phải, tách khỏi vùng thao tác combat.

HUD dùng safe area, reference 1080 × 1920. Health ở trên trái, timer ở giữa,
weapon/ammo state ở trên phải. Hỗ trợ 9:16, 9:18, 9:19.5 và 9:20.

## Soldier và combat

Soldier xoay về auto-target nhưng velocity độc lập, cho phép chạy lùi/strafe.
Base Animator Layer dùng locomotion blend tree; upper-body layer dùng Avatar
Mask cho aim/fire. Damage tạo hit reaction, flash và vignette ngắn.

Auto-target quét theo nhịp 0,15 giây, kiểm tra range và line-of-sight. Mục tiêu
hiện tại được giữ cho đến khi mục tiêu mới tốt hơn rõ rệt hoặc không còn hợp lệ.

### Assault Rifle

- Tầm xa, fire rate cao, projectile/tracer thẳng.
- Damage vừa, spread nhỏ, recoil/camera impulse nhẹ.
- Phù hợp giữ khoảng cách và tiêu diệt mục tiêu đơn.

### Shotgun

- Tầm gần, nhiều pellet, spread rộng.
- Damage burst và recoil mạnh, impact VFX lớn hơn.
- Phù hợp phá cụm zombie đang áp sát.

### Bom

Bom được ném theo hướng auto-target hoặc hướng nhìn hiện tại, fuse 1,5 giây.
Vụ nổ dùng damage falloff, impulse vật lý và cooldown rõ ràng trên HUD.

## Zombie

Zombie thường có Spawn, Chase, Attack, Hit, Knockback và Dead. Mọi zombie dùng
chung soldier target. Hit tạo flash, impact VFX; chết chạy dissolve 0,35–0,6
giây trước khi trở về pool.

Giant zombie xuất hiện ở phút thứ hai Level 2, có health/silhouette lớn và
ground slam được telegraph trước khi gây area damage.

## Level và pacing

### Level 1 — Containment Yard

Arena phẳng, có crate/barricade tạo đường vòng nhưng không tạo ngõ cụt khó đọc.
Mật độ bắt đầu 20–30 và đạt 80–100 zombie.

### Level 2 — Broken Overpass

Địa hình có dốc, nhiều cao độ đọc được bằng màu/ánh sáng. Mật độ đạt 100–120,
kèm một giant và regular wave tiếp tục hoạt động.

Mỗi level: 0–45 giây làm quen; 45–120 tăng áp lực; 120–165 cao trào; 165–180
final surge. Spawn ring luôn nằm ngoài camera viewport và chia bốn sector.

## Art, VFX và audio

Phong cách low-poly stylized, silhouette rõ trên màn hình nhỏ. Soldier dùng màu
lạnh/sáng; zombie dùng xanh bẩn/đỏ; interactable dùng accent vàng cam.

VFX gồm muzzle, tracer, impact, blood, explosion, hit flash và dissolve. Audio
Mixer có Master/Music/SFX; nhạc tăng cường độ theo phase của wave.

## Performance budget

Target 100, hard cap 120 zombie; pool prewarm 130. AI được stagger theo khoảng
cách, collider đơn giản, mesh hai LOD, material dùng chung, Animator culling và
Physics NonAlloc. Ưu tiên giảm shadow/VFX/AI tick trước khi giảm mật độ.

