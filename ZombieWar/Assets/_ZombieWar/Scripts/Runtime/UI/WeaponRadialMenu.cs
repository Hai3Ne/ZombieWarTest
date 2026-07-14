using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using ZombieWar.Combat;

namespace ZombieWar.UI
{
    public sealed class WeaponRadialMenu : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _slotsRoot;
        [SerializeField] private Button[] _slotButtons;
        [SerializeField] private Image[] _slotIcons;

        private readonly AsyncOperationHandle<Sprite>[] _iconHandles = new AsyncOperationHandle<Sprite>[3];
        private readonly bool[] _hasIconHandle = new bool[3];
        private readonly Vector3[] _slotBaseScales = new Vector3[3];
        private WeaponController _weapon;

        public void SetViewReferences(Button toggleButton, GameObject slotsRoot, Button[] slotButtons, Image[] slotIcons)
        {
            _toggleButton = toggleButton;
            _slotsRoot = slotsRoot;
            _slotButtons = slotButtons;
            _slotIcons = slotIcons;
            _slotsRoot.SetActive(false);
        }

        public void Initialize(WeaponController weapon)
        {
            _weapon = weapon;
            _toggleButton.onClick.AddListener(Toggle);
            _slotButtons[0].onClick.AddListener(SelectSlotZero);
            _slotButtons[1].onClick.AddListener(SelectSlotOne);
            _slotButtons[2].onClick.AddListener(SelectSlotTwo);
            _weapon.WeaponChanged += OnWeaponChanged;

            for (int i = 0; i < _slotButtons.Length; i++)
            {
                _slotBaseScales[i] = _slotButtons[i].transform.localScale;
                WeaponConfig config = _weapon.GetWeapon(i);
                _slotButtons[i].interactable = config != null;
                _slotIcons[i].enabled = config != null;
                if (config != null)
                {
                    LoadIcon(i, config);
                }
            }
            OnWeaponChanged(_weapon.CurrentWeaponIndex, _weapon.CurrentWeaponName);
        }

        private void OnDestroy()
        {
            if (_toggleButton != null)
            {
                _toggleButton.onClick.RemoveListener(Toggle);
            }
            if (_slotButtons != null && _slotButtons.Length == 3)
            {
                _slotButtons[0].onClick.RemoveListener(SelectSlotZero);
                _slotButtons[1].onClick.RemoveListener(SelectSlotOne);
                _slotButtons[2].onClick.RemoveListener(SelectSlotTwo);
            }
            if (_weapon != null)
            {
                _weapon.WeaponChanged -= OnWeaponChanged;
            }
            for (int i = 0; i < _iconHandles.Length; i++)
            {
                if (_hasIconHandle[i])
                {
                    Addressables.Release(_iconHandles[i]);
                }
            }
        }

        private void LoadIcon(int index, WeaponConfig config)
        {
            if (config.Icon == null || !config.Icon.RuntimeKeyIsValid())
            {
                return;
            }
            AsyncOperationHandle<Sprite> handle = config.Icon.LoadAssetAsync<Sprite>();
            _iconHandles[index] = handle;
            _hasIconHandle[index] = true;
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded && _slotIcons[index] != null)
                {
                    _slotIcons[index].sprite = operation.Result;
                }
            };
        }

        private void Toggle()
        {
            _slotsRoot.SetActive(!_slotsRoot.activeSelf);
        }

        private void SelectSlotZero() => Select(0);
        private void SelectSlotOne() => Select(1);
        private void SelectSlotTwo() => Select(2);

        private void Select(int index)
        {
            _weapon.SelectWeapon(index);
            _slotsRoot.SetActive(false);
        }

        private void OnWeaponChanged(int index, string displayName)
        {
            for (int i = 0; i < _slotButtons.Length; i++)
            {
                _slotButtons[i].transform.localScale = _slotBaseScales[i] * (i == index ? 1.12f : 1f);
            }
        }
    }
}
