# TSC Iron Rules

## File và type

- Mỗi file C# chỉ chứa một class chính.
- Tên file phải trùng chính xác với tên class.
- Không gom nhiều `MonoBehaviour`, controller, service hoặc model class vào cùng một script.
- Enum, struct và interface dùng chung chỉ được đặt cùng file khi chúng tạo thành một contract nhỏ, gắn kết trực tiếp; nếu phát triển độc lập thì tách file riêng.
- Nested class chỉ dùng khi hoàn toàn là chi tiết triển khai private của class cha.

1. SerializeField luôn private; public access qua property.
2. Unity async dùng Awaitable, không IEnumerator.
3. Cache component trong Awake; không GetComponent/Find trong Update.
4. Region order: Config, Refs, State, Lifecycle, API, Internal.
5. Private field dùng _camelCase; public member PascalCase.
6. Dùng TryGetComponent cho dependency và unsubscribe event trong OnDestroy.
