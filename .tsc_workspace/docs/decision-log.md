# Decision Log

## ADR-001 — Project-owned TSC workspace

.tsc_workspace nằm ở repo root; tsc-unity-kit giữ bất biến dưới dạng submodule.

## ADR-002 — Portrait auto-fire

Portrait 9:16–9:20, một joystick, auto-target/auto-fire; người chơi đổi súng và
ném bom chủ động.

## ADR-003 — Dense crowd without DOTS

Phạm vi 80–120 zombie dùng pooled MonoBehaviour/NavMeshAgent và scheduler
staggered. Chưa dùng DOTS vì tăng độ phức tạp vượt nhu cầu hiện tại.

## ADR-004 — Runtime UI

Chọn uGUI vì OnScreenStick/OnScreenButton, safe area và touch HUD phù hợp hơn
UI Toolkit cho vòng chơi này.

## ADR-005 — Addressables cho runtime assets

Audio, prefab, VFX và các asset nặng được đưa vào Addressables theo feature group,
dùng địa chỉ ổn định và catalog ScriptableObject có kiểu rõ ràng. Scene, hierarchy
và composition root vẫn được author trong Editor; không tạo project hay hierarchy
động khi chạy game. Asset gốc từ package bên ngoài phải được duplicate vào
`Assets/_ZombieWar` trước khi đưa vào Addressables để tránh phụ thuộc package nguồn.
