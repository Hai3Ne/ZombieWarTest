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

## Physics

Soldier dùng Rigidbody, freeze X/Z rotation. Zombie Rigidbody mặc định
kinematic cùng NavMeshAgent; khi trúng bomb, agent tắt, rigidbody nhận
AddExplosionForce, rồi chết hoặc sample lại NavMesh và trở về chase.

## Verification

EditMode: damage falloff, cooldown, target selection, pacing.
PlayMode: boot, win/lose/restart, pool reset, scene transition.
Android profiler: CPU/GPU gần 16,6 ms, memory ổn định qua ba lượt replay.

