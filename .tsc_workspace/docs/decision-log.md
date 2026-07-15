# Decision Log

## ADR-001 — Project-owned TSC workspace

.tsc_workspace nằm ở repo root; tsc-unity-kit giữ bất biến dưới dạng submodule.

## ADR-002 — Landscape twin-stick manual fire

Landscape 16:9 dùng hai vùng joystick nổi: bên trái điều khiển di chuyển, bên phải
điều khiển hướng ngắm và giữ để bắn. Thả joystick phải sẽ dừng bắn; Bomb, Switch
và Options được đặt trên vùng aim để giữ ưu tiên touch độc lập.

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

## ADR-006 — Level catalog và wave timeline do dữ liệu điều khiển

Không hardcode số lượng level hoặc thời lượng 180 giây. `LevelCatalogConfig` là nguồn
dữ liệu chính cho thứ tự scene và `WaveSequenceConfig` của từng level; tổng thời lượng
là tổng duration của các wave. Scene và nút menu vẫn được author sẵn trong Editor,
không tạo hierarchy hay project động ở runtime.

## ADR-007 — Chuyển level qua cổng extraction đã author

Hết thời lượng level chỉ mở khóa cổng extraction; không tự động đổi scene. Cổng là
prefab được gắn sẵn vào từng level và chỉ nhận collider của soldier. Khi người chơi
bước vào, game tải scene `Loading`, giữ scene đích bằng `SceneTransitionRequest`, rồi
kích hoạt level enabled kế tiếp trong `LevelCatalogConfig`. Level cuối hiện quay vòng
về level đầu theo semantics của catalog.
