using NUnit.Framework;
using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Tests
{
    public sealed class BombControllerTests
    {
        [Test]
        public void TryAddBomb_ClampsInventoryToThree()
        {
            GameObject container = new("Bomb Test Container");
            GameObject soldier = new("Bomb Test Soldier");
            soldier.transform.SetParent(container.transform);
            soldier.SetActive(false);
            GameObject bombObject = new("Bomb Test Prefab", typeof(Rigidbody), typeof(BombProjectile));
            bombObject.SetActive(false);

            try
            {
                BombController controller = soldier.AddComponent<BombController>();
                controller.SetPrefab(bombObject.GetComponent<BombProjectile>(), 1);
                controller.SetInventory(3, 2);
                soldier.SetActive(true);

                Assert.That(controller.BombCount, Is.EqualTo(2));
                Assert.That(controller.TryAddBomb(5), Is.True);
                Assert.That(controller.BombCount, Is.EqualTo(3));
                Assert.That(controller.TryAddBomb(), Is.False);
                Assert.That(controller.BombCount, Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(container);
                Object.DestroyImmediate(bombObject);
            }
        }
    }
}
