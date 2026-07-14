# TSC Iron Rules

1. SerializeField luôn private; public access qua property.
2. Unity async dùng Awaitable, không IEnumerator.
3. Cache component trong Awake; không GetComponent/Find trong Update.
4. Region order: Config, Refs, State, Lifecycle, API, Internal.
5. Private field dùng _camelCase; public member PascalCase.
6. Dùng TryGetComponent cho dependency và unsubscribe event trong OnDestroy.

