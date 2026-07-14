# Zombie War — Game Architecture

## Boundary

Scenes: Boot, MainMenu, Level01, Level02. Một runtime assembly
ZombieWar.Runtime chia theo Core, Player, Combat, Enemies, Levels, UI, Audio,
VFX. Không dùng DI framework, service locator hoặc global event bus.

Boot giữ scene loader và audio root. Level scene có composition root nối
dependency bằng SerializeField. Async scene/fuse chỉ dùng Unity Awaitable.

## Public contracts

- IDamageable.ApplyDamage(in DamageInfo): contract damage chung.
- DamageInfo: amount, hit point, impulse, instigator và DamageType.
- WeaponConfig: fire interval, damage, range, spread, pellets, recoil và VFX.
- EnemyConfig: health, speed, attack, ranges và simulation tier settings.
- WaveConfig/LevelConfig: duration, curve, target count, hard cap và giant cue.

## Runtime flow

PlayerInputReader -> SoldierMotor -> AutoTargeter -> WeaponController.
WaveDirector -> EnemyPool -> EnemySimulationScheduler -> ZombieAgent.
GameSessionController sở hữu timer và state Playing/Won/Lost.

EnemySimulationScheduler chia bucket theo distance: near 10 Hz, mid 4 Hz, far
2 Hz. SetDestination được stagger; movement của NavMeshAgent vẫn nội suy.

Projectile, enemy và VFX dùng pool có capacity xác định. Damage query dùng
RaycastNonAlloc/OverlapSphereNonAlloc và layer mask. MaterialPropertyBlock điều
khiển HitFlash/DissolveAmount, không tạo material per zombie.

## Camera và UI

Cinemachine 3.1.7 follow target với perspective top-down. Framing điều chỉnh
height/FOV theo aspect portrait. Spawn boundary lấy từ viewport rays xuống
ground plane thay vì tọa độ cố định.

uGUI Canvas dùng SafeAreaFitter và CanvasScaler 1080 × 1920. Input System map
gồm Move, SwitchWeapon, ThrowBomb và Pause.

Move dùng floating virtual joystick: vùng input trong suốt chiếm 43% Safe Area
phía dưới, visual xuất hiện tại vị trí touch đã clamp theo kích thước joystick,
theo một pointer duy nhất và tự ẩn/reset khi thả. Các button nằm ở sorting order
cao hơn nên vẫn nhận multi-touch độc lập.

## Physics

Soldier dùng Rigidbody, freeze X/Z rotation. Zombie Rigidbody mặc định
kinematic cùng NavMeshAgent; khi trúng bomb, agent tắt, rigidbody nhận
AddExplosionForce, rồi chết hoặc sample lại NavMesh và trở về chase.

## Verification

EditMode: damage falloff, cooldown, target selection, pacing.
PlayMode: boot, win/lose/restart, pool reset, scene transition.
Android profiler: CPU/GPU gần 16,6 ms, memory ổn định qua ba lượt replay.

## Asset management

Runtime asset nặng được tải bằng Addressables qua catalog ScriptableObject có kiểu.
Mỗi feature dùng group, label và address ổn định; controller giữ handle, cache asset
đã tải và release khi kết thúc lifetime. Gameplay prefab không giữ tham chiếu trực
tiếp tới audio/VFX/model được quản lý bởi Addressables.

`ZombieWar-Audio` dùng LZ4 và Pack Together. Weapon fire audio hiện có địa chỉ
`audio/weapons/rifle/fire` và `audio/weapons/shotgun/fire`, label `audio-weapons`.
Android build phải build Addressables content trước Player build.

`ZombieWar-ZombieAudio` chứa voice zombie theo nhóm ambient, attack, hit và death,
label `audio-zombies`. Các clip chỉ load một lần qua `ZombieAudioService` và phát
bằng 8 AudioSource 3D được author sẵn; cooldown toàn cục giới hạn chồng âm khi có
80–120 zombie.

`ZombieWar-Enemies` chứa prefab zombie hoàn chỉnh tại địa chỉ
`prefabs/enemies/zombie`, label `enemies`. `EnemyPool` tải prefab qua
`EnemyPrefabCatalog`, sau đó mới prewarm 130 instance; `WaveDirector` chỉ bắt đầu
spawn khi pool báo sẵn sàng.

Scene và hierarchy vẫn được dựng sẵn trong Editor. Addressables chỉ chịu trách
nhiệm phân phối asset, không tạo project hoặc dựng cấu trúc scene ở runtime.
