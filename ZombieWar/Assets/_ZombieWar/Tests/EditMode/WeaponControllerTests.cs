using NUnit.Framework;
using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Tests
{
    public sealed class WeaponControllerTests
    {
        [Test]
        public void CalculateDirection_WithNoSpread_FollowsMuzzleForward()
        {
            Quaternion muzzleRotation = Quaternion.Euler(12f, 90f, 0f);

            Vector3 direction = WeaponBallistics.CalculateDirection(muzzleRotation, 0f, 0f);

            Assert.That(Vector3.Angle(direction, muzzleRotation * Vector3.forward), Is.LessThan(0.001f));
        }

        [Test]
        public void SwitchWeapon_CyclesThroughBothAuthoredWeapons()
        {
            GameObject root = new("Weapon Controller Test", typeof(Rigidbody), typeof(WeaponController));
            WeaponConfig rifle = ScriptableObject.CreateInstance<WeaponConfig>();
            WeaponConfig shotgun = ScriptableObject.CreateInstance<WeaponConfig>();
            try
            {
                rifle.Configure("RIFLE", 18f, 0.12f, 20f, 36f, 1, 1.5f, 0.08f, Color.yellow);
                shotgun.Configure("SHOTGUN", 14f, 0.62f, 11f, 30f, 7, 10f, 0.24f, Color.red);
                WeaponController controller = root.GetComponent<WeaponController>();
                controller.Configure(new[] { rifle, shotgun }, null, null, null);

                Assert.That(controller.CurrentWeaponIndex, Is.EqualTo(0));
                Assert.That(controller.CurrentWeaponName, Is.EqualTo("RIFLE"));

                controller.SwitchWeapon();
                Assert.That(controller.CurrentWeaponIndex, Is.EqualTo(1));
                Assert.That(controller.CurrentWeaponName, Is.EqualTo("SHOTGUN"));

                controller.SwitchWeapon();
                Assert.That(controller.CurrentWeaponIndex, Is.EqualTo(0));
                Assert.That(controller.CurrentWeaponName, Is.EqualTo("RIFLE"));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(rifle);
                Object.DestroyImmediate(shotgun);
            }
        }
    }
}
